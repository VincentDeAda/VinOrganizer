namespace VinOrgCLI.Commands;
internal class ListCommand
{
	private readonly SQLiteDatabase _db;

	public ListCommand(SQLiteDatabase db) => _db = db;

	[Command(Aliases = new[] { "ls" }, Description = "List all extensions packs with their extensions.")]
	public void List()
	{
		_db.Database.EnsureCreated();
		var packs = _db.ExtensionPacks.ToList();
		ConsoleHelper.PrintExtensionsPacks(packs);
	}
}
