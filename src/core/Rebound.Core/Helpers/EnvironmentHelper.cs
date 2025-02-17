#nullable enable

namespace Rebound.Helpers;
public static class EnvironmentHelper
{
    public static string GetWindowsInstallationDrivePath()
    {
        // Get the system directory path
        var systemPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows);

        // Extract the drive letter
        var driveLetter = System.IO.Path.GetPathRoot(systemPath);
        return driveLetter ??= "C:\\";
    }
}