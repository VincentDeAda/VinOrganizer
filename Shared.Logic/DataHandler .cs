using DataAccess;
using DataAccess.Models;
namespace Shared.Logic;
public static class DataHandler
{
	public static void Add(this IExtensionsPacksRepository db, List<string> extensions, string packName)
	{
		//check if the pack is new or in db.
		var pack = db.ExtensionPacks.FirstOrDefault(x => x.Name == packName);
		bool isNew = false;
		if (pack is null)
		{
			pack = new ExtensionPack { Name = packName };
			isNew = true;
		}
		//check if extensions exist on db. remove if yes.
		var existingExtensions = db.Extensions.Where(x => extensions.Contains(x) && !pack.Extensions.Contains(x));

		foreach (var ext in existingExtensions)
			db.RemoveExtension(ext);

		extensions.ForEach(pack.Extensions.Add);

		if (isNew)
			db.ExtensionPacks.Add(pack);
		db.SaveChanges();
	}

	public static List<string>? Merge(this IExtensionsPacksRepository db, List<string> packs, string targetPack)
	{
		var invalidLists = new List<string>();
		List<ExtensionPack> extPacks = new();
		ExtensionPack? target = db.ExtensionPacks.FirstOrDefault(x => x.Name == targetPack);
		if (target is null)
			target = new ExtensionPack { Name = targetPack };
		foreach (string pack in packs)
		{
			var extPack = db.ExtensionPacks.FirstOrDefault(x => x.Name == pack);
			if (extPack is null)
				invalidLists.Add(pack);
			else
				extPacks.Add(extPack);
		}
		if(invalidLists.Count > 0) return invalidLists;
		extPacks.ForEach(x => x.Extensions.ForEach(target.Extensions.Add));
		extPacks.ForEach(x => db.ExtensionPacks.Remove(x));
		db.SaveChanges();
		return null;
	}
}
