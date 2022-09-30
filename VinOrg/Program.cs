var builder = CoconaApp.CreateBuilder(args);
builder.Services.AddDbContext<SQLiteDatabase>();
var app = builder.Build();
app.AddCommands<AddCommand>();
app.AddCommands<ListCommand>();
app.AddCommands<MergeCommand>();
app.AddCommands<UndoCommand>();
app.AddCommands<LogsCommand>();
app.AddCommands<SetupCommand>();

app.AddSubCommand("set", x =>
{
	x.AddCommands<SetCommand>();
}).WithDescription("Update extension pack name or path.");

app.AddSubCommand("remove", x =>
{
	x.AddCommands<RemoveCommand>();
}).WithDescription("Remove An extension, pack and log file.");


app.Run<OrgCommand>();

