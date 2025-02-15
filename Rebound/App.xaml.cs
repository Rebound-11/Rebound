using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Rebound.Helpers;
using WinUIEx;

namespace Rebound;

public partial class App : Application
{
    private readonly SingleInstanceDesktopApp _singleInstanceApp;

    public App()
    {
        this?.InitializeComponent();

        _singleInstanceApp = new SingleInstanceDesktopApp("Rebound.Hub");
        _singleInstanceApp.Launched += OnSingleInstanceLaunched;
        UnhandledException += App_UnhandledException;
    }

    private async void OnSingleInstanceLaunched(object? sender, SingleInstanceLaunchEventArgs e)
    {
        if (e.IsFirstLaunch)
        {
            await LaunchWork();
        }
        else
        {
            if (MainAppWindow != null)
            {
                _ = ((MainWindow)MainAppWindow).BringToFront();
            }
            else
            {
                await LaunchWork();
            }
            return;
        }
    }

    public async Task LaunchWork()
    {
        if (string.Join(" ", Environment.GetCommandLineArgs().Skip(1)).Contains("CONTROL"))
        {
            _ = Process.Start(new ProcessStartInfo("control:") { UseShellExecute = true });
            MainAppWindow.Close();
        }
        /*if (string.Join(" ", Environment.GetCommandLineArgs().Skip(1)).Contains("UAC"))
        {
            var win = new UACWindow();
            win.Show();
            await Task.Delay(10);
            win.BringToFront();
            MainAppWindow = null;
            return;
        }*/
        if (string.Join(" ", Environment.GetCommandLineArgs().Skip(1)).Contains("SFC"))
        {
            var win = new UninstallationWindow(true);
            _ = win.Show();
            await Task.Delay(10);
            _ = win.BringToFront();
            MainAppWindow = null;
            return;
        }
        if (string.Join(" ", Environment.GetCommandLineArgs().Skip(1)).Contains("UNINSTALLFULL"))
        {
            var win = new UninstallationWindow(true);
            _ = win.Show();
            await Task.Delay(10);
            _ = win.BringToFront();
            MainAppWindow = null;
            return;
        }
        if (string.Join(" ", Environment.GetCommandLineArgs().Skip(1)).Contains("UNINSTALL"))
        {
            var win = new UninstallationWindow(false);
            _ = win.Show();
            await Task.Delay(10);
            _ = win.BringToFront();
            MainAppWindow = null;
            return;
        }
        else
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Log or handle the exception
        Debug.WriteLine($"Unhandled exception: {e.Exception.Message}");
        e.Handled = true; // Prevent the application from terminating
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) => _singleInstanceApp?.Launch(args.Arguments);

    public static Window MainAppWindow;

    public static Window ControlPanelWindow;
}