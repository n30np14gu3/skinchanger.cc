#include "json_ex.h"

#include <rapidjson/document.h>
#include <rapidjson/writer.h>
#include <rapidjson/stringbuffer.h>

#include "../../../crypto/XorStr.h"

namespace json_ex
{
	rapidjson::GenericStringRef<char> toString(const char* buf)
	{
		return rapidjson::StringRef(buf, std::strlen(buf));
	}
	
	local_proto_request* parse_from_bytes(unsigned const char* json)
	{
		if (json == nullptr)
			return nullptr;

		try
		{
			local_proto_request* result = new local_proto_request();
			rapidjson::Document document;
			document.Parse(reinterpret_cast<const char*>(json));
			result->hwid = std::string(document["hwid"].GetString());
			return  result;
		}
		catch (...)
		{
			return nullptr;
		}
	}

	std::string to_json(const end_response& obj)
	{
		try
		{
			rapidjson::Document json;
			json.SetObject();
			rapidjson::Document::AllocatorType& allocator = json.GetAllocator();

			json.AddMember(toString(XorStr("result")), obj.result, allocator);
			json.AddMember(toString(XorStr("salt")), toString(obj.salt.c_str()), allocator);

			rapidjson::StringBuffer buffer;
			rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
			json.Accept(writer);

			return buffer.GetString();
		}
		catch (...)
		{
			return "";
		}
	}
}