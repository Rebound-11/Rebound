using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Rebound.Helpers.Modding;

enum IFEOOperation
{
    AddOrModify,
    Remove
}

internal class IFEOHelper
{
    public const string REG_IFEO_DIR = "";

    public static void ModifyIFEO(string keyName, string launchCommand, IFEOOperation operation)
    {
        switch (operation)
        {
            case IFEOOperation.AddOrModify:
                {
                    break;
                }
            case IFEOOperation.Remove:
                {
                    break;
                }
        }
    }
}