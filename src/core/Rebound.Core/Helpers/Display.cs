using Windows.Foundation;
using WinUIEx;

namespace Rebound.Helpers;

public static class Display
{
    public static double Scale(WindowEx win)
    {
        // Get the handle to the current window
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(win);

        // Get the device context for the window
        var hdc = Win32.GetDC(hWnd);

        // Get the DPI
        var dpiX = Win32.GetDeviceCaps(hdc, Win32.LOGPIXELSX);

        // Release the device context
        _ = Win32.ReleaseDC(hWnd, hdc);

        return dpiX / 96.0;
    }

    public static Rect GetDisplayRect(WindowEx win)
    {
        // Get the handle to the current window
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(win);

        // Get the device context for the window
        var hdc = Win32.GetDC(hWnd);

        // Get the width and height of the display
        var width = Win32.GetDeviceCaps(hdc, Win32.HORZRES);
        var height = Win32.GetDeviceCaps(hdc, Win32.VERTRES);

        // Release the device context
        _ = Win32.ReleaseDC(hWnd, hdc);

        return new Rect()
        {
            X = 0,
            Y = 0,
            Width = width,
            Height = height
        };
    }

    public static Rect GetDPIAwareDisplayRect(WindowEx win)
    {
        // Get the handle to the current window
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(win);

        // Get the device context for the window
        var hdc = Win32.GetDC(hWnd);

        // Get the width and height of the display
        var width = Win32.GetDeviceCaps(hdc, Win32.HORZRES) / Scale(win);
        var height = Win32.GetDeviceCaps(hdc, Win32.VERTRES) / Scale(win);

        // Release the device context
        _ = Win32.ReleaseDC(hWnd, hdc);

        return new Rect()
        {
            X = 0,
            Y = 0,
            Width = width,
            Height = height
        };
    }
}
