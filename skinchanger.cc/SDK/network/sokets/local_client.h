#pragma once
#include <Windows.h>
#include <string>

class local_client
{
public:
	int NoError = 0;

	local_client(const char* ip, u_short port);
	~local_client();
	bool verification();
private:

	bool sendPacket(byte* packet, int packetSize, bool crypt = false);
	byte* recivePacket(int& packetSize, bool crypt = false);

	WSADATA m_wsaData{};
	SOCKET m_sClient;
	sockaddr_in m_sockAddr{};
	int m_iErrorCode;
	byte* pKey{};
};