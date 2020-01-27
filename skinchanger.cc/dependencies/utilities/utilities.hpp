#pragma once
#include <cstdint>
#include "../../source-sdk/math/vector3d.hpp"

#define M_PI 3.14159265358979323846

namespace utilities {
	template<typename FuncType>
	__forceinline static FuncType call_virtual(void* ppClass, int index) {
		int* pVTable = *(int**)ppClass;
		int dwAddress = pVTable[index];
		return (FuncType)(dwAddress);
	}
	namespace math {
		template <typename t> t clamp_value(t value, t min, t max) {
			if (value > max) {
				return max;
			}
			if (value < min) {
				return min;
			}
			return value;
		}
	}
	namespace game {
		void* capture_interface(const char* mod, const char* iface);
	}

	struct hud_weapons_t {
		std::int32_t* get_weapon_count() {
			return reinterpret_cast<std::int32_t*>(std::uintptr_t(this) + 0x80);
		}
	};

	void force_update();
	void console_warning(const char * msg, ...);
	float csgo_armor(float damage, int armor_value);
	std::uint8_t* pattern_scan(void* module, const char* signature);
	int epoch_time();
	template<class T>
	static T * find_hud_element(const char * name);
}
