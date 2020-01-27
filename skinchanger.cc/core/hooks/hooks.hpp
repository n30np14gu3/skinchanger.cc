#pragma once
enum ClientFrameStage_t 
{
	FRAME_UNDEFINED = -1,			// (haven't run any frames yet)
	FRAME_START,

	// A network packet is being recieved
	FRAME_NET_UPDATE_START,
	// Data has been received and we're going to start calling PostDataUpdate
	FRAME_NET_UPDATE_POSTDATAUPDATE_START,
	// Data has been received and we've called PostDataUpdate on all data recipients
	FRAME_NET_UPDATE_POSTDATAUPDATE_END,
	// We've received all packets, we can now do interpolation, prediction, etc..
	FRAME_NET_UPDATE_END,

	// We're about to start rendering the scene
	FRAME_RENDER_START,
	// We've finished rendering the scene.
	FRAME_RENDER_END
};

namespace hooks 
{
	extern WNDPROC wndproc_original;
	extern HWND window;

	extern std::unique_ptr<vmt_hook> client_hook;
	extern std::unique_ptr<vmt_hook> clientmode_hook;


	void initialize();
	void shutdown();

	
	using frame_stage_notify_fn = void(__thiscall*)(i_base_client_dll*, int);
	using create_move_fn = bool(__thiscall*)(i_client_mode*, float, c_usercmd*);

	void __stdcall frame_stage_notify(int frame_stage);
	bool __stdcall create_move(float frame_time, c_usercmd* user_cmd);

	LRESULT __stdcall wndproc(HWND hwnd, UINT message, WPARAM wparam, LPARAM lparam);

}