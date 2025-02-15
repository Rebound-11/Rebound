using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using WinUIEx;

namespace Rebound.Views;

public sealed partial class Rebound11Page : Page
{
    public Rebound11Page()
    {
        this.InitializeComponent();

        if (IsAdmin())
        {
            Admin1.Visibility = Visibility.Collapsed;
            Admin3.Visibility = Visibility.Collapsed;
            Admin5.Visibility = Visibility.Collapsed;
            Admin6.Visibility = Visibility.Collapsed;
        }

        if (IsReboundInstalled())
        {
            Rebound11IsInstalledGrid.Visibility = Visibility.Visible;
            Rebound11IsInstallingGrid.Visibility = Visibility.Collapsed;
            Rebound11IsNotInstalledGrid.Visibility = Visibility.Collapsed;
            DetailsPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            Rebound11IsInstalledGrid.Visibility = Visibility.Collapsed;
            Rebound11IsInstallingGrid.Visibility = Visibility.Collapsed;
            Rebound11IsNotInstalledGrid.Visibility = Visibility.Visible;
            DetailsPanel.Visibility = Visibility.Visible;
        }

        if (string.Join(" ", Environment.GetCommandLineArgs().Skip(1)).ToUpperInvariant().Contains("INSTALLREBOUND11"))
        {
            Rebound11IsInstalledGrid.Visibility = Visibility.Collapsed;
            Rebound11IsInstallingGrid.Visibility = Visibility.Visible;
            Rebound11IsNotInstalledGrid.Visibility = Visibility.Collapsed;
            DetailsPanel.Visibility = Visibility.Visible;
        }
        _ = CheckForUpdatesAsync();
    }

    public bool IsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public bool IsReboundInstalled() => Directory.Exists("C:\\Rebound11");

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var win = new InstallationWindow((bool)FilesCheck.IsChecked, (bool)RunCheck.IsChecked, (bool)DefragCheck.IsChecked, (bool)WinverCheck.IsChecked, (bool)UACCheck.IsChecked, (bool)OSKCheck.IsChecked, (bool)TPMCheck.IsChecked, (bool)DiskCleanupCheck.IsChecked);
        _ = win.Show();
    }

    private async Task CheckForUpdatesAsync()
    {
        // URL of the text file containing the latest version number
        var versionUrl = "https://ivirius.vercel.app/reboundhubversion.txt";

        // Use HttpClient to fetch the content
        using var client = new HttpClient();

        try
        {
            // Fetch the version string from the URL
            var latestVersion = await client.GetStringAsync(versionUrl);

            // Trim any excess whitespace/newlines from the fetched string
            latestVersion = latestVersion.Trim();

            // Get the current app version
            var currentVersion = "v0.0.3 ALPHA";

            // Compare versions
            if (latestVersion == currentVersion)
            {
                // The app is up-to-date
                UpdateBar.IsOpen = false;
            }
            else
            {
                // A new version is available
                UpdateBar.IsOpen = true;

                if (latestVersion.Contains("ALPHA"))
                {
                    UpdateBar.Title = $"A new ALPHA release is available for Rebound Hub and Rebound 11! (New version: {latestVersion})";
                    UpdateBar.Message = "Note: ALPHA versions can be unstable.";
                    UpdateBar.Severity = InfoBarSeverity.Warning;
                    return;
                }
                
                if (latestVersion.Contains("DEV"))
                {
                    UpdateBar.Title = $"A new DEV release is available for Rebound Hub and Rebound 11! (New version: {latestVersion})";
                    UpdateBar.Message = "Note: DEV versions can be unstable.";
                    UpdateBar.Severity = InfoBarSeverity.Warning;
                    return;
                }
                
                if (latestVersion.Contains("BETA"))
                {
                    UpdateBar.Title = $"A new BETA release is available for Rebound Hub and Rebound 11! (New version: {latestVersion})";
                    UpdateBar.Message = "Note: BETA versions can be glitchy.";
                    UpdateBar.Severity = InfoBarSeverity.Warning;
                    return;
                }

                UpdateBar.Title = $"A new update is available for Rebound Hub and Rebound 11! (New version: {latestVersion})";
                UpdateBar.Severity = InfoBarSeverity.Success;
            }
        }
        catch (Exception)
        {
            // Handle any errors that occur during the request
            UpdateBar.IsOpen = true;
            UpdateBar.Severity = InfoBarSeverity.Error;
            UpdateBar.Title = "Something went wrong.";
        }
    }

    private async void Button_Click_1(object sender, RoutedEventArgs e)
    {
        var info = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            Verb = "runas",
            Arguments = @$"Start-Process ""shell:AppsFolder\d6ef5e04-e9da-4e22-9782-8031af8beae7_yejd587sfa94t!App"" -ArgumentList ""INSTALLREBOUND11"" -Verb RunAs"
        };

        var process = Process.Start(info);

        // Wait for the process to exit before proceeding
        await process.WaitForExitAsync();
        App.MainAppWindow.Close();
    }

    private async void Button_Click_2(object sender, RoutedEventArgs e)
    {
        var info = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            Verb = "runas",
            Arguments = @$"Start-Process ""shell:AppsFolder\d6ef5e04-e9da-4e22-9782-8031af8beae7_yejd587sfa94t!App"" -ArgumentList ""UNINSTALL"" -Verb RunAs"
        };

        var process = Process.Start(info);

        // Wait for the process to exit before proceeding
        await process.WaitForExitAsync();
        App.MainAppWindow.Close();
    }

    private void SettingsCard_Click(object sender, RoutedEventArgs e) => _ = File.Exists("C:\\Rebound11\\rwinver.exe") ? Process.Start("C:\\Rebound11\\rwinver.exe") : Process.Start("winver.exe");

    private async void Button_Click_3(object sender, RoutedEventArgs e)
    {
        var info = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            Verb = "runas",
            Arguments = @$"Start-Process ""shell:AppsFolder\d6ef5e04-e9da-4e22-9782-8031af8beae7_yejd587sfa94t!App"" -ArgumentList ""UNINSTALLFULL"" -Verb RunAs"
        };

        var process = Process.Start(info);

        // Wait for the process to exit before proceeding
        await process.WaitForExitAsync();
        App.MainAppWindow.Close();
    }
}