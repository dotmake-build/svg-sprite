using System;
using System.IO;
using DotMake.CommandLine;

namespace DotMake.SvgSprite.Cli.Commands
{
    [CliCommand(Description = "Build an SVG sprite file from input SVG files.")]
    internal class BuildCliCommand
        : FileCliCommandBase, ISvgSymbolCliOptions, ICliRunWithContextAndReturn
    {
        [CliArgument(
            Description = "The input SVG files to build an SVG sprite file, i.e. to add as <symbol> tags to the output SVG sprite file." +
                          " Duplicate files (same name and size and date modified) will be ignored." +
                          "\n" + PathPatternsDescription + "\n"
        )]
        public string[] InputFiles { get; set; }

        [CliOption(
            Description = "The output SVG sprite file which will contain added <symbol> tags.",
            ValidationRules = CliValidationRules.LegalPath,
            Alias = "o"
        )]
        public string OutputFile { get; set; }

        [CliOption(Description = "Overwrite existing files.")]
        public bool OverwriteExisting { get; set; }

        [CliOption(Description = "Create an HTML page next to the output SVG sprite file, for previewing the symbols inside the SVG sprite.")]
        public bool HtmlPreview { get; set; }

        [CliOption(Description = "Minify the SVG output, i.e. disable formatting output, not write individual elements on new lines and indent.")]
        public bool Minify { get; set; }


        public string[] AttributesToDiscard { get; set; } = SvgSymbolOptions.Default.AttributesToDiscard;
        public string[] ElementsToDiscard { get; set; } = SvgSymbolOptions.Default.ElementsToDiscard;
        public string IdPrefix { get; set; } = SvgSymbolOptions.Default.IdPrefix;
        public string IdForMissing { get; set; } = SvgSymbolOptions.Default.IdForMissing;
        public char IdReplacementChar { get; set; } = SvgSymbolOptions.Default.IdReplacementChar;
        public bool IdLowerCased { get; set; } = SvgSymbolOptions.Default.IdLowerCased;
        public string ViewBoxOverride { get; set; } = SvgSymbolOptions.Default.ViewBoxOverride;
        public bool CommentsPreserved { get; set; } = SvgSymbolOptions.Default.CommentsPreserved;


        public int Run(CliContext cliContext)
        {
            var exitInfo = CheckInputFiles(InputFiles, true, out var inputFileInfos);
            if (exitInfo != null)
                return exitInfo.WriteAndReturn();

            exitInfo = CheckOutputFile(OutputFile, OverwriteExisting, out var outputFileInfo);
            if (exitInfo != null)
                return exitInfo.WriteAndReturn();

            Console.WriteLine();
            Console.WriteLine("Processing...");

            var svgDocument = new SvgDocument();
            var svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);
            var svgSymbolOptions = new SvgSymbolOptions
            {
                AttributesToDiscard = AttributesToDiscard,
                ElementsToDiscard = ElementsToDiscard,
                IdPrefix = IdPrefix,
                IdForMissing = IdForMissing,
                IdReplacementChar = IdReplacementChar,
                IdLowerCased = IdLowerCased,
                ViewBoxOverride = ViewBoxOverride,
                CommentsPreserved = CommentsPreserved,
            };

            foreach (var fileInfo in inputFileInfos)
            {
                var svgDocumentToAdd = new SvgDocument(fileInfo.FullName);
                svgSpriteBuilder.AddSymbol(svgDocumentToAdd, Path.GetFileNameWithoutExtension(fileInfo.Name), svgSymbolOptions);
            }

            outputFileInfo.Directory?.Create();
            var svgFile = (outputFileInfo.Extension.Length == 0)
                ? Path.ChangeExtension(outputFileInfo.FullName, ".svg")
                : outputFileInfo.FullName;
            svgDocument.Save(svgFile, Minify);

            if (HtmlPreview)
            {
                var html = svgSpriteBuilder.CreatePreviewPage();
                var htmlFile = Path.ChangeExtension(svgFile, ".html");
                File.WriteAllText(htmlFile, html);
            }

            return new CliExitInfo(0).WriteAndReturn();
        }

    }
}
