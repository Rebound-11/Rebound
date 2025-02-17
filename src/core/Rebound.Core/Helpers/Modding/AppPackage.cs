#nullable enable

namespace Rebound.Helpers.Modding;

public partial class AppPackage
{
    public string? PackageFamilyName { get; set; }

    public string? PackageSource { get; set; }

    public bool IsInstalled()
    {
        return false;
    }
}