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
        x.Path = x.Path ?? "./";
        Console.WriteLine("{0,-12}{1,5}", x.Name, x.Path);
        foreach (var ext in x.Extensions)
            Console.WriteLine("{0,17}", ext.ExtensionName);
    });
});
app.AddCommand("add", async ([FromService] SQLiteDatabase db, [Argument] List<string> extensions, [Argument] string packName) =>
{
    db.Database.EnsureCreated();

    var invalidExtensions = extensions.Where(x => !Regex.IsMatch(x.ToLower(), "[a-z0-9]"));
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
    var existingExtensions = db.Extensions.Where(x => extensions.Contains(x.ExtensionName)).ToList();
    var extList = existingExtensions.ToList();
    if (extList.Count > 0)
    {
        existingExtensions.ForEach(x => extensions.Remove(x.ExtensionName));
        Console.WriteLine(existingExtensions.Count());
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

    foreach (string ext in extensions.Where(x => db.Extensions.FirstOrDefault(y => y.ExtensionName == x) is not null))
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

app.Run();