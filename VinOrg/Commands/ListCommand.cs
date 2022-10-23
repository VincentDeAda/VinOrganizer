namespace VinOrgCLI.Commands;
internal class ListCommand
{
	private readonly IExtensionsPacksRepository _db;
	private readonly ILogManager _logManager;


	public ListCommand(IExtensionsPacksRepository db, ILogManager logManager)
	{
		_db = db;
		_logManager = logManager;
	}

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
		var logs = _logManager.GetLogFiles();
		if (!logs.Any()) return;
		if (listOnlyIds)
			logs.ToList().ForEach(x => Console.WriteLine(x.Name));
		else
			ConsoleHelper.PrintLogs(logs);
	}
}
