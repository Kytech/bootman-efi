using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BootMan.Util
{
    internal static class WindowsAccessControl
    {
        private static class NativeMethods
        {
            internal static readonly string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
            internal static readonly uint SE_PRIVILEGE_ENABLED = 0x00000002;
            internal static readonly int ERROR_SUCCESS = 0x00000000;

            // Note: This definition is not sutible for use with returning the previous
            // state from AdjustTokenPrivileges as this definition is only capable of
            // holding one privilege. I am doing it this way because it is easier
            // and I don't want to worry about the added complexity for the use case of
            // multiple privileges (either as in or out param)
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            internal readonly struct TokenPrivilege
            {
                private readonly uint _count = 1;
                private readonly ulong _luid;
                private readonly uint _attributes;

                public TokenPrivilege(ulong luid, uint attributes)
                {
                    _luid = luid;
                    _attributes = attributes;
                }
            }

            [DllImport("Kernel32.dll")]
            internal static extern SafeProcessHandle GetCurrentProcess();

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool OpenProcessToken(
                SafeProcessHandle processHandle,
                TokenAccessLevels desiredAccess,
                out SafeAccessTokenHandle tokenHandle
            );

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool LookupPrivilegeValue(
                [Optional] string systemName,
                string name,
                out ulong luid
            );

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool AdjustTokenPrivileges(
                SafeAccessTokenHandle tokenHandle,
                bool disableAllPrivileges,
                [Optional] ref TokenPrivilege newState,
                int newStateBufferLengthInBytes,
                [Optional] IntPtr previousState,
                [Optional] IntPtr returnLengthInBytes
            );
        }

        private static bool _firmwareNvramAccessEnabled = false;

        public static void EnableFirmwareNvramAccess()
        {
            if (!_firmwareNvramAccessEnabled)
            {
                bool success = NativeMethods.LookupPrivilegeValue(name: NativeMethods.SE_SYSTEM_ENVIRONMENT_NAME, luid: out ulong luid);
                if (!success)
                {
                    throw new Win32Exception();
                }

                SafeProcessHandle procHandle = NativeMethods.GetCurrentProcess();
                success = NativeMethods.OpenProcessToken(procHandle, TokenAccessLevels.AdjustPrivileges, out SafeAccessTokenHandle tokenHandle);
                procHandle.Close();
                if (!success)
                {
                    throw new Win32Exception();
                }

                var enableFirmwarePrivilege = new NativeMethods.TokenPrivilege(luid, NativeMethods.SE_PRIVILEGE_ENABLED);
                bool lastErrorNotSet = NativeMethods.AdjustTokenPrivileges(tokenHandle, false, ref enableFirmwarePrivilege, Marshal.SizeOf<NativeMethods.TokenPrivilege>());
                tokenHandle.Close();
                if (!lastErrorNotSet)
                {
                    // TODO: test to see what happens here with a user without sufficient privileges
                    int lastErrCode = Marshal.GetLastWin32Error();
                    if (lastErrCode != NativeMethods.ERROR_SUCCESS)
                    {
                        throw new Win32Exception();
                    }
                }

                _firmwareNvramAccessEnabled = true;
            }
        }

        // TODO: Add method to check if current user has permission to access firmware variables (if needed)
    }
}
