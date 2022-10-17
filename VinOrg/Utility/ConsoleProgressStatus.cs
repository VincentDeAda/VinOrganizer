namespace VinOrgCLI.Utility;
internal class ConsoleProgressStatus : IProgressTracker
{
	private int _current;
	private string GetPercentage() => ((double)Current / Max).ToString("0.00%");
	public int Current
	{
		get => _current; set
		{
			_current = value;
			OnChange();
		}
	}
	public int Max { get; set; }

	public void Increament(int value)
	{
		Current += value;
	}

	public void SetMax(int max)
	{
		Max = max;
	}

	public void OnChange()
	{
		var pos = Console.GetCursorPosition();
		Console.WriteLine("Progress: {0}", GetPercentage());
		Console.WriteLine("Total Files iterated: {0}/{1}", Current, Max);
		Console.WriteLine();
		if (Current != Max)
			Console.SetCursorPosition(pos.Left, pos.Top);


	}

}
