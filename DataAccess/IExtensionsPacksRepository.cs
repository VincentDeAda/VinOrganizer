using DataAccess.Models;
namespace DataAccess;
public interface IExtensionsPacksRepository
{
	public List<ExtensionPack> ExtensionPacks { get;  set; }
	public List<string> Extensions { get;  set; }
	public void RemoveExtension(string extension);
	public void SaveChanges();
}
