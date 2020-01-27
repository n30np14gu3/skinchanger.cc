#include <Windows.h>
#include <WinInet.h>
#include "http_request.h"
#include "../../crypto/XorStr.h"

#pragma comment (lib, "Wininet.lib")



namespace http_request
{
	string post(
		const string& server,
		const string& route,
		const string& user_agent,
		const string& params,
		bool ssl
	)
	{
		HINTERNET hInternet = InternetOpen(user_agent.c_str(), INTERNET_OPEN_TYPE_PRECONFIG, nullptr, nullptr, 0);

		if (hInternet != nullptr)
		{
			HINTERNET hConnect = InternetConnect(hInternet, server.c_str(), ssl ? INTERNET_DEFAULT_HTTPS_PORT : INTERNET_DEFAULT_HTTP_PORT, nullptr, nullptr, INTERNET_SERVICE_HTTP, 0, 0);
			if (hConnect != nullptr)
			{
				HINTERNET hRequest = HttpOpenRequest(hConnect, XorStr("POST"), route.c_str(), nullptr, nullptr, nullptr, ssl ? (INTERNET_FLAG_SECURE | INTERNET_FLAG_KEEP_CONNECTION) : INTERNET_SERVICE_HTTP, 0);
				if (hRequest != nullptr)
				{
					string heades = XorStr("Content-Type: application/x-www-form-urlencoded\r\n");
					if (HttpSendRequest(hRequest, heades.c_str(), heades.length(), LPVOID(TEXT(params.c_str())), params.length()))
					{
						unsigned char c = 0;
						string response = "";
						for (;;)
						{
							DWORD readed = 0;
							BOOL isRead = InternetReadFile(hRequest, LPVOID(&c), 1, &readed);
							if (readed == 0 || !isRead)
								break;

							response += c;
						}
						response += '\0';
						InternetCloseHandle(hRequest);
						InternetCloseHandle(hConnect);
						InternetCloseHandle(hInternet);
						return  response;
					}
				}
			}
			InternetCloseHandle(hConnect);
			InternetCloseHandle(hInternet);
		}
		InternetCloseHandle(hInternet);

		return "";
	}

	string get(
		const string& server,
		const string& route,
		const string& user_agent,
		const string& params,
		bool ssl
	)
	{
		HINTERNET hInternet = InternetOpen(user_agent.c_str(), INTERNET_OPEN_TYPE_PRECONFIG, nullptr, nullptr, 0);

		if (hInternet != nullptr)
		{
			HINTERNET hConnect = InternetConnect(hInternet, server.c_str(), ssl ? INTERNET_DEFAULT_HTTPS_PORT : INTERNET_DEFAULT_HTTP_PORT, nullptr, nullptr, INTERNET_SERVICE_HTTP, 0, 0);
			if (hConnect != nullptr)
			{
				HINTERNET hRequest = HttpOpenRequest(hConnect, XorStr("GET"), (route + "?" + params).c_str(), nullptr, nullptr, nullptr, ssl ? (INTERNET_FLAG_SECURE | INTERNET_FLAG_KEEP_CONNECTION) : INTERNET_SERVICE_HTTP, 0);
				if (hRequest != nullptr)
				{
					if (HttpSendRequest(hRequest, nullptr, 0, nullptr, 0))
					{
						unsigned char c = 0;
						string response;
						for (;;)
						{
							DWORD readed = 0;
							BOOL isRead = InternetReadFile(hRequest, LPVOID(&c), 1, &readed);
							if (readed == 0 || !isRead)
								break;

							response += c;
						}
						response += '\0';
						InternetCloseHandle(hRequest);
						InternetCloseHandle(hConnect);
						InternetCloseHandle(hInternet);
						return  response;
					}
				}
			}
			InternetCloseHandle(hConnect);
			InternetCloseHandle(hInternet);
		}
		InternetCloseHandle(hInternet);

		return "";
	}
}