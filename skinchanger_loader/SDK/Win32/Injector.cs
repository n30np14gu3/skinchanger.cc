using System;
using System.IO;
using System.Runtime.InteropServices;

namespace skinchanger_loader.SDK.Win32
{
    internal class Injector
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int Inject(Int32 size, byte[] buffer, Int32 pId, Int32 flags, bool asImage);


        internal static bool ManualMapInject(Int32 size, byte[] buffer, Int32 pId, Int32 flags, bool asImage)
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.WriteAllBytes(tmpFile, Properties.Resources._mmi_);

            IntPtr _pDll = NativeMethods.LoadLibrary(tmpFile);
            IntPtr pAddr = NativeMethods.GetProcAddress(_pDll, "_InjectRaw@20");
            Inject inject = (Inject) Marshal.GetDelegateForFunctionPointer(pAddr, typeof(Inject));

            int result = inject(size, buffer, pId, flags, asImage);

            NativeMethods.FreeLibrary(_pDll);

            File.Delete(tmpFile);

            return result == 0;
        }
    }
}