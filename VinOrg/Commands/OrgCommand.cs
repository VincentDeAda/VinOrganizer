namespace VinOrgCLI.Commands;
internal class OrgCommand
{
	private readonly IExtensionsPacksRepository _db;

	public OrgCommand(IExtensionsPacksRepository db) => _db = db;
	[PrimaryCommand]
	public void Organize(OrganizeParams paramSet)
	{
		var res = _db.Organize(paramSet.Path, paramSet.RecursionDepth, paramSet.Steal, paramSet.Recursive, paramSet.MoveUncategorized, paramSet.AutoRename, paramSet.NoLog);
		if (res is null)
			Console.WriteLine("Using the application within this directory will cause system malfunction. task aborted.");
		else
		{
			Console.WriteLine("Summary\nFiles Moved: {0}.\nFiles Renamed: {1}\nFiles Failed To Move: {2}.\nDirectory Failed To Create: {3}.", res.FileMoved, res.Renamed, res.FailedToMove, res.FailedToCreateDir);
		}

	}



}

