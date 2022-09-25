namespace VinOrgCLI.ParameterSets;

internal class SetParams : ICommandParameterSet
{
    [Argument(Description = "The name of the pack requested.")]
    public string PackName { get; set; } = null!;
    [Option('p', Description = "Specify new path for the requested pack."), HasDefaultValue, PathValid]
    public string? NewPath { get; set; }
    [Option('n', Description = "Specify new name for the requested pack."), HasDefaultValue]
    public string? NewName { get; set; }
    [Option('y', Description = "Automatically confirm the requested action.")]
    public bool Confirm { get; set; }
}
