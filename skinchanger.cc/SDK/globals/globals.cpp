#include "globals.h"
#include "../crypto/XorStr.h"
namespace globals
{
	string hwid;
	int last_update = 1579392001;
	int code;
	
	void initGlobals()
	{
		last_update = 0;
		code = 0;
	}
}