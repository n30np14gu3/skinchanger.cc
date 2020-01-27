using System;
using System.Runtime.InteropServices;

namespace skinchanger_loader.SDK.Win32
{
    [StructLayout(LayoutKind.Sequential, Size = 72), Serializable]
    struct SERVER_RESPONSE
    {
        public int user_id;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
        public byte[] access_token;
    }
}