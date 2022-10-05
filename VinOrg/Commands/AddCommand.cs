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
		_db.Add(extensions, packName);
	}
}
