namespace VinOrgCLI.Commands;
internal class SetCommand
{
	private readonly IExtensionsPacksRepository _db;

	public SetCommand(IExtensionsPacksRepository db) => _db = db;
	[Command(Aliases = new[] { "p" })]
	public void Path([Argument] List<string> packs, [Argument, PathValid] string path)
	{

		var invalidPacks = _db.SetPaths(packs, path);

		if (invalidPacks is not null)
		{
			Console.WriteLine("The following packs doesn't exist on the database: ");
			invalidPacks.ForEach(Console.WriteLine);
		}
	}
	public void Name([Argument] string pack, [Argument,IsValidPackName] string newName)
	{
		var res= _db.SetName(pack, newName);
		switch (res)
		{
			case 1:
				Console.WriteLine("The provided pack name doesn't exist on the database.");
				break;
			case 2:
				Console.WriteLine("The new pack name already in use.");
				break;
			default:
				break;
		}
	}
}
