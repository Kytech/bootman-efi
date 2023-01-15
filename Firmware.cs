using System;
using System.Runtime.InteropServices;

namespace BootMan
{
    internal static class Firmware
    {
        public enum FirmwareType
        {
            FirmwareTypeUnknown,
            FirmwareTypeBios,
            FirmwareTypeUefi,
            FirmwareTypeMax,
        }

        public static readonly string EFI_GLOBAL_VARIABLE = "{8BE4DF61-93CA-11D2-AA0D-00E098032B8C}";

        [DllImport("Kernel32.dll")]
        private static extern bool GetFirmwareType(out FirmwareType firmwareType);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetFirmwareEnvironmentVariable(
            string lpName,
            string lpGuid,
            byte[] lpBuffer,
            uint nSize
        );

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetFirmwareEnvironmentVariable(
            string lpName,
            string lpGuid,
            byte[] lpBuffer,
            uint nSize
        );

        public static unsafe T GetFirmwareEnvironmentVariable<T>(string name, string guid) where T : unmanaged
        {
            var buffer = new byte[sizeof(T)];
            uint size = GetFirmwareEnvironmentVariable(name, guid, buffer, (uint)buffer.Length);
            if (size == 0)
            {
                throw new Exception($"GetFirmwareEnvironmentVariable failed with error code {Marshal.GetLastPInvokeError()}");
            }
            return MemoryMarshal.Cast<byte, T>(buffer)[0];
        }

        public static FirmwareType GetFirmwareType()
        {
            // TODO: Do error handling to indicate win32 error
            bool success = GetFirmwareType(out FirmwareType firmwareType);
            return success ? firmwareType : FirmwareType.FirmwareTypeUnknown;
        }
    }
}
