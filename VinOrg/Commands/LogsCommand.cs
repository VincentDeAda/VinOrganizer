namespace VinOrgCLI.Commands;
internal class LogsCommand
{
	[Command(Aliases = new[] {"l"},Description = "Print all log files.")]
	public void Logs([Option('d', Description = "List only the ids of the logs.")] bool listOnlyIds)
	{
		Directory.CreateDirectory(LogManager.LogDir);
		var logs = Directory.GetFiles(LogManager.LogDir, "*")
		.Select(x => new FileInfo(x))
		.OrderByDescending(x => x.CreationTime);
		if (!logs.Any()) return;
		if (listOnlyIds)
			logs.ToList().ForEach(x => Console.WriteLine(x.Name));
		else
			ConsoleHelper.PrintLogs(logs);
	}
}