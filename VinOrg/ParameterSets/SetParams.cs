using Cocona;
namespace VinOrgCLI.ParameterSets;

internal class SetParams : ICommandParameterSet
{
    [Argument]
    public string PackName { get; set; } = null!;
    [Option('p'), HasDefaultValue]
    public string? NewPath { get; set; }
    [Option('n'),HasDefaultValue]
    public string? NewName { get; set; }
    [Option('y')]
    public bool Confirm { get; set; }
}
