#pragma once
#include <string>

namespace globals
{
	using namespace  std;
	extern string hwid;
	extern int last_update;
	extern int code;
	void initGlobals();
}