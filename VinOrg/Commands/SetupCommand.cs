
namespace VinOrgCLI.Commands;
internal class SetupCommand
{
	private readonly IExtensionsPacksRepository _db;

	public SetupCommand(IExtensionsPacksRepository db) => _db = db;

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

		var commonExt = new List<ExtensionPack>()
	{
		new (){ Name="Programs", Extensions= new List<string>{"exe", "msi", "jar", "csh", "bat", "reg", "vsix" }},
		new (){ Name="Videos", Extensions= new List<string>{"mp4", "mkv", "flv", "webm", "vob", "ogv", "drc", "mng", "avi", "ts", "mov", "qt", "wmv", "m4p", "m4v"  }},
		new (){ Name="Images", Extensions= new List<string>{"png", "jpg", "gif", "jpeg", "svg", "webp", "bmp", "ico", "tif"  }},
		new (){ Name="Compressed", Extensions= new List<string>{"zip", "rar", "7z", "gz", "iso", "tar", "lz", "lz4"  }},
		new (){ Name="Audio", Extensions= new List<string>{"mp3", "wav", "flac", "acc", "wma", "weba"}},
		new (){ Name="Documents", Extensions= new List<string>{"doc", "docx", "pdf", "txt", "csv", "xlsx", "srt"}},
	};
		_db.ExtensionPacks.Clear();
		_db.ExtensionPacks.AddRange(commonExt);
		_db.SaveChanges();
	}
}
