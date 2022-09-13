﻿using Cocona;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

var builder = CoconaApp.CreateBuilder();
builder.Services.AddDbContext<SQLiteDatabase>();
var app = builder.Build();



app.AddCommand("list", async ([FromService] SQLiteDatabase db) =>
{
    db.Database.EnsureCreated();

    await db.ExtensionPacks.ForEachAsync(x =>
    {

        x.Path ??= "./";
        Console.WriteLine("{0,-12}{1,5}", x.Name, x.Path);
        foreach (var ext in x.Extensions)
            Console.WriteLine("{0,17}", ext.ExtensionName);
        Console.WriteLine();
    });
});
app.AddCommand("add", ([FromService] SQLiteDatabase db, [Argument] List<string> extensions, [Argument] string packName) =>
{
    db.Database.EnsureCreated();
    extensions = extensions.Select(x => x.ToLower()).ToList();
    var invalidExtensions = extensions.Where(x => !Regex.IsMatch(x,"[a-z0-9]"));
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
        Console.WriteLine(existingExtensions.Count);
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
app.AddSubCommand("set", x =>
{

    x.AddCommand("path", ([FromService] SQLiteDatabase db, [Argument] string path) =>
    {
        try
        {
            Console.WriteLine(Path.IsPathRooted(path));
            Console.WriteLine(Path.GetFullPath(path));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    });
});
app.Run(([FromService] SQLiteDatabase db, [Option('r')] bool recurisve) =>
{
    var currentDir = Directory.GetCurrentDirectory();
    var files = Directory.GetFiles(currentDir
                 , "*", new EnumerationOptions()
                 {
                     IgnoreInaccessible = true,
                     RecurseSubdirectories = recurisve,
                 }).Select(x => new FileInfo(x)).ToList();

    var groupedFiles = files.GroupBy(x => x.Extension);

    foreach (var ext in groupedFiles)
    {
        var packName = db.Extensions.FirstOrDefault(x => x.ExtensionName == ext.Key.Substring(1))?.ExtensionPack;
        if (packName is not null)
        {
            var newDir = Path.Combine(packName.Path ?? currentDir, packName.Name);
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            ext.ToList().ForEach(x =>
            {
                try
                {
                    x.MoveTo(Path.Combine(newDir, x.Name));
                }
                catch (Exception)
                {

                    Console.WriteLine("Couldn't Move File: {0}", x.FullName);
                }
            });
        }
    }

});