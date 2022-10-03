namespace VinOrgCLI.Commands;
internal class ListCommand
{
	private readonly IExtensionsPacksRepository _db;

	public ListCommand(IExtensionsPacksRepository db) => _db = db;

	[Command(Aliases = new[] { "ext" }, Description = "List all extensions packs with their extensions.")]
	public void Extensions()
	{
		var packs = _db.ExtensionPacks.ToList();
		ConsoleHelper.PrintExtensionsPacks(packs);
	}

	[Command(Aliases = new[] { "l" }, Description = "Print all log files.")]
	public void Logs([Option('d', Description = "List only the ids of the logs.")] bool listOnlyIds)
	{
		Directory.CreateDirectory(Paths.LogDir);
		var logs = LogManager.GetLogFiles();
		if (!logs.Any()) return;
		if (listOnlyIds)
			logs.ToList().ForEach(x => Console.WriteLine(x.Name));
		else
			ConsoleHelper.PrintLogs(logs);
	}
}
