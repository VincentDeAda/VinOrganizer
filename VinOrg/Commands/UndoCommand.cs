namespace VinOrgCLI.Commands;
internal class UndoCommand
{
	private readonly ILogManager _logManager;

	public UndoCommand(ILogManager logManager)
	{
		_logManager = logManager;
	}

	[Command(Aliases = new[] { "u" }, Description = "Return the moved files to their original path from the latest organize action, or provide custom log file to undo.")]
	public void Undo([Argument(Description = "The log name or the first couple unique characters of the requested log.")] string? logName, [Option('p', Description = "Keep the log file after returning files to the original source.")] bool preserveLog)
	{

		var logs = _logManager.GetLogFiles(logName);
		if (!logs.Any())
		{
			string errorMessage = logName is null ? "No log file found." : "The provided log file name doesn't exist.";
			Console.WriteLine("No log file found.");
			return;
		}
		if (logName is not null && logs.Count > 1)
		{
			Console.WriteLine("There's more than one file that start with the provided log name. please include more letters.");
			return;
		}
		var log = logs.First();
		var logInfo = _logManager.ReadLog(log.Name);


		if (!preserveLog)
		{
			File.Delete(log.FullName);
		}
		foreach (var file in logInfo)
		{
			try
			{
				File.Move(file.To, file.From);
				Console.WriteLine("File returned: {0}", file.From);
			}
			catch (Exception e)
			{
				Console.WriteLine("Couldn't move file: {0}. Reason: {1}", file.From, e.Message);
			}
		}

	}


}
