using Windows.Storage;

namespace Rebound.Helpers;

public static class SettingsHelper
{
    public static object GetSetting(string key)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        return localSettings.Values[key];
    }

    public static int GetSettingInt(string key)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        return (int)localSettings.Values[key];
    }

    public static bool GetSettingBool(string key)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        return (bool)localSettings.Values[key];
    }

    public static string GetSettingString(string key)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        return (string)localSettings.Values[key];
    }

    public static void SetSetting(string key, object value)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        localSettings.Values[key] = value;
    }
}
