var builder = CoconaLiteApp.CreateBuilder( );
builder.Services.AddSingleton<IExtensionsPacksRepository>(x=>new JsonDatabase(Paths.ConfigDir));
builder.Services.AddSingleton<IProgressTracker,ConsoleProgressStatus>();
var app = builder.Build();
app.AddCommands<AddCommand>();
app.AddCommands<MergeCommand>();
app.AddCommands<UndoCommand>();
app.AddCommands<SetupCommand>();

app.AddSubCommand("list", x =>
{
	x.AddCommands<ListCommand>();
}).WithDescription("List extensions or logs.");
app.AddSubCommand("set", x =>
{
	x.AddCommands<SetCommand>();
}).WithDescription("Update extension pack name or path.");

app.AddSubCommand("remove", x =>
{
	x.AddCommands<RemoveCommand>();
}).WithDescription("Remove An extension, pack or log file.");


app.Run<OrgCommand>();

