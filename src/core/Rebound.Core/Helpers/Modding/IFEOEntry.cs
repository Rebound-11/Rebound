using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Rebound.Helpers.Modding;
public partial class IFEOEntry
{
    public string? EntryName { get; set; }

    public bool IsIntact()
    {
        return false;
    }
}