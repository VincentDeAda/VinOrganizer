namespace Shared.Models;
public class OrganizeParamsBase
{
	public virtual string? Path { get; set; } = null!;
	public virtual bool Recursive { get; set; }

	public virtual int? RecursionDepth { get; set; }
	public virtual bool MoveUncategorized { get; set; }

	public virtual bool AutoRename { get; set; }

	public virtual bool NoLog { get; set; }

	public virtual bool Steal { get; set; }
}