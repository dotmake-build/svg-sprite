using System;
using System.IO;
using DotMake.CommandLine;

namespace DotMake.SvgSprite.Cli.Commands
{
    [CliCommand(Description = "Extract symbols from an SVG sprite file to individual SVG files.")]
    internal class ExtractCliCommand
        : FileCliCommandBase, ISvgSymbolCliOptions, ICliRunWithContextAndReturn
    {
        [CliArgument(
            Description = "The input SVG sprite file to extract symbols from, i.e. to extract <symbol> tags to individual SVG files."
        )]
        public string InputFile { get; set; }

        [CliOption(
            Description = "The output directory to write extracted SVG files.",
            ValidationRules = CliValidationRules.LegalPath
        )]
        public string OutputDirectory { get; set; }

        [CliOption(Description = "Overwrite existing files.")]
        public bool OverwriteExisting { get; set; }

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
            var exitInfo = CheckInputFile(InputFile, out var inputFileInfo);
            if (exitInfo != null)
                return exitInfo.WriteAndReturn();

            exitInfo = CheckOutputDirectory(OutputDirectory, out var outputDirectoryInfo);
            if (exitInfo != null)
                return exitInfo.WriteAndReturn();

            Console.WriteLine();
            Console.WriteLine("Processing...");

            var svgDocument = new SvgDocument(inputFileInfo.FullName);
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

            foreach (var symbolId in svgSpriteBuilder.GetSymbolIds())
            {
                var svgDocumentToExtract = svgSpriteBuilder.ExtractSymbol(symbolId, svgSymbolOptions);

                var svgFile = Path.Combine(outputDirectoryInfo.FullName, Path.ChangeExtension(symbolId, ".svg"));

                exitInfo = CheckOutputFile(svgFile, OverwriteExisting, out _);
                if (exitInfo != null)
                    return exitInfo.WriteAndReturn();

                outputDirectoryInfo.Create();
                svgDocumentToExtract.Save(svgFile, Minify);
            }

            return new CliExitInfo(0).WriteAndReturn();
        }
    }
}
