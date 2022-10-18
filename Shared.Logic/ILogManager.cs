using Shared.Models;

namespace Shared.Logic;
public interface ILogManager
{
	string? DumpLog();
	void Log(LogFile file);
}