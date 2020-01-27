#define _SILENCE_CXX17_ITERATOR_BASE_CLASS_DEPRECATION_WARNING
#define _SILENCE_ALL_CXX17_DEPRECATION_WARNINGS


#include "../../rapidjson/document.h"
#include "../../crypto/XorStr.h"
#include "../../globals/globals.h"
#include "../http/http_request.h"
#include <thread>

void ping()
{
	while(1)
	{
		std::string requestParams = std::string(XorStr("hwid=")) + globals::hwid;
		std::string postData = http_request::post(std::string(XorStr("skinchanger.cc")), std::string(XorStr("/api/software/ping")), std::string(XorStr("CKINCHANGER_LIB")), requestParams, true);
		rapidjson::Document responcse;
		responcse.Parse(postData.c_str());

		if (responcse[XorStr("status")].GetInt() != 1)
			exit(0);

		std::this_thread::sleep_for(std::chrono::milliseconds(15000));
	}
}
