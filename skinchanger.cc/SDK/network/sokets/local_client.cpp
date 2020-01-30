#pragma comment(lib, "Ws2_32.lib")

#include <winsock.h>

#include "local_client.h"
#include <wincrypt.h>

#include "../../crypto/XorStr.h"
#include "../../crypto/hash/sha256.hpp"
#include "../../globals/globals.h"
#include "json_ex/json_ex.h"



local_client::local_client(const char* ip, u_short port)
{
	m_sClient = 0;
	m_iErrorCode = 0;

	NoError = !WSAStartup(MAKEWORD(2, 2), &m_wsaData);
	if (!NoError)
		return;

	m_sClient = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	ZeroMemory(&m_sockAddr, sizeof(sockaddr_in));
	m_sockAddr.sin_family = AF_INET;
	m_sockAddr.sin_addr.s_addr = inet_addr(ip);
	m_sockAddr.sin_port = htons(port);

	m_iErrorCode = connect(m_sClient, reinterpret_cast<sockaddr*>(&m_sockAddr), sizeof(m_sockAddr));

	NoError = m_iErrorCode != SOCKET_ERROR;
	if (!NoError)
	{
		closesocket(m_sClient);
		WSACleanup();
	}
	pKey = new byte[8];
	memset(pKey, 0, 8);

	HCRYPTPROV hCryptCtx = NULL;
	CryptAcquireContext(&hCryptCtx, NULL, MS_DEF_PROV, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT);
	CryptGenRandom(hCryptCtx, 8, pKey);
	CryptReleaseContext(hCryptCtx, 0);
	send(m_sClient, reinterpret_cast<const char*>(pKey), 8, 0);

}

local_client::~local_client()
{
	closesocket(m_sClient);
	WSACleanup();
	NoError = 0;
	m_sClient = 0;
	m_iErrorCode = 0;
	ZeroMemory(&m_sockAddr, sizeof(sockaddr_in));
	ZeroMemory(pKey, 8);
	delete[] pKey;
}

bool local_client::verification()
{
	int jsonSize = 0;
	byte* userData = recivePacket(jsonSize, true);
	json_ex::local_proto_request* req = json_ex::parse_from_bytes(userData);
	if (req == nullptr)
		return true;

	json_ex::end_response rsp_obj{};
	for (int i = 0; i < 8; i++)
		rsp_obj.result += pKey[i] * 3;

	rsp_obj.salt = sha256(pKey, 8);
	std::string rsp = to_json(rsp_obj);
	if (rsp.empty())
		return false;

	byte* rspBytes = new byte[rsp.length()];
	memcpy(rspBytes, rsp.c_str(), rsp.length());
	if (!sendPacket(rspBytes, rsp.length(), true))
		return false;
	globals::hwid = req->hwid;
	return true;
}

bool local_client::sendPacket(byte* packet, int packetSize, bool crypt)
{
	if (packet == nullptr)
		return false;

	byte packetSizePtr[4]{};
	memcpy(packetSizePtr, &packetSize, 4);;

	if (crypt)
	{
		for (int i = 0; i < 4; i++)
			packetSizePtr[i] ^= pKey[i % 8];
	}

	if (send(m_sClient, reinterpret_cast<const char*>(packetSizePtr), 4, 0) != 4)
		return false;

	if (crypt)
	{
		for (int i = 0; i < packetSize; i++)
			packet[i] ^= pKey[i % 8];
	}

	if (send(m_sClient, reinterpret_cast<const char*>(packet), packetSize, 0) != packetSize)
		return false;


	return true;
}

byte* local_client::recivePacket(int& packetSize, bool crypt)
{
	byte packetSizePtr[4]{};

	if (recv(m_sClient, reinterpret_cast<char*>(packetSizePtr), 4, 0) != 4)
		return nullptr;

	if (crypt)
	{
		for (int i = 0; i < 4; i++)
			packetSizePtr[i] ^= pKey[i % 8];
	}

	packetSize = reinterpret_cast<int&>(packetSizePtr);

	byte* packet = new byte[packetSize];
	memset(packet, 0, packetSize);

	if (recv(m_sClient, (char*)packet, packetSize, 0) != packetSize)
		return nullptr;

	if (crypt)
	{
		for (int i = 0; i < packetSize; i++)
			packet[i] ^= pKey[i % 8];
	}

	return packet;
}