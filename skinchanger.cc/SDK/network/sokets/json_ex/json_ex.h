#pragma once
#include <string>

namespace json_ex
{
	struct local_proto_request
	{
		std::string hwid;
	};

	struct end_response
	{
		int result;
		std::string salt;
	};

	local_proto_request* parse_from_bytes(unsigned const char* json);
	std::string to_json(const end_response& obj);
}