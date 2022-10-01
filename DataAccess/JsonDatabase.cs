using DataAccess.Models;
using System.Text.Json;

namespace DataAccess;
public class JsonDatabase : IExtensionsPacksRepository
{
	public List<ExtensionPack> ExtensionPacks { get; set; } = new();
	public List<string> Extensions { get; set; } = new();
	private readonly string _jsonPath;
	public JsonDatabase(string configDir)
	{
		Directory.CreateDirectory(configDir);
		_jsonPath = Path.Combine(configDir, "extensions.json");
		if (File.Exists(_jsonPath))
			ReadJsonDatabase();
	}
	private void ReadJsonDatabase()
	{
		try
		{
			ExtensionPacks = JsonSerializer.Deserialize<List<ExtensionPack>>(File.ReadAllText(_jsonPath)) ?? new();
			Extensions = ExtensionPacks.SelectMany(x => x.Extensions).ToList();
		}
		catch (JsonException e)
		{
			Console.WriteLine("Detected corrupted JSON file.");
			Console.WriteLine("Error Message: ");
			Console.WriteLine(e.Message);
		}

	}

	public void RemoveExtension(string extension)
	{
		var extpack = ExtensionPacks.FirstOrDefault(x => x.Extensions.Contains(extension));
		if (extpack is null) return;
		extpack.Extensions.Remove(extension);
	}
	public void SaveChanges()
	{
		File.WriteAllText(_jsonPath, JsonSerializer.Serialize(ExtensionPacks));
	}


}
