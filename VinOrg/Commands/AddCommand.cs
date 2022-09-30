namespace VinOrgCLI.Commands;
internal class AddCommand
{
	private readonly SQLiteDatabase _db;

	public AddCommand(SQLiteDatabase db) => _db = db;
	[Command(Aliases = new[] { "a" }, Description = "Add new extensions to pre-existing or new extension pack.")]
	public void Add([Argument] List<string> extensions, [Argument][IsValidPackName] string packName)
	{
		_db.Database.EnsureCreated();


		extensions = extensions.Select(x => x.ToLower()).ToList();
		var invalidExtensions = extensions.Where(x => !Regex.IsMatch(x, "[a-z0-9]"));
		if (invalidExtensions.Any())
		{
			Console.WriteLine("The provided extensions are invalid: ");
			foreach (string ext in invalidExtensions)
				Console.WriteLine(ext);
			return;
		}

		var pack = _db.ExtensionPacks.FirstOrDefault(x => x.Name == packName);
		bool isNew = false;
		if (pack is null)
		{
			pack = new ExtensionPack { Name = packName };
			isNew = true;
		}
		var existingExtensions = _db.Extensions.Where(x => extensions.Contains(x.ExtensionName) && !pack.Extensions.Contains(x)).ToList();
		if (existingExtensions.Count > 0)
		{
			existingExtensions.ForEach(x => extensions.Remove(x.ExtensionName));
			Console.WriteLine("Some of the provided extensions already exist on other extension groups:");
			existingExtensions.ForEach(x => Console.WriteLine(x.ExtensionName));
			Console.Write("Press [Y] to confirm moving, or any other key to ignore: ");
			var k = Console.ReadKey();
			if (k.Key == ConsoleKey.Y)
			{
				existingExtensions.ForEach(x => x.ExtensionPack = pack);
				_db.Extensions.UpdateRange(existingExtensions);
			}

		}

		foreach (string ext in extensions.Where(x => _db.Extensions.FirstOrDefault(y => y.ExtensionName == x) is null))
		{
			var i = new Extension { ExtensionName = ext, ExtensionPack = pack };
			pack.Extensions.Add(i);
			_db.Add(i);
		}
		if (isNew)
			_db.Add(pack);
		else
			_db.Update(pack);
		_db.SaveChanges();
	}
}
