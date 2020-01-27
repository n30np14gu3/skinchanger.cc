using System;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace skinchanger_loader.SDK.Device
{
    internal static class HWID
    {
        internal static string Sha256(byte[] data) => BitConverter
            .ToString(new SHA256CryptoServiceProvider().ComputeHash(data)).Replace("-", "").ToLower();

        public static string GetHwid() =>
            (from x in new ManagementObjectSearcher("SELECT * FROM Win32_processor").Get().OfType<ManagementObject>()
                select x.GetPropertyValue("ProcessorId")).First().ToString();

        public static string GetUserOs() =>
            (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                select x.GetPropertyValue("Caption")).First().ToString();

        public static string GetHddSerial() =>
            (from x in new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory").Get().OfType<ManagementObject>()
                select x.GetPropertyValue("SerialNumber")).First().ToString();

        public static string GetMacAdress() => NetworkInterface.GetAllNetworkInterfaces()
            .First(x => x.GetPhysicalAddress().ToString() != string.Empty).GetPhysicalAddress().ToString();

        public static string GetSign() => Sha256(Encoding.UTF8.GetBytes($"SKINCHANGER_CC.{GetHddSerial()}.{GetUserOs()}.SKINCHANGER_CC"));
    }
}