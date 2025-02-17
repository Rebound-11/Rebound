using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace Rebound.Helpers;

public static class Win32
{
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr RegOpenKeyEx(IntPtr hKey, string lpSubKey, uint ulOptions, uint samDesired, out IntPtr phkResult);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern int RegCloseKey(IntPtr hKey);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree, uint dwNotifyFilter, IntPtr hEvent, bool fAsynchronous);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    public const int HORZRES = 8; // Horizontal width of the display
    public const int VERTRES = 10; // Vertical height of the display
    public const int LOGPIXELSX = 88; // Logical pixels/inch in X

    [DllImport("dwmapi.dll", SetLastError = true)]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    public static void SetDarkMode(WindowEx window, Application app)
    {
        var i = 1;
        if (app.RequestedTheme == ApplicationTheme.Light)
        {
            i = 0;
        }
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        _ = DwmSetWindowAttribute(hWnd, 20, ref i, sizeof(int));
        CheckTheme();
        async void CheckTheme()
        {
            await Task.Delay(100);
            try
            {
                if (app != null)
                {
                    var i = 1;
                    if (app.RequestedTheme == ApplicationTheme.Light)
                    {
                        i = 0;
                    }
                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    _ = DwmSetWindowAttribute(hWnd, 20, ref i, sizeof(int));
                    CheckTheme();
                }
            }
            catch
            {

            }
        }
    }

    public static int GET_X_LPARAM(IntPtr lParam) => unchecked((short)(long)lParam);

    public static int GET_Y_LPARAM(IntPtr lParam) => unchecked((short)((long)lParam >> 16));
}
