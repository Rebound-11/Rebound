using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Rebound.Helpers.Modding;

public abstract class ReboundAppInstructions : IReboundRootApp
{
    public virtual InstallationTemplate PreferredInstallationTemplate { get; set; } = InstallationTemplate.Extras;

    public virtual List<AppPackage>? AppPackages { get; set; }

    public virtual List<ReboundAppShortcut>? Shortcuts { get; set; }

    public virtual List<IFEOEntry>? IFEOEntries { get; set; }

    public void Install()
    {

    }

    public void Uninstall()
    {

    }

    public ReboundAppIntegrity GetIntegrity()
    {
        var isAppPackageIntact = AppPackages?.All(pkg => pkg.IsInstalled());
        var isShortcutIntact = Shortcuts?.All(sc => sc.IsShortcutIntact());
        var isIFEOEntryIntact = IFEOEntries?.All(entry => entry.IsIntact());

        return
            // Check if everything is ok
            isShortcutIntact is true or null && isAppPackageIntact is true or null && isIFEOEntryIntact is true or null ?

            // All good
            ReboundAppIntegrity.Installed :

            // Check if nothing is ok
            isShortcutIntact is false && !isAppPackageIntact is false && !isIFEOEntryIntact is false ?

            // Not installed
            ReboundAppIntegrity.NotInstalled : 
            
            // Corrupt
            ReboundAppIntegrity.Corrupt;
    }
}