using System;
using System.Threading;
using static Rebound.Helpers.Win32;

namespace Rebound.Helpers;

internal class Registry
{
    public const string AccentRegistryKeyPath = @"Software\Microsoft\Windows\DWM";
    public const string AccentRegistryValueName = "ColorPrevalence";

    private const uint REG_NOTIFY_CHANGE_NAME = 0x00000001;
    private const uint REG_NOTIFY_CHANGE_ATTRIBUTES = 0x00000002;
    private const uint REG_NOTIFY_CHANGE_LAST_SET = 0x00000004;
    private const uint REG_NOTIFY_CHANGE_SECURITY = 0x00000008;

    private static readonly ManualResetEvent _changeEvent = new(false);
    private static bool _running = true;
    private static readonly IntPtr HKEY_CURRENT_USER = unchecked((IntPtr)0x80000001);

    private static TitleBarService serv;

    private class RegistryMonitor : IDisposable
    {
        private readonly string _registryKeyPath;
        private IntPtr _hKey;
        private readonly Thread _monitorThread;

        public RegistryMonitor(string registryKeyPath, TitleBarService service)
        {
            serv = service;
            _registryKeyPath = registryKeyPath;
            _monitorThread = new Thread(MonitorRegistry) { IsBackground = true };
        }

        public void Start() => _monitorThread.Start();

        public void Stop()
        {
            _running = false;
            _ = _changeEvent.Set();
            _monitorThread.Join();
        }

        public void MonitorRegistry()
        {
            if (RegOpenKeyEx(HKEY_CURRENT_USER, _registryKeyPath, 0, 0x20019, out _hKey) != 0)
            {
                throw new InvalidOperationException("Failed to open registry key.");
            }

            while (_running)
            {
                // Wait for registry change notification
                if (RegNotifyChangeKeyValue(_hKey, true, REG_NOTIFY_CHANGE_NAME | REG_NOTIFY_CHANGE_ATTRIBUTES | REG_NOTIFY_CHANGE_LAST_SET | REG_NOTIFY_CHANGE_SECURITY, _changeEvent.SafeWaitHandle.DangerousGetHandle(), true) == 0)
                {
                    // Handle registry change
                    if (serv.GetWindow() != null)
                    {
                        serv.CheckFocus();
                    }

                    _ = _changeEvent.WaitOne();
                }
            }

            _ = RegCloseKey(_hKey);
        }

        public void Dispose()
        {
            Stop();
            _changeEvent.Dispose();
        }
    }
}