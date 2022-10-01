namespace VinOrgCLI.Commands;
internal class AddCommand
{
	private readonly IExtensionsPacksRepository _db;

	public AddCommand(IExtensionsPacksRepository db) => _db = db;
	[Command(Aliases = new[] { "a" }, Description = "Add new extensions to pre-existing or new extension pack.")]
	public void Add([Argument] List<string> extensions, [Argument][IsValidPackName] string packName)
	{
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
		var existingExtensions = _db.Extensions.Where(x => extensions.Contains(x) && !pack.Extensions.Contains(x)).ToList();
		if (existingExtensions.Count > 0)
		{
			existingExtensions.ForEach(x => extensions.Remove(x));
			Console.WriteLine("Some of the provided extensions already exist on other extension groups:");
			existingExtensions.ForEach(x => Console.WriteLine(x));
			Console.Write("Press [Y] to confirm moving, or any other key to ignore: ");
			var k = Console.ReadKey();
			if (k.Key == ConsoleKey.Y)
			{
				foreach (string ext in existingExtensions)
				{
					var containingPack = _db.ExtensionPacks.First(x => x.Extensions.Contains(ext));
					containingPack.Extensions.Remove(ext);
					pack.Extensions.Add(ext); 

				}
				_db.SaveChanges();
			}

		}

		foreach (string ext in extensions.Where(x => _db.Extensions.FirstOrDefault(y => y == x) is null))
		{
			pack.Extensions.Add(ext);
		}
		if (isNew)
			_db.ExtensionPacks.Add(pack);
		_db.SaveChanges();
	}
}
