namespace VinOrgCLI.Commands;
internal class UndoCommand
{
	[Command(Aliases = new[] {"u"},Description = "Undo moving the files that got organized [require log].")]
	public void Undo([Argument(Description = "The log name or the first couple unique characters of the requested log.")] string? logName, [Option('l', Description = "Automatically pass the most recent log.")] bool useLastLog)
	{
		if (!useLastLog && string.IsNullOrEmpty(logName))
		{
			Console.WriteLine("Error: Argument 'log-name' is required. See '--help' for usage.");
			return;
		}
		string errorMsg;
		string searchPattern;
		if (useLastLog)
		{
			errorMsg = "No logs found.";
			searchPattern = "*";
		}
		else
		{
			errorMsg = "The provided log file name doesn't exist.";
			searchPattern = logName + "*";
		}


		Directory.CreateDirectory(LogManager.LogDir);
		var files = Directory.GetFiles(LogManager.LogDir, searchPattern)
		.Select(x => new FileInfo(x))
		.OrderByDescending(x => x.CreationTime);

		if (files.Any() == false)
		{
			Console.WriteLine(errorMsg);
			return;
		}

		var logs = LogManager.ReadLog(files.First().Name);
		foreach (var log in logs)
		{
			try
			{
				File.Move(log.To, log.From);
				Console.WriteLine("File: {0}", log.From);
			}
			catch (Exception e)
			{
				Console.WriteLine("Couldn't move file: {0}. Reason: {1}", log.From, e.Message);
			}
		}

	}
}
