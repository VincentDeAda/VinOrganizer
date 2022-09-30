namespace VinOrgCLI.Commands;
internal class RemoveCommand
{

	private readonly SQLiteDatabase _db;

	public RemoveCommand(SQLiteDatabase db) => _db = db;

	public void Logs()
	{
		Directory.CreateDirectory(LogManager.ConfigDir);
		var files = Directory.GetFiles(LogManager.ConfigDir);
		if (files.Any() == false)
			return;
		Console.WriteLine("Are you sure you want to delete {0} log file?", files.Length);
		Console.Write("Press [Y] to confirm the requested action: ");
		var k = Console.ReadKey();
		if (k.Key != ConsoleKey.Y)
			return;

		foreach (var file in files)
			try
			{
				File.Delete(file);

			}
			catch (Exception e)
			{
				Console.WriteLine("Couldn't delete file: {0}. Reason: {1}", file, e.Message);
			}
	}

	public void Pack([Argument] string packName)
	{

		_db.Database.EnsureCreated();
		var pack = _db.ExtensionPacks.FirstOrDefault(x => x.Name == packName);
		if (pack is null)
		{
			Console.WriteLine("The provided pack name doesn't exist.");
			return;
		}
		_db.Remove(pack);
		_db.SaveChanges();
	}

	public void Extensions([Argument] List<string> extensions)
	{
		_db.Database.EnsureCreated();
		List<Extension> exts = new();
		foreach (string extension in extensions)
		{
			var ext = _db.Extensions.FirstOrDefault(x => x.ExtensionName == extension.ToLower());
			if (ext is not null)
				exts.Add(ext);
		}
		_db.RemoveRange(exts);
		_db.SaveChanges();
	}
}
