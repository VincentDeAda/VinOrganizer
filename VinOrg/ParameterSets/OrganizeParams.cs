namespace VinOrgCLI.ParameterSets;
internal class OrganizeParams : ICommandParameterSet
{
	[Option('p',Description ="Path to run the organizing process in."),PathValid]
	public string? Path { get; set; } =null!;
	[Option('s', Description = "Stop printing any errors or fail messages to the console.")]
    public bool SilentMode { get; set; }

    [Option('r', Description = "Enables searching for files in sub folders.")]
    public bool Recursive { get; set; }

    [Option('d', Description = "Specify max recursion depth."),HasDefaultValue]
    public int? RecursionDepth { get; set; }
    [Option('c', Description = "Create folder named \"Uncategorized\" that contain any unrecognized file extension.")]
    public bool MoveUncategorized { get; set; }

    [Option('a', Description = "Automatically rename files that have the same name.")]
    public bool AutoRename { get; set; }
    
    [Option('n', Description = "Organize without logging files.")]
    public bool NoLog { get; set; } 
	
	[Option("steal", Description = "Move the files from the provided path to the current [Require path].")]
    public bool Steal { get; set; }

}
