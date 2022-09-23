using Cocona;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using VinOrgCLI.ParameterSets;
using VinOrgCLI.Validation;

var builder = CoconaApp.CreateBuilder();
builder.Services.AddDbContext<SQLiteDatabase>();
var app = builder.Build();



app.AddCommand("list", async ([FromService] SQLiteDatabase db) =>
{
    db.Database.EnsureCreated();

    Console.WriteLine(new string('-', Console.WindowWidth));
    Console.WriteLine("|{0,-20}|{1,5}", "Pack", "Extensions");
    Console.WriteLine(new string('-', Console.WindowWidth));
    await db.ExtensionPacks.ForEachAsync(x =>
    {


        Console.Write("|{0,-20}", x.Name);
        var currentX = Console.GetCursorPosition().Left;
        foreach (var ext in x.Extensions)
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
        if (!string.IsNullOrEmpty(x.Path))
            Console.WriteLine("|{0}", Path.Combine(x.Path, x.Name));
        Console.WriteLine(new string('-', Console.WindowWidth));

    });
}).WithDescription("List all extensions packs with their extensions.").WithAliases("ls");
app.AddCommand("add", ([FromService] SQLiteDatabase db, [Argument] List<string> extensions, [Argument][IsValidPackName] string packName) =>
{
    db.Database.EnsureCreated();


    extensions = extensions.Select(x => x.ToLower()).ToList();
    var invalidExtensions = extensions.Where(x => !Regex.IsMatch(x, "[a-z0-9]"));
    if (invalidExtensions.Any())
    {
        Console.WriteLine("The provided extensions are invalid: ");
        foreach (string ext in invalidExtensions)
            Console.WriteLine(ext);
        return;
    }

    var pack = db.ExtensionPacks.FirstOrDefault(x => x.Name == packName);
    bool isNew = false;
    if (pack is null)
    {
        pack = new ExtensionPack { Name = packName };
        isNew = true;
    }
    var existingExtensions = db.Extensions.Where(x => extensions.Contains(x.ExtensionName) && !pack.Extensions.Contains(x)).ToList();
    if (existingExtensions.Count > 0)
    {
        existingExtensions.ForEach(x => extensions.Remove(x.ExtensionName));
        Console.WriteLine("Some of the provided extensions already exist on other extension groups:");
        existingExtensions.ForEach(x => Console.WriteLine(x.ExtensionName));
        Console.Write("Press [Y] to confirm moving, or any other key to ignore: ");
        var k = Console.ReadKey();
        if (k.Key == ConsoleKey.Y)
        {
            existingExtensions.ForEach(x => x.ExtensionPack = pack);
            db.Extensions.UpdateRange(existingExtensions);


        }

    }

    foreach (string ext in extensions.Where(x => db.Extensions.FirstOrDefault(y => y.ExtensionName == x) is null))
    {
        var i = new Extension { ExtensionName = ext, ExtensionPack = pack };
        pack.Extensions.Add(i);
        db.Add(i);
    }
    if (isNew)
        db.Add(pack);
    else
        db.Update(pack);
    db.SaveChanges();
}).WithDescription("Add new extensions to pre-existing or new extension pack.").WithAliases("a");

app.AddCommand("setup-common-extensions", ([FromService] SQLiteDatabase db, [Option('y')] bool confirm) =>
{
    if (!confirm)
    {
        Console.WriteLine("This action will erase all current saved extension packs.");
        Console.Write("Press [Y] to proceed further : ");
        var k = Console.ReadKey();
        if (k.Key != ConsoleKey.Y)
            return;
    }

    Func<ICollection<string>, List<Extension>> stringToExt = (x) => x.Select(y => new Extension() { ExtensionName = y }).ToList();
    var commonExt = new List<ExtensionPack>()
    {
        new (){ Name="Programs", Extensions= stringToExt(new List<string>{"exe", "msi", "jar", "csh", "bat", "reg", "vsix" }) },
        new (){ Name="Videos", Extensions= stringToExt(new List<string>{"mp4", "mkv", "flv", "webm", "vob", "ogv", "drc", "mng", "avi", "ts", "mov", "qt", "wmv", "m4p", "m4v"  }) },
        new (){ Name="Images", Extensions= stringToExt(new List<string>{"png", "jpg", "gif", "jpeg", "svg", "webp", "bmp", "ico", "tif"  }) },
        new (){ Name="Compressed", Extensions= stringToExt(new List<string>{"zip", "rar", "7z", "gz", "iso", "tar", "lz", "lz4"  }) },
        new (){ Name="Audio", Extensions= stringToExt(new List<string>{"mp3", "wav", "flac", "acc", "wma", "weba"}) },
        new (){ Name="Documents", Extensions= stringToExt(new List<string>{"doc", "docx", "pdf", "txt", "csv", "xlsx", "srt"}) },
    };
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    db.AddRange(commonExt);
    db.SaveChanges();
}).WithDescription("Add all known and most used extension types to the database.");

app.AddSubCommand("remove", x =>
{

    x.AddCommand("pack", ([FromService] SQLiteDatabase db, [Argument] string packName) =>
    {
        db.Database.EnsureCreated();
        var pack = db.ExtensionPacks.FirstOrDefault(x => x.Name == packName);
        if (pack is null)
        {
            Console.WriteLine("The provided pack name doesn't exist.");
            return;
        }
        db.Remove(pack);
        db.SaveChanges();
    }).WithAliases("p");

    x.AddCommand("extensions", ([FromService] SQLiteDatabase db, [Argument] List<string> extensions) =>
    {
        db.Database.EnsureCreated();
        List<Extension> exts = new();
        foreach (string extension in extensions)
        {
            var ext = db.Extensions.FirstOrDefault(x => x.ExtensionName == extension.ToLower());
            if (ext is not null)
                exts.Add(ext);
        }
        db.RemoveRange(exts);
        db.SaveChanges();

    }).WithAliases("e");

}).WithDescription("Remove packs or extensions from the database.").WithAliases("r");

app.AddCommand("set", ([FromService] SQLiteDatabase db, SetParams paramSet) =>
{
    if (paramSet.NewPath is null && paramSet.NewName is null)
    {
        Console.WriteLine("There's no action requested.");
        return;
    }
    db.Database.EnsureCreated();
    var pack = db.ExtensionPacks.FirstOrDefault(x => x.Name == paramSet.PackName);
    if (pack is null)
    {
        Console.WriteLine("The provided pack name doesn't exist.");
        return;
    }
    if (paramSet.NewName is not null)
    {
        if (db.ExtensionPacks.FirstOrDefault(x => x.Name == paramSet.NewName) is not null)
        {
            Console.WriteLine("Pack name already used");
            return;
        }
        pack.Name = paramSet.NewName;
    }
    if (paramSet.NewPath is not null)
    {
        var absPath = Path.GetFullPath(paramSet.NewPath);
        if (!paramSet.Confirm)
        {
            Console.Write("Press [Y] to confirm updating the path for {0} to {1}. : ", pack.Name, absPath);
            var k = Console.ReadKey();
            if (k.Key != ConsoleKey.Y)
                return;
        }
        pack.Path = absPath;
    }
    db.Update(pack);
    db.SaveChanges();

}).WithDescription("Update extension pack name or path.").WithAliases("s");
app.AddCommand("merge", ([FromService] SQLiteDatabase db, [Argument] List<string> packs, [Argument] string targetPack) =>
{
    List<ExtensionPack> extPacks = new();
    ExtensionPack? target = db.ExtensionPacks.FirstOrDefault(x => x.Name == targetPack);
    if (target is null)
    {
        Console.WriteLine("The provided target Pack \"{0}\" doesn't exist", target);
        return;
    }
    foreach (string pack in packs)
    {
        var extPack = db.ExtensionPacks.FirstOrDefault(x => x.Name == pack);
        if (extPack is null)
        {
            Console.WriteLine("The provided pack \"{0}\" doesn't exist in the database.", pack);
            return;
        }
        extPacks.Add(extPack);
    }
    extPacks.ForEach(x => x.Extensions.ForEach(target.Extensions.Add));
    db.Update(target);
    db.RemoveRange(extPacks);
    db.SaveChanges();

}).WithDescription("Merge one or more extension pack to a pre-existing one.").WithAliases("m");

app.Run(([FromService] SQLiteDatabase db, OrganizeParams paramSet) =>
{
    var currentDir = Directory.GetCurrentDirectory();
    List<Environment.SpecialFolder> systemDirs = new List<Environment.SpecialFolder>() {
        Environment.SpecialFolder.ApplicationData,
        Environment.SpecialFolder.LocalApplicationData,
        Environment.SpecialFolder.CommonApplicationData,
        Environment.SpecialFolder.Windows,
        Environment.SpecialFolder.System,
        Environment.SpecialFolder.SystemX86,
        Environment.SpecialFolder.ProgramFiles,
        Environment.SpecialFolder.ProgramFilesX86,

    };
    bool isSystemDir = systemDirs.Any(x => currentDir == Environment.GetFolderPath(x));
    string sysDir = string.IsNullOrWhiteSpace(Environment.SystemDirectory) ? "/" : Environment.SystemDirectory;
    bool isSystemRoot = Directory.GetDirectoryRoot(sysDir) == currentDir;
    if (isSystemDir || isSystemRoot)
    {
        Console.WriteLine("Using the application within this directory will cause system malfunction. task aborted.");
        return;
    }

    var files = Directory.GetFiles(currentDir
                 , "*", new EnumerationOptions()
                 {
                     MaxRecursionDepth = paramSet.RecursionDepth ?? int.MaxValue,
                     IgnoreInaccessible = true,
                     RecurseSubdirectories = paramSet.Recursive,
                 }).Select(x => new FileInfo(x)).ToList();

    var groupedFiles = files.GroupBy(x => x.Extension.ToLower());

    foreach (var ext in groupedFiles)
    {
        var packName = db.Extensions.FirstOrDefault(x => x.ExtensionName == ext.Key.Substring(1))?.ExtensionPack;
        if (packName is not null || paramSet.MoveUncategorized)
        {
            var newDir = Path.Combine(packName?.Path ?? currentDir, packName?.Name ?? "Uncategorized");
            if (!Directory.Exists(newDir))
                try
                {
                    Directory.CreateDirectory(newDir);
                }
                catch (Exception e)
                {

                    Console.WriteLine("Failed to create {0}", newDir);
                    Console.WriteLine("Error message: {0}", e.Message);
                    continue;
                }
            int renamed = 0;
            foreach (FileInfo file in CollectionsMarshal.AsSpan(ext.ToList()))
            {
                try
                {
                    file.MoveTo(Path.Combine(newDir, file.Name));
                }
                catch (Exception e)
                {
                    if (paramSet.AutoRename && e is System.IO.IOException)
                    {

                        file.MoveTo(Path.Combine(newDir, string.Format("{0}-{1}", file.CreationTime.ToString("yyyyMMddHHmmssff"), file.Name)));
                        renamed++;



                    }
                    if (paramSet.SilentMode) continue;
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Couldn't move file: {0}", file.FullName);
                }
            }
            if (paramSet.AutoRename && renamed > 0) Console.WriteLine("{0} Files Renamed.", renamed);
        }
    }

});