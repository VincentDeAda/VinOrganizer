namespace VinOrgCLI.Commands;
internal class ListCommand
{
	private readonly IExtensionsPacksRepository _db;

	public ListCommand(IExtensionsPacksRepository db) => _db = db;

	[Command(Aliases = new[] { "ls" }, Description = "List all extensions packs with their extensions.")]
	public void List()
	{
		var packs = _db.ExtensionPacks.ToList();
		ConsoleHelper.PrintExtensionsPacks(packs);
	}
}
