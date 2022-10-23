using Shared.Models;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace Shared.Logic;

public class LogManager : ILogManager
{
	private readonly string _logDir;

	public LogManager(string logDir)
	{
		_logDir = logDir;
		Directory.CreateDirectory(_logDir);

	}

	private readonly List<LogFile> _logs = new();
	
	public  List<FileInfo> GetLogFiles(string? logName = null)
	{
		return Directory.GetFiles(_logDir, logName ?? "*").Select(x => new FileInfo(x)).OrderByDescending(x => x.CreationTime).ToList();
	}
	public  IEnumerable<LogFile> ReadLog(string logName)
	{
		using (var stream = File.Open(Path.Combine(_logDir, logName), FileMode.Open))
		using (GZipStream zipStream = new(stream, CompressionMode.Decompress))
			return JsonSerializer.Deserialize<List<LogFile>>(zipStream)!;
	}


	public void Log(LogFile file)
	{
		if (file.From == file.To) return;
		_logs.Add(file);
	}
	public string? DumpLog()
	{
		if (_logs.Count == 0) return null;
		string md5String;
		using (MemoryStream ms = new MemoryStream())
		{
			using (GZipStream gzip = new(ms, CompressionLevel.SmallestSize, true))
			using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(gzip))
				JsonSerializer.Serialize(jsonWriter, _logs);
			ms.Position = 0;

			using (var md5 = MD5.Create())
				md5String = BitConverter.ToString(md5.ComputeHash(ms)).ToLower().Replace("-", null);
			using (var fs = File.Create(Path.Combine(_logDir, md5String), (int)ms.Length))
			{
				ms.Position = 0;
				ms.CopyTo(fs);
			}
		}
		return md5String;
	}
}
