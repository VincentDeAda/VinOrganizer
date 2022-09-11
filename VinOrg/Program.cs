using Cocona;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

var builder = CoconaApp.CreateBuilder();
builder.Services.AddDbContext<SQLiteDatabase>();
var app = builder.Build();



app.AddSubCommand("list", (x) =>
{
    x.AddCommand("extensions", async ([FromService] SQLiteDatabase db, [Option('p', Description = "The name of extension pack to list containing extensions")] string? packName) =>
    {
        db.Database.EnsureCreated();
        if (packName is null)
            await db.Extensions.ForEachAsync(x => Console.WriteLine(x.ExtensionName));
        else
        {
            var exts = db.Extensions.Where(x => x.ExtensionPack.Name == packName);
            if (exts.Count() == 0)
                Console.WriteLine("Pack name doesn't exist");
            else await exts.ForEachAsync(x => Console.WriteLine(x.ExtensionName));
        }
    }).WithAliases("ext", "exts", "ex", "e").WithDescription("List extensions");

    x.AddCommand("packs", async ([FromService] SQLiteDatabase db, [Option('e')] bool listExtensions) =>
    {
        if (listExtensions)
        {
            await db.ExtensionPacks.ForEachAsync(x =>
             {
                 Console.WriteLine("{0} {1:5}", x.Name, x.Path ?? "./");
                 x.Extensions?.ForEach(x => Console.WriteLine("{0:-5}", x));
                 Console.WriteLine();
             });
        }
        else await db.ExtensionPacks.ForEachAsync(x => Console.WriteLine(x.Name));
    }).WithAliases("p", "pa", "pack").WithDescription("List packs"); ;
});

app.AddSubCommand("add", x =>
{

    x.AddCommand("extensions", ([FromService] SQLiteDatabase db, [Option('e', Description = "The Extensions you want to add")] string[] extensions, [Option('p', Description = "The name of parent pack. if ignored extensions will be added to the \"uncategorized\" Pack")] string? parentPack) =>
    {
        parentPack = parentPack ?? "Uncategorized";
        var isValid = extensions.All(x => Regex.IsMatch(x.ToLower(), "[a-z0-9]"));
        if (isValid)
        {
            db.Database.EnsureCreated();
            var isUnique = extensions.All(x => db.Extensions.FirstOrDefault(y => y.ExtensionName == x.ToLower()) is null);
            if (isUnique)
            {

                var pack = db.ExtensionPacks.FirstOrDefault(x => x.Name == parentPack);
                if (pack is null)
                {
                    pack = new ExtensionPack
                    {
                        Name = parentPack,
                    };
                }
                db.AddRange(extensions.Select(x => new Extension { ExtensionPack = pack, ExtensionName = x }));
                db.SaveChanges();
            }
            else
            {
                Console.WriteLine("Some values aren't unique");
            }

        }
        else
        {
            Console.WriteLine("Values contain invalid extension name");
        }
    }).WithAliases("ext", "exts", "ex", "e");


});


//app.AddCommand("list", async ([FromService] SQLiteDatabase db) =>
//{
//    await db.Database.EnsureCreatedAsync();

//    await db.AddAsync<ExtensionPack>(new()
//    {
//        Name = "Programs",
//        Extensions = new List<Extension> {
//            new(){ ExtensionName=  "exe"},
//            new(){ ExtensionName=  "msi"},
//    }
//    });

//    db.SaveChanges();
//    await db.ExtensionPacks.ForEachAsync(x =>
//    {

//        Console.WriteLine(x.Id);
//        Console.WriteLine(x.Name);
//        x.Extensions.ForEach(Console.WriteLine);

//    });

//});



app.Run();