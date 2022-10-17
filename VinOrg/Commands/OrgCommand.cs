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
			Console.WriteLine("Summary\n{0} File  Moved.\n{1} File Renamed.\n{2} File Failed To Move.\n{3} Directory Failed To Create.", res.FileMoved, res.Renamed, res.FailedToMove, res.FailedToCreateDir);
		}

	}



}

