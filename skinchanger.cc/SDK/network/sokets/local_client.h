#pragma once
#include <Windows.h>
#include <string>


class local_client
{
public:
	int NoError = 0;

	local_client(const char* ip, u_short port);
	~local_client();

	bool data_exchange();
	byte* recivepacket(DWORD len, DWORD* lpRecived);
	bool sendpacket(byte* data, DWORD data_size);
private:

	struct SERVER_RESPONSE
	{
		DWORD user_id;
		byte hwid[65];
	};


	WSADATA m_wsaData{};
	SOCKET m_sClient;
	sockaddr_in m_sockAddr{};
	int m_iErrorCode;

};