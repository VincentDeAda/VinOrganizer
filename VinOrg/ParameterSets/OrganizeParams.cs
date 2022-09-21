using Cocona;
namespace VinOrgCLI.ParameterSets;

internal class OrganizeParams : ICommandParameterSet
{
    [Option('s')]
    public bool SilentMode { get; set; }

    [Option('r')]
    public bool Recursive { get; set; }

    [Option('c')]
    public bool MoveUncategorized { get; set; }

    [Option('a')]
    public bool AutoRename { get; set; }

    [Option('d'),HasDefaultValue]
    public int? RecursionDepth { get; set; }
}
