namespace DataAccess.Models;
public class Extension
{
    public int Id { get; set; }
    public string ExtensionName { get; set; } = null!;
    public virtual ExtensionPack ExtensionPack { get; set; } = null!;
}
