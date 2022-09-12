

using Cocona;

namespace DataAccess.Models;

public class Extension
{
    public int Id { get; set; }
    public string ExtensionName { get; set; } = "";

    public virtual ExtensionPack ExtensionPack { get; set; } = null!;
}
