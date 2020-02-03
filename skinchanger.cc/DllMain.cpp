#include "dependencies/common_includes.hpp"
#include "core/features/skinchanger/knifehook.hpp"
#include "SDK/crypto/XorStr.h"
#include "SDK/network/sokets/local_client.h"
#include "SDK/globals/globals.h"
#include "SDK/network/websockets/ws.h"

unsigned long __stdcall stub(void* reserved)
{
	int seconds = 0;
	while (1)
	{
		seconds++;
		if (seconds > 15 && globals::hwid.empty())
			TerminateProcess(GetCurrentProcess(), 0);
		else
		{
			if (!globals::hwid.empty())
				break;
		}
		Sleep(1000);
	}
	return 0;
}

void ws_thread()
{
#if DEBUG
	MessageBox(nullptr, "WS CONNECTING", "", MB_OK);
#endif
	
	ws::init_client(XorStr("95.183.12.141"), XorStr("3019"), XorStr("/api/software_v2"));
	Sleep(INFINITE);
}


void init()
{
#if DEBUG
	MessageBox(nullptr, "Injected", "", MB_OK);
#endif
	local_client client(XorStr("127.0.0.1"), 1338);
	if (!client.verification())
	{
		TerminateProcess(GetCurrentProcess(), 0);
	}
	CreateThread(nullptr, 0, reinterpret_cast<LPTHREAD_START_ROUTINE>(ws_thread), nullptr, 0, nullptr);
}

void _stdcall initial_thread() 
{
	init();

	interfaces::initialize();
	hooks::initialize();
	config_system.item.skinchanger_enable = true;
	config_system.item.glovechanger_enable = true;
	knife_hook.knife_animation();
	Beep(500, 500);
}

bool __stdcall DllMain(void* instance, unsigned long reason_to_call, void* reserved) 
{
	if (reason_to_call == DLL_PROCESS_ATTACH) 
	{
		CreateThread(nullptr, 0, reinterpret_cast<LPTHREAD_START_ROUTINE>(initial_thread), nullptr, 0, nullptr);

#if !DEBUG
		CreateThread(0, 0, stub, nullptr, 0, 0);
#endif
		
	}
	return true;
}
