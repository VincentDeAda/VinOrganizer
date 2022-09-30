namespace VinOrgCLI.Commands;
internal class OrgCommand
{
	private readonly SQLiteDatabase _db;

	public OrgCommand(SQLiteDatabase db) => _db = db;
	[PrimaryCommand]
	public void Organize(OrganizeParams paramSet)
	{
		var currentDir = Directory.GetCurrentDirectory();
		string lookupFolder = paramSet.Path ?? currentDir;
		string targetFolder = paramSet.Steal ? currentDir : lookupFolder;

		List<Environment.SpecialFolder> systemDirs = new List<Environment.SpecialFolder>() {
		Environment.SpecialFolder.ApplicationData,
		Environment.SpecialFolder.LocalApplicationData,
		Environment.SpecialFolder.CommonApplicationData,
		Environment.SpecialFolder.Windows,
		Environment.SpecialFolder.System,
		Environment.SpecialFolder.SystemX86,
		Environment.SpecialFolder.ProgramFiles,
		Environment.SpecialFolder.ProgramFilesX86,

	};
		bool isSystemDir = systemDirs.Any(x => currentDir == Environment.GetFolderPath(x));
		string sysDir = string.IsNullOrWhiteSpace(Environment.SystemDirectory) ? "/" : Environment.SystemDirectory;
		bool isSystemRoot = Directory.GetDirectoryRoot(sysDir) == lookupFolder;
		if (isSystemDir || isSystemRoot)
		{
			Console.WriteLine("Using the application within this directory will cause system malfunction. task aborted.");
			return;
		}

		var files = Directory.GetFiles(lookupFolder
					 , "*", new EnumerationOptions()
					 {
						 MaxRecursionDepth = paramSet.RecursionDepth ?? int.MaxValue,
						 IgnoreInaccessible = true,
						 RecurseSubdirectories = paramSet.Recursive,
					 }).Select(x => new FileInfo(x)).ToList();
		var extensions = _db.Extensions.ToList();
		if (!paramSet.MoveUncategorized)
			files = files.Where(x => extensions.FirstOrDefault(y => x.Extension.Length > 0 ? x.Extension.Substring(1).ToLower() == y.ExtensionName : false) != null).ToList();

		var groupedFiles = files.GroupBy(x => x.Extension.ToLower());
		LogManager? changeLogger = paramSet.NoLog ? null : LogManager.CreateLogger();
		int renamed = 0;
		foreach (var ext in groupedFiles)
		{
			var packName = extensions.FirstOrDefault(x => ext.Key.Length > 0 ? x.ExtensionName == ext.Key.Substring(1).ToLower() : false)?.ExtensionPack;
			if (packName is not null || paramSet.MoveUncategorized)
			{

				var newDir = Path.Combine(packName?.Path ?? targetFolder, packName?.Name ?? "Uncategorized");
				if (!Directory.Exists(newDir))
					try
					{
						Directory.CreateDirectory(newDir);
					}
					catch (Exception e)
					{
						Console.WriteLine("Failed to create {0}", newDir);
						Console.WriteLine("Error message: {0}", e.Message);
						continue;
					}

				foreach (FileInfo file in CollectionsMarshal.AsSpan(ext.ToList()))
				{
					var log = new LogFile();
					log.From = file.FullName;
					try
					{
						file.MoveTo(Path.Combine(newDir, file.Name));
						log.To = file.FullName;
						changeLogger?.Log(log);
					}
					catch (IOException e)
					{
						if (paramSet.AutoRename)
						{

							file.MoveTo(Path.Combine(newDir, string.Format("{0}-{1}", file.CreationTime.ToString("yyyyMMddHHmmssff") + Random.Shared.Next(1000).ToString(), file.Name)));
							renamed++;
							log.To = file.FullName;
							changeLogger?.Log(log);

						}
						if (paramSet.SilentMode) continue;
						Console.WriteLine("Couldn't move file: {0}. Reason:{1}", file.FullName, e.Message);
					}
				}
			}
		}
		if (paramSet.AutoRename && renamed > 0) Console.WriteLine("{0} Files Renamed.", renamed);
		changeLogger?.DumpLog();

	}
}

