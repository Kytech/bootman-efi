using System.Runtime.InteropServices;

namespace BootMan
{
    internal sealed class Firmware
    {
        public enum FirmwareType
        {
            FirmwareTypeUnknown,
            FirmwareTypeBios,
            FirmwareTypeUefi,
            FirmwareTypeMax,
        }

        private static readonly string EFI_GLOBAL_VARIABLE = "{8BE4DF61-93CA-11D2-AA0D-00E098032B8C}";

        [DllImport("Kernel32.dll")]
        private static extern bool GetFirmwareType(out FirmwareType firmwareType);

        // TODO: Refactor these to use Span<uint16> instead
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

        public static FirmwareType GetFirmwareType()
        {
            bool success = Firmware.GetFirmwareType(out FirmwareType firmwareType);
            return success ? firmwareType : FirmwareType.FirmwareTypeUnknown;
        }
    }
}
