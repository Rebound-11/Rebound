using System;

#nullable enable

namespace Rebound.Helpers.Services;

public partial class ReboundAppService
{
    public const string LEGACY_LAUNCH = "legacy";

    public ReboundAppService(string legacyLaunchActionName)
    {
        AddLegacyAppLauncher(legacyLaunchActionName);
    }

    public static async void AddLegacyAppLauncher(string name)
    {
        // Get the app's jump list.
        var jumpList = await Windows.UI.StartScreen.JumpList.LoadCurrentAsync();

        // Disable the system-managed jump list group.
        jumpList.SystemGroupKind = Windows.UI.StartScreen.JumpListSystemGroupKind.None;

        // Remove any previously added custom jump list items.
        jumpList.Items.Clear();

        var item = Windows.UI.StartScreen.JumpListItem.CreateWithArguments(LEGACY_LAUNCH, name);
        item.Logo = new Uri("ms-appx:///Assets/Computer disk.png");

        jumpList.Items.Add(item);

        // Save the changes to the app's jump list.
        await jumpList.SaveAsync();
    }
}