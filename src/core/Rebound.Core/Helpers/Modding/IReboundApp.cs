using System.Collections.Generic;

#nullable enable

namespace Rebound.Helpers.Modding;

public enum ReboundAppIntegrity
{
    Installed,
    Corrupt,
    NotInstalled
}

public enum InstallationTemplate
{
    Basic,
    Recommended,
    Complete,
    Extras
}

public interface IReboundRootApp
{
    public void Install();

    public void Uninstall();

    public ReboundAppIntegrity GetIntegrity();

    public InstallationTemplate PreferredInstallationTemplate { get; set; }

    public List<AppPackage>? AppPackages { get; set; }

    public List<ReboundAppShortcut>? Shortcuts { get; set; }

    public List<IFEOEntry>? IFEOEntries { get; set; }
}