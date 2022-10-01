namespace DataAccess.Models;
public class ExtensionPack
{
    public string Name { get; set; } = null!;
    public string? Path { get; set; } = null!;

    public virtual List<string> Extensions { get; set; } = new();
}
