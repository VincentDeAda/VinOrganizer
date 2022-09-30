namespace VinOrgCLI.Utility;

internal static class ConsoleHelper
{
	private static void PrintLine() => Console.WriteLine(new string('-', Console.WindowWidth));
	public static void PrintLogs(IEnumerable<FileInfo> logs)
	{
		PrintLine();
		Console.WriteLine("|{0,-40}|{1,4}", "Log", "Date");
		PrintLine();

		foreach (var log in logs)
			Console.WriteLine("|{0,-40}|{1,5}", log.Name, log.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
		PrintLine();

	}
	public static void PrintExtensionsPacks(IEnumerable<ExtensionPack> packs)
	{
		PrintLine();
		Console.WriteLine("|{0,-20}|{1,5}", "Pack", "Extensions");
		PrintLine();
		if (packs.Any())
			foreach (var pack in packs)
			{
				Console.Write("|{0,-20}", pack.Name);
				var currentX = Console.GetCursorPosition().Left;
				foreach (var ext in pack.Extensions)
				{
					if (currentX + ext.ExtensionName.Length + 2 >= Console.WindowWidth)
					{
						Console.WriteLine();
						Console.Write("{0,-21}", " ");
					}
					Console.Write("#" + ext.ExtensionName + " ");
					currentX = Console.GetCursorPosition().Left;
				}
				Console.WriteLine();
				if (!string.IsNullOrEmpty(pack.Path))
					Console.WriteLine("|{0}", Path.Combine(pack.Path, pack.Name));
				PrintLine();

			}
	}
}
