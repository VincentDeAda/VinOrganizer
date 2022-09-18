using Cocona;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
            if (currentX + ext.ExtensionName.Length+2 >= Console.WindowWidth)
            {
                Console.WriteLine();
                Console.Write("{0,-21}", " ");
            }
            Console.Write("#"+ext.ExtensionName+" ");
            currentX = Console.GetCursorPosition().Left;
        }
        Console.WriteLine();
        if(!string.IsNullOrEmpty(x.Path))
        Console.WriteLine("|{0}",Path.Combine(x.Path,x.Name));
        Console.WriteLine(new string('-', Console.WindowWidth));

    });
});
app.AddCommand("add", ([FromService] SQLiteDatabase db, [Argument] List<string> extensions, [Argument] string packName) =>
{
    db.Database.EnsureCreated();


    extensions = extensions.Select(x => x.ToLower()).ToList();
    var invalidExtensions = extensions.Where(x => !Regex.IsMatch(x, "[a-z0-9]"));
    if (invalidExtensions.Any())
    {
        Console.WriteLine("The Following Provided Extensions are Invalid: ");
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
        Console.WriteLine("The Following Extensions Already Exist on Other Extension Group:");
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
});

app.AddCommand("setup-common-extensions", ([FromService] SQLiteDatabase db, [Option('y')] bool confirm) =>
{
    if (!confirm)
    {
        Console.WriteLine("This action will erase all current saved extension packs.");
        Console.Write("Press [Y] To Proceed Further : ");
        var k = Console.ReadKey();
        if (k.Key != ConsoleKey.Y)
            return;
    }


    Func<ICollection<string>, List<Extension>> stringToExt = (x) => x.Select(y => new Extension() { ExtensionName = y }).ToList();


    var commonExt = new List<ExtensionPack>()
    {
        new (){ Name="Programs", Extensions= stringToExt(new List<string>{"exe", "msi", "jar", "csh", "bat", "reg", "vsix" }) },
        new (){ Name="Video", Extensions= stringToExt(new List<string>{"mp4", "mkv", "flv", "webm", "vob", "ogv", "drc", "mng", "avi", "ts", "mov", "qt", "wmv", "m4p", "m4v"  }) },
        new (){ Name="Images", Extensions= stringToExt(new List<string>{"png", "jpg", "gif", "jpeg", "svg", "webp", "bmp", "ico", "tif"  }) },
        new (){ Name="Compressed", Extensions= stringToExt(new List<string>{"zip", "rar", "7z", "gz", "iso", "tar", "lz", "lz4"  }) },
        new (){ Name="Audio", Extensions= stringToExt(new List<string>{"mp3", "wav", "flac", "acc", "wma", "weba"}) },
        new (){ Name="Documents", Extensions= stringToExt(new List<string>{"doc", "docx", "pdf", "txt", "csv", "xlsx", "srt"}) },
    };
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    db.AddRange(commonExt);
    db.SaveChanges();
});

app.AddSubCommand("remove", x =>
{

    x.AddCommand("pack", ([FromService] SQLiteDatabase db, [Argument] string packName) =>
    {
        db.Database.EnsureCreated();
        var pack = db.ExtensionPacks.FirstOrDefault(x => x.Name == packName);
        if (pack is null)
        {
            Console.WriteLine("The Provided Pack Name Doesn't Exist.");
            return;
        }
        db.Remove(pack);
        db.SaveChanges();
    });

    x.AddCommand("extensions", ([FromService] SQLiteDatabase db, [Argument] List<string> extensions) =>
    {
        db.Database.EnsureCreated();
        List<Extension> exts = new();

        foreach (string extension in extensions)
        {
            var ext = db.Extensions.FirstOrDefault(x => x.ExtensionName == extension.ToLower());
            if (ext is null)
            {
                Console.WriteLine("Extension {0} Doesn't Exist", extension);
                return;
            }
            exts.Add(ext);
        }
        db.RemoveRange(exts);
        db.SaveChanges();

    });

});

app.AddCommand("set", ([FromService] SQLiteDatabase db, [Argument] string packName, [Option('p')] string? newPath, [Option('n')] string? newName, [Option('y')] bool confirm) =>
{
    db.Database.EnsureCreated();
    var pack = db.ExtensionPacks.FirstOrDefault(x => x.Name == packName);
    if (pack is null)
    {
        Console.WriteLine("The Provided Pack Name Doesn't Exist.");
        return;
    }
    if (newName is not null)
    {
        if (db.ExtensionPacks.FirstOrDefault(x => x.Name == newName) is not null)
        {
            Console.WriteLine("Pack Name Already Used");
            return;
        }
        pack.Name = newName;
    }
    if (newPath is not null)
    {

        bool isValidPath = Path.IsPathFullyQualified(newPath);

        if (!isValidPath)
            Console.WriteLine("Invalid Path");
        else
        {
            var absPath = Path.GetFullPath(newPath);
            if (!confirm)
            {
                Console.Write("Press [Y] to confirm updating the path for {0} to {1}. : ", pack.Name, absPath);
                var k = Console.ReadKey();
                if (k.Key != ConsoleKey.Y)
                    return;
            }
            pack.Path = absPath;
        }

    }



    db.Update(pack);
    db.SaveChanges();

});
app.AddCommand("merge", ([FromService] SQLiteDatabase db, [Argument] List<string> packs, [Argument] string targetPack) =>
{
    List<ExtensionPack> extPacks = new();
    ExtensionPack? target = db.ExtensionPacks.FirstOrDefault(x => x.Name == targetPack);
    if (target is null)
    {
        Console.WriteLine("Target Pack {0} Doesn't Exist");
        return;
    }
    foreach (string pack in packs)
    {
        var extPack = db.ExtensionPacks.FirstOrDefault(x => x.Name == pack);
        if (extPack is null)
        {
            Console.WriteLine("Pack {0} Doesn't Exist.", pack);
            return;
        }
        extPacks.Add(extPack);
    }
    extPacks.ForEach(x => x.Extensions.ForEach(target.Extensions.Add));
    db.Update(target);
    db.RemoveRange(extPacks);
    db.SaveChanges();


});

app.Run(([FromService] SQLiteDatabase db, [Option('r')] bool recurisve, [Option('d')] int? maxRecursionDepth, [Option('c')] bool moveUncategorized) =>
{
    var currentDir = Directory.GetCurrentDirectory();
    var files = Directory.GetFiles(currentDir
                 , "*", new EnumerationOptions()
                 {
                     MaxRecursionDepth = maxRecursionDepth ?? int.MaxValue,
                     IgnoreInaccessible = true,
                     RecurseSubdirectories = recurisve,
                 }).Select(x => new FileInfo(x)).ToList();

    var groupedFiles = files.GroupBy(x => x.Extension.ToLower());

    foreach (var ext in groupedFiles)
    {
        var packName = db.Extensions.FirstOrDefault(x => x.ExtensionName == ext.Key.Substring(1))?.ExtensionPack;
        if (packName is not null || moveUncategorized)
        {
            var newDir = Path.Combine(packName?.Path ?? currentDir, packName?.Name ?? "Uncategorized");
            if (!Directory.Exists(newDir))
                try
                {
                    Directory.CreateDirectory(newDir);
                }
                catch (Exception e)
                {

                    Console.WriteLine("Failed To Create {0}", newDir);
                    Console.WriteLine("Error Message:\n{0}", e.Message);
                    continue;
                }
            foreach (FileInfo file in CollectionsMarshal.AsSpan(ext.ToList()))
            {
                try
                {
                    file.MoveTo(Path.Combine(newDir, file.Name));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Couldn't Move File: {0}", file.FullName);
                }
            }
        }
    }

});