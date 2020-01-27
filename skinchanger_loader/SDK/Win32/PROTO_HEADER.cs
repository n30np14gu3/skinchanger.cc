using System;
using System.Runtime.InteropServices;

namespace skinchanger_loader.SDK.Win32
{
    [StructLayout(LayoutKind.Sequential, Size = 72), Serializable]
    internal struct PROTO_HEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)]
        public byte[] hash;
        public int key_length;
    }

}