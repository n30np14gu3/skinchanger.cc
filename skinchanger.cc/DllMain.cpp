#include "dependencies/common_includes.hpp"
#include "core/features/skinchanger/knifehook.hpp"
#include "SDK/crypto/XorStr.h"
#include "SDK/network/sokets/local_client.h"
#include "SDK/network/ping/ping.h"
#include "SDK/globals/globals.h"


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

void init()
{
	local_client client(XorStr("127.0.0.1"), 1358);
	CreateThread(0, 0, stub, nullptr, 0, 0);
	if (!client.data_exchange())
	{
		TerminateProcess(GetCurrentProcess(), 0);
	}
	CreateThread(nullptr, 0, reinterpret_cast<LPTHREAD_START_ROUTINE>(ping), nullptr, 0, nullptr);
}

unsigned long __stdcall initial_thread(void* reserved) 
{
	init();

	interfaces::initialize();
	hooks::initialize();
	config_system.item.skinchanger_enable = true;
	config_system.item.glovechanger_enable = true;
	//config_system.run("1337");
	knife_hook.knife_animation();
	Beep(500, 500);
	while(1)
	{
		std::this_thread::sleep_for(std::chrono::milliseconds(100000));
	}
	hooks::shutdown();
	std::this_thread::sleep_for(std::chrono::milliseconds(100));

	FreeLibraryAndExitThread(reinterpret_cast<HMODULE>(reserved), 0);
}

bool __stdcall DllMain(void* instance, unsigned long reason_to_call, void* reserved) 
{
	if (reason_to_call == DLL_PROCESS_ATTACH) {
		CreateThread(0, 0, initial_thread, instance, 0, 0);
		CreateThread(0, 0, stub, nullptr, 0, 0);
	}
	return true;
}
