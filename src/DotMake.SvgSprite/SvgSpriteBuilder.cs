using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace DotMake.SvgSprite
{
    /// <summary>
    /// Handles building or extracting of an SVG sprite, i.e. a .svg file with child <c>&lt;symbol&gt;</c> tags.
    /// </summary>
    /// <example>
    ///     <code source="../DotMake.SvgSprite/UsageExamples.cs" region="SvgSpriteBuilding" language="cs" />
    ///     <code source="../DotMake.SvgSprite/UsageExamples.cs" region="SvgSpriteBuildingWithOptions" language="cs" />
    ///     <code source="../DotMake.SvgSprite/UsageExamples.cs" region="SvgSpriteExtracting" language="cs" />
    ///     <code source="../DotMake.SvgSprite/UsageExamples.cs" region="SvgSpriteExtractingWithOptions" language="cs" />
    ///     <code source="../DotMake.SvgSprite/UsageExamples.cs" region="SvgSpriteCreatingHtmlPreview" language="cs" />
    /// </example>
    public class SvgSpriteBuilder
    {
        private readonly SvgDocument currentSvgDocument;
        private readonly Dictionary<string, HashSet<int>> idIndexes = new(StringComparer.Ordinal); //IDs are case-sensitive

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgSpriteBuilder" /> class.
        /// </summary>
        public SvgSpriteBuilder()
        {
            currentSvgDocument = new SvgDocument();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgSpriteBuilder" /> class from an SVG document.
        /// </summary>
        /// <param name="svgDocument">The input SVG document.</param>
        public SvgSpriteBuilder(SvgDocument svgDocument)
        {
            currentSvgDocument = svgDocument;

            //Add existing IDs to the dictionary
            foreach (var symbolId in GetSymbolIds())
            {
                AddIdWithIndex(symbolId);
            }
        }

        /// <summary>
        /// Adds an SVG document as a symbol to current SVG document.
        /// </summary>
        /// <param name="svgDocument">The input SVG document to add as a symbol.</param>
        /// <param name="symbolId">
        /// The symbol id. If not specified, existing <c>id</c> attribute will be used. If no existing <c>id</c> attribute, <see cref="SvgSymbolOptions.IdForMissing"/> will be used.
        /// <para>The resulting id will be prefixed with <see cref="SvgSymbolOptions.IdPrefix"/> if it was set.</para>
        /// <para>The resulting id will be converted to <c>lowercase</c> if <see cref="SvgSymbolOptions.IdLowerCased"/> was set.</para>
        /// <para>When there are duplicates, the id will be incremented like <c>"symbol-2"</c>, <c>"symbol-3"</c> etc.</para>
        /// <para><inheritdoc cref="SvgSymbolOptions.IdReplacementChar" path="/summary/para[@id='info']" /></para>
        /// </param>
        /// <param name="svgSymbolOptions">The options to use when converting an <c>&lt;svg&gt;</c> tag to an <c>&lt;symbol&gt;</c> tag or vice versa.</param>
        /// <returns>A <see cref="XElement" /> for the added <c>&lt;symbol&gt;</c> tag.</returns>
        public XElement AddSymbol(SvgDocument svgDocument, string symbolId = null, SvgSymbolOptions svgSymbolOptions = null)
        {
            if (svgDocument == null)
                return null;

            var inputElement = svgDocument.Root;

            // Create <symbol> element
            var outputElement = new XElement(SvgDocument.Namespace + "symbol");

            CopySvgElement(inputElement, outputElement, symbolId, svgSymbolOptions);

            //Add determined symbol id to dictionary and increment it if necessary
            var outputId = outputElement.Attribute("id")?.Value;
            if (!string.IsNullOrWhiteSpace(outputId))
                outputElement.SetAttributeValue("id", AddIdWithIndex(outputId));

            currentSvgDocument.Root.Add(outputElement);

            return outputElement;
        }

        /// <summary>
        /// Extracts a symbol by id from current SVG document, to a new SVG document.
        /// </summary>
        /// <param name="symbolId">The symbol id.</param>
        /// <param name="svgSymbolOptions">The options to use when converting an <c>&lt;svg&gt;</c> tag to an <c>&lt;symbol&gt;</c> tag or vice versa.</param>
        /// <returns>A <see cref="SvgDocument" /> for the added <c>&lt;symbol&gt;</c> tag.</returns>
        public SvgDocument ExtractSymbol(string symbolId, SvgSymbolOptions svgSymbolOptions = null)
        {
            var inputElement = GetSymbol(symbolId);
            if (inputElement == null)
                return null;

            // Create <svg> element
            var svgDocument = new SvgDocument();
            var outputElement = svgDocument.Root;

            CopySvgElement(inputElement, outputElement, symbolId, svgSymbolOptions);

            return svgDocument;
        }

        /// <summary>
        /// Extracts a symbol from current SVG document, to a new SVG document.
        /// </summary>
        /// <param name="symbol">The symbol <see cref="XElement" />.</param>
        /// <param name="svgSymbolOptions">The options to use when converting an <c>&lt;svg&gt;</c> tag to an <c>&lt;symbol&gt;</c> tag or vice versa.</param>
        /// <returns>A <see cref="SvgDocument" /> for the added <c>&lt;symbol&gt;</c> tag.</returns>
        public SvgDocument ExtractSymbol(XElement symbol, SvgSymbolOptions svgSymbolOptions = null)
        {
            if (symbol == null)
                return null;

            var inputElement = symbol;

            // Create <svg> element
            var svgDocument = new SvgDocument();
            var outputElement = svgDocument.Root;

            CopySvgElement(inputElement, outputElement, null, svgSymbolOptions);

            return svgDocument;
        }

        /// <summary>
        /// Gets a symbol by id from current SVG document.
        /// </summary>
        /// <param name="symbolId">The symbol id.</param>
        /// <returns>A <see cref="XElement" /> for the <c>&lt;symbol&gt;</c> tag, <see langword="null"/> if not found.</returns>
        public XElement GetSymbol(string symbolId)
        {
            if (string.IsNullOrWhiteSpace(symbolId))
                return null;

            return currentSvgDocument.Root.Elements(SvgDocument.Namespace + "symbol")
                .FirstOrDefault(s => s.Attribute("id")?.Value == symbolId); //IDs are case-sensitive
        }

        /// <summary>
        /// Gets all symbol ids from current SVG document.
        /// </summary>
        /// <returns>An array of <see cref="string" /> for the <c>&lt;symbol&gt;</c> ids.</returns>
        public IEnumerable<string> GetSymbolIds()
        {
            return currentSvgDocument.Root.Elements(SvgDocument.Namespace + "symbol")
                .Select(s => s.Attribute("id")?.Value)
                .Where(id => !string.IsNullOrWhiteSpace(id));
        }

        /// <summary>
        /// Gets all symbols from current SVG document.
        /// </summary>
        /// <returns>An array of <see cref="XElement" /> for the <c>&lt;symbol&gt;</c> tags.</returns>
        public XElement[] GetSymbols()
        {
            //return an array so that deleting elements does not affect enumeration.
            return currentSvgDocument.Root.Elements(SvgDocument.Namespace + "symbol").ToArray();
        }

        /// <summary>
        /// Removes a symbol by id from current SVG document.
        /// </summary>
        /// <param name="symbolId">The symbol id.</param>
        /// <returns>A <see cref="XElement" /> for the removed <c>&lt;symbol&gt;</c> tag, <see langword="null"/> if not found.</returns>
        public XElement RemoveSymbol(string symbolId)
        {
            var symbol = GetSymbol(symbolId);

            if (symbol != null)
            {
                symbol.Remove();
                RemoveIdWithIndex(symbolId);
            }

            return symbol;
        }

        /// <summary>
        /// Creates an HTML page for previewing the symbols inside the SVG sprite.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the HTML preview page.</returns>
        public string CreatePreviewPage()
        {
            var type = GetType();
            var assembly = type.Assembly;

            var html = string.Empty;
            using (var resourceStream = assembly.GetManifestResourceStream(type.Namespace + ".Resources.PreviewPageTemplate.html"))
                if (resourceStream != null)
                    using (var reader = new StreamReader(resourceStream))
                        html = reader.ReadToEnd();

            var sb = new StringBuilder();
            var replacements = new Dictionary<string, string>();

            //<use> tag does not work cross-origin, including local html files (when not viewed from a web server),
            //so we need to put the SVG sprite into HTML for the preview page:
            using (var stringWriter = new StringWriter(sb))
            {
                currentSvgDocument.Save(stringWriter);
                sb.AppendLine();
            }
            replacements.Add("$SvgSprite", sb.ToString());
            sb.Clear();

            sb.AppendLine();
            var symbolCount = 0;
            foreach (var symbolId in GetSymbolIds())
            {
                sb.AppendLine($"  <div class=\"icon\">");
                sb.AppendLine($"    <svg>");
                sb.AppendLine($"      <use href=\"#{symbolId}\" />");
                sb.AppendLine($"    </svg>");
                sb.AppendLine($"    <div class=\"label\">{symbolId}</div>");
                sb.AppendLine($"  </div>");
                sb.AppendLine();
                symbolCount++;
            }
            replacements.Add("$SvgSymbolCount", symbolCount.ToString());
            replacements.Add("$SvgIcons", sb.ToString());
            sb.Clear();

            html = Regex.Replace(
                html,
                string.Join("|", replacements.Keys.Select(Regex.Escape)),
                match => replacements[match.Value],
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
            );

            return html;
        }

        private string AddIdWithIndex(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;

            var match = Regex.Match(id, @"^(.*)-(\d+)$");
            var idWithoutIndex = match.Success ? match.Groups[1].Value : id;
            var index = match.Success ? int.Parse(match.Groups[2].Value) : 1;

            if (idIndexes.TryGetValue(idWithoutIndex, out var indexes))
            {
                if (!indexes.Add(index))
                {
                    var highestIndex = indexes.Max();
                    highestIndex++;
                    indexes.Add(highestIndex);
                    return $"{idWithoutIndex}-{highestIndex}";
                }

                return id;
            }

            idIndexes.Add(idWithoutIndex, new HashSet<int> { index });
            return id;
        }

        private void RemoveIdWithIndex(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var match = Regex.Match(id, @"^(.*)-(\d+)$");
            var idWithoutIndex = match.Success ? match.Groups[1].Value : id;
            var index = match.Success ? int.Parse(match.Groups[2].Value) : 1;

            if (idIndexes.TryGetValue(idWithoutIndex, out var indexes))
            {
                indexes.Remove(index);

                if (indexes.Count == 0)
                    idIndexes.Remove(idWithoutIndex);
            }
        }

        private static void CopySvgElement(XElement inputElement, XElement outputElement, string outputId, SvgSymbolOptions svgSymbolOptions)
        {
            svgSymbolOptions ??= SvgSymbolOptions.Default;

            foreach (var attribute in inputElement.Attributes())
            {
                if (attribute.Name.LocalName.Equals("xmlns", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!svgSymbolOptions.ShouldPreserveAttribute(attribute.Name.LocalName))
                    continue;

                if (string.IsNullOrWhiteSpace(attribute.Value))
                    continue;

                outputElement.SetAttributeValue(attribute.Name.LocalName, attribute.Value);
            }

            if (string.IsNullOrWhiteSpace(outputId))
                outputId = outputElement.Attribute("id")?.Value; //this is copied to outputElement above (only if "id" attribute is not discarded)
            if (string.IsNullOrWhiteSpace(outputId))
                outputId = svgSymbolOptions.IdForMissing;
            if (!string.IsNullOrWhiteSpace(outputId) && !string.IsNullOrWhiteSpace(svgSymbolOptions.IdPrefix))
                outputId = svgSymbolOptions.IdPrefix + outputId;
            if (!string.IsNullOrWhiteSpace(outputId))
            {
                outputId = SanitizeSvgId(outputId, svgSymbolOptions.IdReplacementChar);
                if (svgSymbolOptions.IdLowerCased)
                    outputId = outputId.ToLowerInvariant();
                outputElement.SetAttributeValue("id", outputId);
            }

            if (!string.IsNullOrWhiteSpace(svgSymbolOptions.ViewBoxOverride))
                outputElement.SetAttributeValue("viewBox", svgSymbolOptions.ViewBoxOverride);

            // Move all children from <svg> to <symbol>
            foreach (var node in inputElement.Nodes())
            {
                if (node is XElement element)
                {
                    if (!svgSymbolOptions.ShouldPreserveElement(element.Name.LocalName))
                        continue;

                    if (!element.HasAttributes && !element.HasElements && string.IsNullOrWhiteSpace(element.Value))
                        continue;
                }

                if (node.NodeType == XmlNodeType.Comment && !svgSymbolOptions.CommentsPreserved)
                    continue;

                //If parent element has xml:space="preserve" attribute then new lines will come here so ignore them so that we can completely minify file when writing
                //So when "word wrap" is turned off in text editor, you should see everything in one line
                //Example file for testing this behaviour arrows-up-down-to-line.svg which has
                //<!-- Generator: Adobe Illustrator 28.1.0, SVG Export Plug-In . SVG Version: 6.00 Build 0)  -->
                if (node is XText text && text.Value == "\n") //seems no need to check "\r\n", new lines always come as "\n" when reading
                    continue;

                outputElement.Add(node);
            }
        }

        private static string SanitizeSvgId(string rawId, char replacementChar)
        {
            /*
            SVG uses XML rules for id attributes, so the allowed characters are:
               1. Start Character
                   - Must begin with a letter (A–Z, a–z) or underscore (_)
                   - Cannot start with a number or hyphen (-)
               2. Subsequent Characters
                   - Letters (A–Z, a–z)
                   - Digits (0–9)
                   - Underscore (_)
                   - Hyphen (-)
                   - Period (.)
                   - Colon (:) — technically allowed, but discouraged due to namespace conflicts
               
               🚫 Disallowed Characters
                   - Spaces ( )
                   - Special characters like @, #, $, %, &, *, +, =, /, \, ?, !, ,, ', ", <, >, (, )
                   - Emojis or non-ASCII symbols
               
           💡 Best Practices
               - Stick to lowercase letters, numbers, hyphens, and underscores
               - Use kebab-case (icon-star) or snake_case (icon_star)
               - Avoid colons and periods unless necessary
           
           
            Invalid SVG IDs:

                123icon          // Starts with a digit
                -icon            // Starts with a hyphen
                icon star        // Contains a space
                icon@star        // Contains @ symbol
                icon#star        // Contains #
                icon$star        // Contains $
                icon%star        // Contains %
                icon&star        // Contains &
                icon*star        // Contains *
                icon+star        // Contains +
                icon=star        // Contains =
                icon/star        // Contains /
                icon\star        // Contains backslash
                icon?star        // Contains ?
                icon!star        // Contains !
                icon,star        // Contains comma
                icon'star        // Contains apostrophe
                icon"star        // Contains quotation mark
                icon<star        // Contains <
                icon>star        // Contains >
                icon(star)       // Contains parentheses
                icon🙂star       // Contains emoji
            */
            if (string.IsNullOrWhiteSpace(rawId))
                return string.Empty;

            var allowedReplacementChars = new[] { '-', '_', '.' };
            if (Array.IndexOf(allowedReplacementChars, replacementChar) == -1)
                return rawId; // Setting any other value will disable sanitizing

            // Replace invalid characters with hyphen
            var sanitized = Regex.Replace(rawId, @"[^a-zA-Z0-9_\-\.]+", replacementChar.ToString());

            // Remove leading and trailing sanitized characters
            sanitized = sanitized.Trim(replacementChar);

            // Ensure it starts with a letter or underscore
            if (!Regex.IsMatch(sanitized, @"^[a-zA-Z_]"))
                sanitized = "_" + sanitized;

            return sanitized;
        }
    }
}
