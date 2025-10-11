// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using DotMake.CommandLine;
using DotMake.SvgSprite;
using DotMake.SvgSprite.Cli.Commands;

var argsOrCommandLine = args;
//var argsOrCommandLine = @"b ..\..\..\..\..\publish\test-files\set1\*  -o ..\..\..\..\..\publish\test-files\set1_sprite_cli.svg";
//var argsOrCommandLine = @"b ..\..\..\..\..\publish\test-files\set12 -o ..\..\..\..\..\publish\test-files\set1_sprite_cli.svg";
Cli.Run<RootCliCommand>(argsOrCommandLine, new CliSettings {EnableDefaultExceptionHandler = true});

const string testFilesPath = @"D:\Development\Projects\DotMake-Build\svg-sprite\publish\test-files";

/*
var invalidFiles = new []
{
    "_invalid1.svg",
    "_invalid2.svg",
    "_invalid3.svg"
};

foreach (var invalidFile in invalidFiles)
{
    try
    {
        var svgDocument = new SvgDocument(Path.Combine(testFilesPath, invalidFile));
        svgDocument.Save(Path.Combine(testFilesPath, "_output" + invalidFile));
        Console.WriteLine($"Save success for {invalidFile}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Save failed for {invalidFile}: {e.Message}");
    }
}
*/

var svgSymbolOptions = new SvgSymbolOptions
{
    //AttributesToPreserve = new []{"viewbox", "id"},
    IdPrefix = "icon-",
    IdForMissing = "coco",
    //IdReplacementChar = '.',
    //IdLowerCased = false
};

/*
var svgDocument = new SvgDocument();
var svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);

foreach (var file in Directory.EnumerateFiles(Path.Combine(testFilesPath, "set1")))
{
    var svgDocumentToAdd = new SvgDocument(file);
    string symbolId = Path.GetFileNameWithoutExtension(file);
    //string symbolId = null;
    svgSpriteBuilder.AddSymbol(svgDocumentToAdd, symbolId, svgSymbolOptions);
}
svgDocument.Save(Path.Combine(testFilesPath, "set1_sprite.svg"));
svgDocument.Save(Path.Combine(testFilesPath, "set1_sprite_noformat.svg"), disableFormatting: true);

svgDocument = new SvgDocument(Path.Combine(testFilesPath, "set1_sprite.svg"));
svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);
//svgDocument.RemoveSymbol("icon-coco-2");
//svgDocument.RemoveSymbol("icon-coco-8");
svgSpriteBuilder.RemoveSymbol("icon-angle-down");
foreach (var file in Directory.EnumerateFiles(Path.Combine(testFilesPath, "set1")))
{
    var svgDocumentToAdd = new SvgDocument(file);
    string symbolId = Path.GetFileNameWithoutExtension(file);
    //string symbolId = null;
    svgSpriteBuilder.AddSymbol(svgDocumentToAdd, symbolId, svgSymbolOptions);
}
svgDocument.Save(Path.Combine(testFilesPath, "set1_sprite2.svg"));
*/

/*
var svgDocument = new SvgDocument(Path.Combine(testFilesPath, "set1_sprite.svg"));
var svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);
var extractPath = Path.Combine(testFilesPath, "set1_extracted");
Directory.CreateDirectory(extractPath);

foreach (var symbol in svgSpriteBuilder.GetSymbols())
{
    var symbolId = symbol.Attribute("id")?.Value;

    var svgDocumentToExtract = svgSpriteBuilder.ExtractSymbol(symbolId, svgSymbolOptions);

    svgDocumentToExtract.Save(Path.Combine(extractPath, symbolId + ".svg"));
}
*/
