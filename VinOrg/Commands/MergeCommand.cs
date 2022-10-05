namespace VinOrgCLI.Commands;
internal class MergeCommand
{
	private readonly IExtensionsPacksRepository _db;
	public MergeCommand(IExtensionsPacksRepository db) => _db = db;


	[Command(Aliases = new[] { "m" }, Description = "Merge one or more extension pack to a pre-existing or new one.")]
	public void Merge([Argument] List<string> packs, [Argument] string targetPack)
	{
		var res = _db.Merge(packs, targetPack);

		if (res != null)
		{
			Console.WriteLine("Couldn't find the following packs to merge: ");
			res.ForEach(Console.WriteLine);
		}
		
	}

}