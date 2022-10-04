using Shared.Models;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace Shared.Logic;

public class LogManager
{

	private readonly List<LogFile> _logs = new();
	public static LogManager CreateLogger() => new LogManager();
	public LogManager()
	{
		Directory.CreateDirectory(Paths.LogDir);
	}

	public static List<FileInfo> GetLogFiles(string? logName=null)
	{
		return Directory.GetFiles(Paths.LogDir,logName ?? "*").Select(x=>new FileInfo(x)).OrderByDescending(x=>x.CreationTime).ToList();
	}
	public static IEnumerable<LogFile> ReadLog(string logName)
	{
		using (var stream = File.Open(Path.Combine(Paths.LogDir, logName), FileMode.Open))
		using (GZipStream zipStream = new(stream, CompressionMode.Decompress))
			return JsonSerializer.Deserialize<List<LogFile>>(zipStream)!;
	}


	public void Log(LogFile file)
	{
		if (file.From == file.To) return;
		_logs.Add(file);
	}
	public void DumpLog()
	{
		if (_logs.Count == 0) return;
		string md5String;
		using (MemoryStream ms = new MemoryStream())
		{
			using (GZipStream gzip = new(ms, CompressionLevel.SmallestSize, true))
			using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(gzip))
				JsonSerializer.Serialize(jsonWriter, _logs);
			ms.Position = 0;

			using (var md5 = MD5.Create())
				md5String = BitConverter.ToString(md5.ComputeHash(ms)).ToLower().Replace("-", null);
			using (var fs = File.Create(Path.Combine(Paths.LogDir, md5String), (int)ms.Length))
			{
				ms.Position = 0;
				ms.CopyTo(fs);
			}
		}
		Console.WriteLine(md5String);
	}
}
