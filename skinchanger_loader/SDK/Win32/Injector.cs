using System;
using System.IO;
using System.Runtime.InteropServices;

namespace skinchanger_loader.SDK.Win32
{
    internal class Injector
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private delegate bool Inject(Int32 pId, byte[] pDll);


        internal static bool ManualMapInject(Int32 pId, byte[] pDll)
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.WriteAllBytes(tmpFile, Properties.Resources._mmi_);

            IntPtr _pDll = NativeMethods.LoadLibrary(tmpFile);
            IntPtr pAddr = NativeMethods.GetProcAddress(_pDll, "_Inject@8");
            Inject inject = (Inject) Marshal.GetDelegateForFunctionPointer(pAddr, typeof(Inject));

            bool result = inject(pId, pDll);

            NativeMethods.FreeLibrary(_pDll);

            File.Delete(tmpFile);

            return result;
        }
    }
}