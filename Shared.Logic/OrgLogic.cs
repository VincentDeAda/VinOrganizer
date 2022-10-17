using DataAccess;
using DataAccess.Models;
using Shared.Models;
using System.Runtime.InteropServices;

namespace Shared.Logic;
public static class OrgLogic
{
	public static OrganizeResult? Organize(this IExtensionsPacksRepository db, OrganizeParamsBase paramSet, IProgressTracker? progressTracker = null)
	{
		var result = new OrganizeResult();
		var currentDir = Directory.GetCurrentDirectory();

		string lookupFolder = paramSet.Path ?? currentDir;
		string targetFolder = paramSet.Steal ? currentDir : lookupFolder;

		//Checking if dir is not dangerous dir.
		if (IsBadDir(currentDir, lookupFolder))
			return null;


		var files = GetFiles(lookupFolder, paramSet.Recursive, paramSet.RecursionDepth);


		//Grouping the files by their extension
		var groupedFiles = files.GroupBy(x => x.Extension.ToLower()).ToList();
		if (paramSet.MoveUncategorized == false)
		{
			int fileCount = groupedFiles
									.Where(x => x.Key.Length > 0 && db.Extensions.Contains(x.Key[1..]))
									.SelectMany(x => x)
									.Count();
			progressTracker?.SetMax(fileCount);

		}
		else progressTracker?.SetMax(files.Count);



		//Creating The Logger
		LogManager? changeLogger;

		if (paramSet.NoLog)
			changeLogger = null;
		else
			changeLogger = LogManager.CreateLogger();

		foreach (var ext in groupedFiles)
		{

			ExtensionPack? pack = null;

			//Checking if the extension valid and finding it in the db.
			if (ext.Key.Length > 0)
				pack = db.ExtensionPacks.FirstOrDefault(x => x.Extensions.Contains(ext.Key[1..].ToLower()));

			//If the extension doesn't exist on the db ignore it.
			if (pack is null && paramSet.MoveUncategorized == false)
				continue;

			//Assigning the path to either the pack custom path or the target folder with checking if 
			var extPackDir = Path.Combine(pack?.Path ?? targetFolder, pack?.Name ?? "Uncategorized");

			try
			{
				Directory.CreateDirectory(extPackDir);
			}
			catch (Exception)
			{
				result.FailedToCreateDir++;
				continue;
			}

			foreach (FileInfo file in CollectionsMarshal.AsSpan(ext.ToList()))
			{
				var log = new LogFile();
				log.From = file.FullName;
				try
				{
					file.MoveTo(Path.Combine(extPackDir, file.Name));
					log.To = file.FullName;
					result.FileMoved++;
				}
				catch (Exception)
				{
					if (paramSet.AutoRename)
					{
						file.MoveTo(Path.Combine(extPackDir, string.Format("{1}-{0}", file.CreationTime.ToString("yyyyMMddHHmmssff") + Random.Shared.Next(1000).ToString(), file.Name)));
						log.To = file.FullName;
						result.Renamed++;
						result.FileMoved++;

					}
					else
					{
						result.FailedToMove++;
					}
				}

				changeLogger?.Log(log);
				progressTracker?.Increament(1);
			}
		}
		changeLogger?.DumpLog();
		return result;
	}

	private static List<FileInfo> GetFiles(string lookupFolder, bool recursive, int? maxRecursionDepth)
	{
		return Directory.GetFiles(lookupFolder
			 , "*", new EnumerationOptions()
			 {
				 MaxRecursionDepth = maxRecursionDepth ?? int.MaxValue,
				 IgnoreInaccessible = true,
				 RecurseSubdirectories = recursive
			 }).Select(x => new FileInfo(x)).ToList();
	}
	private static bool IsBadDir(string currentDir, string lookupFolder)
	{
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
		return isSystemDir || isSystemRoot;
	}
}
