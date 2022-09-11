

using Cocona;

namespace DataAccess.Models;

public class Extension
{
    public int Id { get; set; }
    public string ExtensionName { get; set; } = "";

    public ExtensionPack ExtensionPack { get; set; } = null!;
}
