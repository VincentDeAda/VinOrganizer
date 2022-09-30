
namespace VinOrgCLI.Commands;
internal class SetupCommand
{
	private readonly SQLiteDatabase _db;

	public SetupCommand(SQLiteDatabase db) => _db = db;

	[Command(Description = "Add all known and most used extension types to the database.")]
	public void SetupExtensionPacks([Option('y')] bool confirm)
	{
		if (!confirm)
		{
			Console.WriteLine("This action will erase all current saved extension packs.");
			Console.Write("Press [Y] to proceed further : ");
			var k = Console.ReadKey();
			if (k.Key != ConsoleKey.Y)
				return;
		}

		Func<ICollection<string>, List<Extension>> stringToExt = (x) => x.Select(y => new Extension() { ExtensionName = y }).ToList();
		var commonExt = new List<ExtensionPack>()
	{
		new (){ Name="Programs", Extensions= stringToExt(new List<string>{"exe", "msi", "jar", "csh", "bat", "reg", "vsix" }) },
		new (){ Name="Videos", Extensions= stringToExt(new List<string>{"mp4", "mkv", "flv", "webm", "vob", "ogv", "drc", "mng", "avi", "ts", "mov", "qt", "wmv", "m4p", "m4v"  }) },
		new (){ Name="Images", Extensions= stringToExt(new List<string>{"png", "jpg", "gif", "jpeg", "svg", "webp", "bmp", "ico", "tif"  }) },
		new (){ Name="Compressed", Extensions= stringToExt(new List<string>{"zip", "rar", "7z", "gz", "iso", "tar", "lz", "lz4"  }) },
		new (){ Name="Audio", Extensions= stringToExt(new List<string>{"mp3", "wav", "flac", "acc", "wma", "weba"}) },
		new (){ Name="Documents", Extensions= stringToExt(new List<string>{"doc", "docx", "pdf", "txt", "csv", "xlsx", "srt"}) },
	};
		_db.Database.EnsureDeleted();
		_db.Database.EnsureCreated();
		_db.AddRange(commonExt);
		_db.SaveChanges();
	}
}
