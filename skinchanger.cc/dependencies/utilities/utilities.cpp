#include "utilities.hpp"
#include "../common_includes.hpp"
#include <psapi.h>



template<class T>
static T* utilities::find_hud_element(const char* name) {
	static auto fn = *reinterpret_cast<DWORD**>(utilities::pattern_scan(GetModuleHandleA("client_panorama.dll"), ("B9 ? ? ? ? E8 ? ? ? ? 8B 5D 08")) + 1);

	static auto find_hud_element = reinterpret_cast<DWORD(__thiscall*)(void*, const char*)>(utilities::pattern_scan(GetModuleHandleA("client_panorama.dll"), ("55 8B EC 53 8B 5D 08 56 57 8B F9 33 F6 39 77 28")));
	return (T*)find_hud_element(fn, name);
}


void utilities::force_update() {
	static auto fn = reinterpret_cast<std::int32_t(__thiscall*)(void*, std::int32_t)>(utilities::pattern_scan(GetModuleHandleA("client_panorama.dll"), ("55 8B EC 51 53 56 8B 75 08 8B D9 57 6B FE 2C")));

	auto element = find_hud_element<std::uintptr_t*>(("CCSGO_HudWeaponSelection"));

	auto hud_weapons = reinterpret_cast<hud_weapons_t*>(std::uintptr_t(element) - 0xA0);
	if (hud_weapons == nullptr)
		return;

	if (!*hud_weapons->get_weapon_count())
		return;

	for (std::int32_t i = 0; i < *hud_weapons->get_weapon_count(); i++)
		i = fn(hud_weapons, i);

	interfaces::clientstate->full_update();
}

void utilities::console_warning(const char* msg, ...) {
	if (msg == nullptr)
		return;
	typedef void(__cdecl* console_warning_fn)(const char* msg, va_list);
	static console_warning_fn fn = (console_warning_fn)GetProcAddress(GetModuleHandle("tier0.dll"), "Warning");
	char buffer[989];
	va_list list;
	va_start(list, msg);
	vsprintf(buffer, msg, list);
	va_end(list);
	fn(buffer, list);
}

float utilities::csgo_armor(float damage, int armor_value) {
	float armor_ratio = 0.5f;
	float armor_bonus = 0.5f;
	if (armor_value > 0) {
		float armor_new = damage * armor_ratio;
		float armor = (damage - armor_new) * armor_bonus;

		if (armor > static_cast<float>(armor_value)) {
			armor = static_cast<float>(armor_value) * (1.f / armor_bonus);
			armor_new = damage - armor;
		}

		damage = armor_new;
	}
	return damage;
}

std::uint8_t* utilities::pattern_scan(void* module, const char* signature) {
	static auto pattern_to_byte = [](const char* pattern) {
		auto bytes = std::vector<int>{};
		auto start = const_cast<char*>(pattern);
		auto end = const_cast<char*>(pattern) + strlen(pattern);

		for (auto current = start; current < end; ++current) {
			if (*current == '?') {
				++current;
				if (*current == '?')
					++current;
				bytes.push_back(-1);
			}
			else {
				bytes.push_back(strtoul(current, &current, 16));
			}
		}
		return bytes;
	};

	auto dos_headers = reinterpret_cast<PIMAGE_DOS_HEADER>(module);
	auto nt_headers = reinterpret_cast<PIMAGE_NT_HEADERS>((std::uint8_t*)module + dos_headers->e_lfanew);

	auto size_of_image = nt_headers->OptionalHeader.SizeOfImage;
	auto pattern_bytes = pattern_to_byte(signature);
	auto scan_bytes = reinterpret_cast<std::uint8_t*>(module);

	auto s = pattern_bytes.size();
	auto d = pattern_bytes.data();

	for (auto i = 0ul; i < size_of_image - s; ++i) {
		bool found = true;
		for (auto j = 0ul; j < s; ++j) {
			if (scan_bytes[i + j] != d[j] && d[j] != -1) {
				found = false;
				break;
			}
		}
		if (found) {
			return &scan_bytes[i];
		}
	}
	return nullptr;
}

int utilities::epoch_time() {
	return std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
}

void* utilities::game::capture_interface(const char* mod, const char* iface) {
	using fn_capture_iface_t = void*(*)(const char*, int*);
	auto fn_addr = reinterpret_cast<fn_capture_iface_t>(GetProcAddress(GetModuleHandleA(mod), "CreateInterface"));
	auto iface_addr = fn_addr(iface, nullptr);
	printf("found %s at 0x%p\n", iface, fn_addr(iface, nullptr));

	return iface_addr;
}
