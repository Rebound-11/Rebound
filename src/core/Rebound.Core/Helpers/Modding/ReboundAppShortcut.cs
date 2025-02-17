using System;
using IWshRuntimeLibrary;

#nullable enable

namespace Rebound.Helpers.Modding;

public enum ShortcutAction
{
    CreateOrModify,
    DeleteOrRestore
}

public partial class ReboundAppShortcut
{
    public string? Path { get; set; }

    public string? OriginalIconLocation { get; set; }

    public string? ModernIconLocation { get; set; }

    public string? TargetPath { get; set; }

    public bool ReplaceExisting { get; set; } = true;

    public bool RunAsAdmin { get; set; } = false;

    public bool IsShortcutIntact()
    {
        return false;
    }

    public void Modify(ShortcutAction iconAction)
    {
        switch (ReplaceExisting)
        {
            case true:
                {
                    switch (iconAction)
                    {
                        case ShortcutAction.CreateOrModify:
                            {
                                // Obtain the shortcut class
                                var wsh = new WshShell();
                                IWshShortcut shortcut = wsh.CreateShortcut(Path);

                                // Set the icon location
                                shortcut.IconLocation = ModernIconLocation;

                                // Define admin launch
                                shortcut.Arguments = RunAsAdmin ? "runas" : string.Empty;

                                // Set target path
                                shortcut.TargetPath = TargetPath;

                                // Save the changes
                                shortcut.Save();
                                break;
                            }
                        case ShortcutAction.DeleteOrRestore:
                            {
                                break;
                            }
                    }
                    break;
                }
            case false:
                {
                    // Obtain the shortcut class
                    var wsh = new WshShell();
                    IWshShortcut shortcut = wsh.CreateShortcut(Path);

                    // Set the icon location
                    shortcut.IconLocation = iconAction switch
                    {
                        ShortcutAction.CreateOrModify => ModernIconLocation,
                        ShortcutAction.DeleteOrRestore => OriginalIconLocation,
                        _ => throw new Exception("Invalid argument for IconAction.")
                    };

                    // Define admin launch
                    shortcut.Arguments = RunAsAdmin ? "runas" : string.Empty;

                    // Save the changes
                    shortcut.Save();

                    break;
                }
        }
    }
}