namespace VinOrgCLI.Commands;
internal class SetCommand
{
	private readonly SQLiteDatabase _db;

	public SetCommand(SQLiteDatabase db) => _db = db;
	[Command(Aliases = new[] { "s" },Description = "Update extension pack name or path.")]
	public void Set([FromService] SQLiteDatabase _db, SetParams paramSet)
	{

		if (paramSet.NewPath is null && paramSet.NewName is null)
		{
			Console.WriteLine("There's no action requested.");
			return;
		}
		_db.Database.EnsureCreated();
		var pack = _db.ExtensionPacks.FirstOrDefault(x => x.Name == paramSet.PackName);
		if (pack is null)
		{
			Console.WriteLine("The provided pack name doesn't exist.");
			return;
		}
		if (paramSet.NewName is not null)
		{
			if (_db.ExtensionPacks.FirstOrDefault(x => x.Name == paramSet.NewName) is not null)
			{
				Console.WriteLine("Pack name already used");
				return;
			}
			pack.Name = paramSet.NewName;
		}
		if (paramSet.NewPath is not null)
		{
			var absPath = Path.GetFullPath(paramSet.NewPath);
			if (!paramSet.Confirm)
			{
				Console.Write("Press [Y] to confirm updating the path for {0} to {1}. : ", pack.Name, absPath);
				var k = Console.ReadKey();
				if (k.Key != ConsoleKey.Y)
					return;
			}
			pack.Path = absPath;
		}
		_db.Update(pack);
		_db.SaveChanges();
	}
}
