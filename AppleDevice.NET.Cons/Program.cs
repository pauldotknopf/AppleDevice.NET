using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace AppleDevice.NET.Cons
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct AMDeviceNotificationCallbackInfo
        {
            public IntPtr dev
            {
                get
                {
                    return dev_ptr;
                }
            }
            private IntPtr dev_ptr;
            public int msg;
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void DeviceNotificationCallback(ref AMDeviceNotificationCallbackInfo callback_info);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void DeviceRestoreNotificationCallback(IntPtr callback_info);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static int AMDeviceNotificationSubscribe(DeviceNotificationCallback callback, uint unused1, uint unused2, uint unused3, out IntPtr am_device_notification_ptr);

        [DllImport("iTunesMobileDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static int AMRestoreRegisterForDeviceNotifications(
            DeviceRestoreNotificationCallback dfu_connect,
            DeviceRestoreNotificationCallback recovery_connect,
            DeviceRestoreNotificationCallback dfu_disconnect,
            DeviceRestoreNotificationCallback recovery_disconnect,
            uint unknown0,
            IntPtr user_info);

        static void dfuConnect(IntPtr usbDev)
        {
            Console.WriteLine("Device connected in DFU Mode");
        }

        static void dfuDisconnect(IntPtr usbDev)
        {
            Console.WriteLine("Device exited DFU Mode");
        }

        static void recoveryConnect(IntPtr usbDev)
        {
            Console.WriteLine("Device connected in Recovery Mode");
        }

        static void recoveryDisconnect(IntPtr usbDev)
        {
            Console.WriteLine("Device exited Recovery Mode");
        }

        static void usbMuxMode(ref AMDeviceNotificationCallbackInfo callback_info)
        {
            if (callback_info.msg == 1)
            {
                Console.WriteLine("Device connected in Usb Multiplexing mode");
                DoMyCode();
            }
            else if (callback_info.msg == 2)
                Console.WriteLine("Device disconnected when in Usb Multiplexing mode");
            else
                Console.WriteLine("Device in unknown usbmux mode");
            IntPtr devHandle = callback_info.dev;
        }

        static void DoMyCode()
        {

        }
        public static int Main(string[] args)
        {
            if (!Initialization.AttachITunes())
            {
                Console.Error.WriteLine("Failed to attach iTunes!");
                return -1;
            }
            IntPtr am_device_notification;

            AMDeviceNotificationSubscribe(usbMuxMode, 0, 0, 0, out am_device_notification);
            AMRestoreRegisterForDeviceNotifications(dfuConnect, recoveryConnect, dfuDisconnect, recoveryDisconnect, 0, IntPtr.Zero);
            Thread.Sleep(-1);
            return 0;
        }
    }
}
