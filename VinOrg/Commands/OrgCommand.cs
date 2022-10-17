namespace VinOrgCLI.Commands;
internal class OrgCommand
{
	private readonly IExtensionsPacksRepository _db;
	private readonly IProgressTracker _progressTracker;
	public OrgCommand(IExtensionsPacksRepository db, IProgressTracker progressTracker)
	{
		_db = db;
		_progressTracker = progressTracker;
	}

	[PrimaryCommand]
	public void Organize(OrganizeParams paramSet)
	{
		var res = _db.Organize(paramSet, _progressTracker);
		if (res is null)
			Console.WriteLine("Using the application within this directory will cause system malfunction. task aborted.");
		else
		{
			Console.WriteLine("Summary");
			Console.WriteLine("{0:00} File  Moved", res.FileMoved);
			Console.WriteLine("{0:00} File  Failed to move", res.FailedToMove);
			Console.WriteLine("{0:00} File  Renamed", res.Renamed);
			Console.WriteLine("{0:00} File  Directory Failed To Create", res.FailedToCreateDir);
			if (res.LogMD5 is not null)
				Console.WriteLine("Log MD5: {0}", res.LogMD5);
		}

	}



}

