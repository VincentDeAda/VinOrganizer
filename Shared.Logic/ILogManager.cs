using Shared.Models;

namespace Shared.Logic;
public interface ILogManager
{
	string? DumpLog();
	void Log(LogFile file);
	List<FileInfo> GetLogFiles(string? logName = null);
	IEnumerable<LogFile> ReadLog(string logName);
}