namespace Shared.Logic;
public static class Paths
{
    public static string ConfigDir
    {
        get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VDA", "VinOrg");
    }
    public static string LogDir
    {
        get => Path.Combine(ConfigDir, "Logs");
    }
}
