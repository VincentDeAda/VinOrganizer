
namespace VinOrgCLI.Commands;
internal class MergeCommand
{

	private readonly SQLiteDatabase _db;

	public MergeCommand(SQLiteDatabase db) => _db = db;


	[Command(Aliases = new[] { "m" }, Description = "Merge one or more extension pack to a pre-existing one.")]
	public void Merge([Argument] List<string> packs, [Argument] string targetPack)
	{
		List<ExtensionPack> extPacks = new();
		ExtensionPack? target = _db.ExtensionPacks.FirstOrDefault(x => x.Name == targetPack);
		if (target is null)
		{
			Console.WriteLine("The provided target Pack \"{0}\" doesn't exist", target);
			return;
		}
		foreach (string pack in packs)
		{
			var extPack = _db.ExtensionPacks.FirstOrDefault(x => x.Name == pack);
			if (extPack is null)
			{
				Console.WriteLine("The provided pack \"{0}\" doesn't exist in the database.", pack);
				return;
			}
			extPacks.Add(extPack);
		}
		extPacks.ForEach(x => x.Extensions.ForEach(target.Extensions.Add));
		_db.Update(target);
		_db.RemoveRange(extPacks);
		_db.SaveChanges();
	}

}