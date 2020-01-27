#include <WinSock.h>

#include "local_client.h"

#include "../../crypto/hash/sha256.hpp"
#include "../../crypto/XorStr.h"
#include "../../globals/globals.h"

#include <fstream>

#pragma comment(lib, "Ws2_32.lib")


std::ifstream::pos_type filesize(const char* filename)
{
	std::ifstream in(filename, std::ifstream::ate | std::ifstream::binary);
	return in.tellg();
}

local_client::local_client(const char* ip, u_short port)
{
	m_sClient = 0;
	m_iErrorCode = 0;

	NoError = !WSAStartup(MAKEWORD(2, 2), &m_wsaData);
	if(!NoError)
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
}

local_client::~local_client()
{
	NoError = 0;
	m_sClient = 0;
	m_iErrorCode = 0;
	ZeroMemory(&m_sockAddr, sizeof(sockaddr_in));
	closesocket(m_sClient);
	WSACleanup();

}



bool local_client::data_exchange()
{

#if !NDEBUG
	MessageBox(nullptr, ("Get Length" + std::to_string(sizeof(SERVER_RESPONSE))).c_str(), "", MB_OK);
#endif
	//RECV KEYS & DECRYPT
	DWORD recived = 0;
	DWORD* encryptedSize = reinterpret_cast<DWORD*>(recivepacket(sizeof(DWORD), &recived));
	if(recived != 4)
		return false;

	DWORD encryptedRecv = 0;

#if !NDEBUG
	MessageBox(nullptr, "Get Data", "", MB_OK);
#endif

	byte* encryptedData = recivepacket(*encryptedSize, &encryptedRecv);
	if(*encryptedSize != encryptedRecv)
		return false;

#if !NDEBUG
	MessageBox(nullptr, "Decrypting", "", MB_OK);
#endif

	SERVER_RESPONSE* response = (SERVER_RESPONSE*)encryptedData;

	for (unsigned i = 0; i < *encryptedSize; i++)
		encryptedData[i] ^= 0xAC;

	//STORE DATA
	globals::hwid = std::string(reinterpret_cast<char*>(response->hwid));

	return true;
}

byte* local_client::recivepacket(DWORD len, DWORD* lpRecived)
{
	byte* buff = new byte[len];
	memset(buff, 0, len);
	*lpRecived = recv(m_sClient, reinterpret_cast<char*>(buff), len, 0);
	return buff;
}

bool local_client::sendpacket(byte* data, DWORD data_size)
{
	return  static_cast<DWORD>(send(m_sClient, reinterpret_cast<const char*>(data), data_size, 0)) == data_size;
}
