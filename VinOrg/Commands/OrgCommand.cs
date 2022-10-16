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
		var res = _db.Organize(paramSet.Path, paramSet.RecursionDepth, paramSet.Steal, paramSet.Recursive, paramSet.MoveUncategorized, paramSet.AutoRename, paramSet.NoLog, _progressTracker);
		if (res is null)
			Console.WriteLine("Using the application within this directory will cause system malfunction. task aborted.");
		else
		{
			Console.WriteLine("Summary\nFiles Moved: {0}.\nFiles Renamed: {1}\nFiles Failed To Move: {2}.\nDirectory Failed To Create: {3}.", res.FileMoved, res.Renamed, res.FailedToMove, res.FailedToCreateDir);
		}

	}



}

