#if DEBUG
using System.IO;
using System.Xml;
// ReSharper disable LocalVariableHidesMember
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantAssignment

namespace DotMake.SvgSprite
{
    internal class UsageExamples
    {
        // ReSharper disable once NotAccessedField.Local
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private Stream stream;
        private TextReader textReader;
        private XmlReader xmlReader;
        private string svgString;
        private TextWriter textWriter;
        private XmlWriter xmlWriter;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        public void SvgDocumentExamples()
        {
            #region SvgLoading

            SvgDocument svgDocument;

            //Create an empty SVG document
            svgDocument = new SvgDocument();

            //Load an SVG document from a file
            svgDocument = new SvgDocument("SomeFile.svg");

            //Load an SVG document from a stream
            svgDocument = new SvgDocument(stream);

            //Load an SVG document from a text reader
            svgDocument = new SvgDocument(textReader);

            //Load an SVG document from a xml reader
            svgDocument = new SvgDocument(xmlReader);

            //Load an SVG document from a string
            svgDocument = SvgDocument.Parse(svgString);

            #endregion

            #region SvgSaving

            //Saves this SVG to a file
            svgDocument.Save("SomeFile.svg");
            //Save minified
            svgDocument.Save("SomeFile.svg", true);

            //Saves this SVG to a stream
            svgDocument.Save(stream);
            //Save minified
            svgDocument.Save(stream, true);

            //Saves this SVG to a text writer
            svgDocument.Save(textWriter);
            //Save minified
            svgDocument.Save(textWriter, true);

            //Saves this SVG to a xml writer
            svgDocument.Save(xmlWriter);
            //Save minified
            svgDocument.Save(xmlWriter, true);

            //Saves this SVG to a string
            svgString = svgDocument.ToString();
            //Save minified
            svgString = svgDocument.ToString(true);

            #endregion
        }

        public void SvgSpriteBuilding()
        {
            #region SvgSpriteBuilding

            //Build an SVG sprite file from input SVG files

            var svgDocument = new SvgDocument();
            var svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);

            foreach (var file in Directory.EnumerateFiles(@"inputs\", "*.svg"))
            {
                var svgDocumentToAdd = new SvgDocument(file);
                var symbolId = Path.GetFileNameWithoutExtension(file);

                svgSpriteBuilder.AddSymbol(svgDocumentToAdd, symbolId);
            }

            svgDocument.Save(@"sprite.svg");

            #endregion
        }

        public void SvgSpriteBuildingWithOptions()
        {
            #region SvgSpriteBuildingWithOptions

            //Build an SVG sprite file from input SVG files with custom options

            var svgDocument = new SvgDocument();
            var svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);
            var svgSymbolOptions = new SvgSymbolOptions
            {
                //These are default values for options
                AttributesToPreserve = new[] { "id", "viewBox", "class", "style", "fill", "stroke", "opacity", "transform" },
                ElementsToPreserve = new[] { "*" },
                IdForMissing = "symbol",
                IdReplacementChar = '-',
                IdLowerCased = true
            };

            foreach (var file in Directory.EnumerateFiles(@"inputs\", "*.svg"))
            {
                var svgDocumentToAdd = new SvgDocument(file);
                var symbolId = Path.GetFileNameWithoutExtension(file);

                svgSpriteBuilder.AddSymbol(svgDocumentToAdd, symbolId, svgSymbolOptions);
            }

            svgDocument.Save(@"sprite.svg");

            #endregion
        }
        
        public void SvgSpriteExtracting()
        {
            #region SvgSpriteExtracting

            //Extract symbols from an SVG sprite file to individual SVG files

            var svgDocument = new SvgDocument(@"sprite.svg");
            var svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);
            var outputDirectory = @"outputs\";

            foreach (var symbolId in svgSpriteBuilder.GetSymbolIds())
            {
                var svgDocumentToExtract = svgSpriteBuilder.ExtractSymbol(symbolId);
                var svgFile = Path.Combine(outputDirectory, Path.ChangeExtension(symbolId, ".svg"));

                Directory.CreateDirectory(outputDirectory);
                svgDocumentToExtract.Save(svgFile);
            }

            #endregion
        }

        public void SvgSpriteExtractingWithOptions()
        {
            #region SvgSpriteExtractingWithOptions

            //Extract symbols from an SVG sprite file to individual SVG files with custom options

            var svgDocument = new SvgDocument(@"sprite.svg");
            var svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);
            var svgSymbolOptions = new SvgSymbolOptions
            {
                //These are default values for options
                AttributesToPreserve = new[] { "id", "viewBox", "class", "style", "fill", "stroke", "opacity", "transform" },
                ElementsToPreserve = new[] { "*" },
                IdForMissing = "symbol",
                IdReplacementChar = '-',
                IdLowerCased = true
            };
            var outputDirectory = @"outputs\";

            foreach (var symbolId in svgSpriteBuilder.GetSymbolIds())
            {
                var svgDocumentToExtract = svgSpriteBuilder.ExtractSymbol(symbolId, svgSymbolOptions);
                var svgFile = Path.Combine(outputDirectory, Path.ChangeExtension(symbolId, ".svg"));

                Directory.CreateDirectory(outputDirectory);
                svgDocumentToExtract.Save(svgFile);
            }

            #endregion
        }

        public void SvgSpriteCreatingHtmlPreview()
        {
            #region SvgSpriteCreatingHtmlPreview

            //Create an HTML page for previewing the symbols inside the SVG sprite

            var svgDocument = new SvgDocument(@"sprite.svg");
            var svgSpriteBuilder = new SvgSpriteBuilder(svgDocument);

            var html = svgSpriteBuilder.CreatePreviewPage();
            File.WriteAllText(@"preview.html", html);

            #endregion
        }
    }
}
#endif
