namespace VinOrgCLI.ParameterSets;
internal class OrganizeParams : OrganizeParamsBase, ICommandParameterSet
{
	[Option('p', Description = "Path to run the organizing process in."), PathValid, HasDefaultValue]
	public override string? Path { get; set; } = null!;
	[Option('r', Description = "Enables searching for files in sub folders.")]
	public override bool Recursive { get; set; }

	[Option('d', Description = "Specify max recursion depth."), HasDefaultValue]
	public override int? RecursionDepth { get; set; }
	[Option('c', Description = "Create folder named \"Uncategorized\" that contain any unrecognized file extension.")]
	public override bool MoveUncategorized { get; set; }

	[Option('a', Description = "Automatically rename files that have the same name.")]
	public override bool AutoRename { get; set; }

	[Option('n', Description = "Organize without logging files.")]
	public override bool NoLog { get; set; }

	[Option('s', Description = "Move the files from the provided path to the current [Require path].")]
	public override bool Steal { get; set; }
}
