namespace DataAccess.Models;

public class ExtensionPack
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = null!;
    public List<Extension> Extensions { get; set; } = new();
    public string? Path { get; set; } 
}
