using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Rebound.Generators;
using Rebound.Helpers.Services;
using Rebound.Shell.Desktop;
using Windows.Storage;
using WinUIEx;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace Rebound.Shell.ExperienceHost;

[ReboundApp("Rebound.ShellExperienceHost", "Legacy Shell")]
public partial class App : Application
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, uint wParam, int lParam, uint fuFlags, uint uTimeout, out uint lpdwResult);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private const uint WM_SPAWN_WORKER = 0x052C;
    private const int SW_HIDE = 0;
    private const int GWLP_HWNDPARENT = -8;

    public App()
    {
        InitializeComponent();
        // Desktop
        Run();
    }

    private async void Run()
    {
        // Background window
        BackgroundWindow = new WindowEx();
        BackgroundWindow = new() { SystemBackdrop = new TransparentTintBackdrop(), IsMaximizable = true };
        BackgroundWindow.SetExtendedWindowStyle(ExtendedWindowStyle.ToolWindow);
        BackgroundWindow.SetWindowStyle(WindowStyle.Visible);
        BackgroundWindow.Activate();
        BackgroundWindow.MoveAndResize(0, 0, 0, 0);
        BackgroundWindow.Minimize();
        BackgroundWindow.SetWindowOpacity(0);

        // Desktop window
        DesktopWindow = new DesktopWindow();
        DesktopWindow.SetWindowOpacity(0);
        DesktopWindow.Activate();
        await Task.Delay(1000);
        DesktopWindow.AttachToProgMan();

        // Automated window parenting process
        await Task.Run(() =>
        {
            try
            {
                // Find Progman window
                IntPtr hProgman = FindWindow("Progman", null);
                Debug.WriteLine($"Found Progman: 0x{hProgman.ToInt64():X}");

                if (hProgman == IntPtr.Zero)
                {
                    throw new Exception("Failed to find Progman window");
                }

                // Send message to create WorkerW
                uint result;
                SendMessageTimeout(hProgman, WM_SPAWN_WORKER, 0, 0, 0x0, 1000, out result);

                // Give Windows time to create the WorkerW window
                Task.Delay(1000).Wait();

                // Find WorkerW (both inside and outside Progman)
                IntPtr hWorkerW = FindWorkerW(hProgman);
                if (hWorkerW == IntPtr.Zero)
                {
                    throw new Exception("Failed to find WorkerW window");
                }
                Debug.WriteLine($"Found WorkerW: 0x{hWorkerW.ToInt64():X}");

                // Find and hide SysListView32
                IntPtr hSysListView32 = FindWindowEx(hProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (hSysListView32 != IntPtr.Zero)
                {
                    hSysListView32 = FindWindowEx(hSysListView32, IntPtr.Zero, "SysListView32", null);
                    if (hSysListView32 != IntPtr.Zero)
                    {
                        ShowWindow(hSysListView32, SW_HIDE);
                        Debug.WriteLine($"Hidden SysListView32: 0x{hSysListView32.ToInt64():X}");
                    }
                }

                // Get the handle of the DesktopWindow
                IntPtr hReboundDesktop = DesktopWindow.GetWindowHandle();
                Debug.WriteLine($"Rebound Desktop handle: 0x{hReboundDesktop.ToInt64():X}");

                // Verify if the Rebound Desktop window is valid
                if (!IsWindowVisible(hReboundDesktop))
                {
                    throw new Exception("The Rebound Desktop window is not visible or valid.");
                }

                // Print window info
                PrintWindowInfo(hWorkerW, "WorkerW");
                PrintWindowInfo(hReboundDesktop, "Rebound Desktop");

                // Get current parent of Rebound Desktop
                IntPtr currentParent = GetParent(hReboundDesktop);
                Debug.WriteLine($"Current parent of Rebound Desktop: 0x{currentParent.ToInt64():X}");

                // Try to set WorkerW as the parent of Rebound Desktop using SetParent
                IntPtr setParentResult = SetParent(hReboundDesktop, hWorkerW);
                if (setParentResult == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"SetParent failed. Error: {error}");

                    // Try alternative method using SetWindowLongPtr
                    IntPtr setWindowLongResult = SetWindowLongPtr(hReboundDesktop, GWLP_HWNDPARENT, hWorkerW);
                    if (setWindowLongResult == IntPtr.Zero)
                    {
                        error = Marshal.GetLastWin32Error();
                        throw new Exception($"Both SetParent and SetWindowLongPtr failed. Error: {error}");
                    }
                    else
                    {
                        Debug.WriteLine("Successfully set WorkerW as parent of Rebound Desktop using SetWindowLongPtr!");
                    }
                }
                else
                {
                    Debug.WriteLine("Successfully set WorkerW as parent of Rebound Desktop using SetParent!");
                }

                // Verify the new parent
                IntPtr newParent = GetParent(hReboundDesktop);
                Debug.WriteLine($"New parent of Rebound Desktop: 0x{newParent.ToInt64():X}");

                if (newParent == hWorkerW)
                {
                    Debug.WriteLine("Verification successful: WorkerW is now the parent of Rebound Desktop.");
                }
                else
                {
                    Debug.WriteLine("Warning: The parent window doesn't match the expected WorkerW handle.");
                }

                Debug.WriteLine("\nWindow handles summary:");
                Debug.WriteLine($"WorkerW: 0x{hWorkerW.ToInt64():X}");
                Debug.WriteLine($"Rebound Desktop: 0x{hReboundDesktop.ToInt64():X}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        });
    }

    private IntPtr FindWorkerW(IntPtr hProgman)
    {
        IntPtr hWorkerW = IntPtr.Zero;

        // First, try to find WorkerW inside Progman
        hWorkerW = FindWindowEx(hProgman, IntPtr.Zero, "WorkerW", null);
        if (hWorkerW != IntPtr.Zero)
        {
            Debug.WriteLine("Found WorkerW inside Progman");
            return hWorkerW;
        }

        // If not found inside Progman, search for it at the top level
        EnumWindows((hWnd, lParam) =>
        {
            IntPtr hDefView = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (hDefView != IntPtr.Zero)
            {
                // Get the WorkerW window after the current one
                hWorkerW = FindWindowEx(IntPtr.Zero, hWnd, "WorkerW", null);
                return false;
            }
            return true;
        }, IntPtr.Zero);

        if (hWorkerW != IntPtr.Zero)
        {
            Debug.WriteLine("Found WorkerW outside Progman");
        }

        return hWorkerW;
    }

    private void PrintWindowInfo(IntPtr hWnd, string windowName)
    {
        StringBuilder className = new StringBuilder(256);
        GetClassName(hWnd, className, className.Capacity);

        StringBuilder windowText = new StringBuilder(256);
        GetWindowText(hWnd, windowText, windowText.Capacity);

        Debug.WriteLine($"{windowName} class name: {className}");
        Debug.WriteLine($"{windowName} window text: {windowText}");
    }

    private void OnSingleInstanceLaunched(object? sender, SingleInstanceLaunchEventArgs e)
    {
        if (e.IsFirstLaunch)
        {
            // Additional logic for first launch if needed
        }
    }

    public static WindowEx? RunWindow { get; set; }
    public static WindowEx? DesktopWindow { get; set; }
    public static WindowEx? ShutdownDialog { get; set; }
    public static WindowEx? BackgroundWindow { get; set; }
}