namespace Shared.Logic;
public interface IProgressTracker
{
	public int Current { get; set; }
	public int Max { get;  set; }
	void Increament(int value);
	void SetMax(int max);
}
