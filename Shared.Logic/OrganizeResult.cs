namespace Shared.Models;
public class OrganizeResult
{
	public int FileMoved { get; set; }
	public int FailedToCreateDir { get; set; }
	public int FailedToMove { get; set; }
	public int Renamed { get; set; }
	public string? LogMD5 { get; set; }
}