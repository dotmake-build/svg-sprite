using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DotMake.SvgSprite
{
    /// <summary>
    /// Represents an SVG document. Provides methods for loading, editing and saving SVG files.
    /// </summary>
    /// <example>
    ///     <code source="..\DotMake.SvgSprite\UsageExamples.cs" region="SvgLoading" language="cs" />
    ///     <code source="..\DotMake.SvgSprite\UsageExamples.cs" region="SvgSaving" language="cs" />
    /// </example>
    public class SvgDocument
    {
        private const LoadOptions SvgLoadOptions = LoadOptions.None;
        private readonly XDocument svgXDocument;

        private SvgDocument(XDocument svgXDocument)
        {
            this.svgXDocument = svgXDocument;

            EnsureSvgElement(svgXDocument);
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDocument" /> class.
        /// </summary>
        public SvgDocument()
        {
            svgXDocument = new XDocument(
                new XElement(Namespace + "svg",
                    new XAttribute("xmlns", Namespace)
                )
            );
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDocument" /> class from a file.
        /// </summary>
        /// <param name="svgFile">The input file for the SVG.</param>
        public SvgDocument(string svgFile)
            : this(XDocument.Load(svgFile, SvgLoadOptions))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDocument" /> class from a stream.
        /// </summary>
        /// <param name="svgStream">The input stream for the SVG.</param>
        public SvgDocument(Stream svgStream)
            : this(XDocument.Load(svgStream, SvgLoadOptions))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDocument" /> class from a text reader.
        /// </summary>
        /// <param name="svgTextReader">The input text reader for the SVG.</param>
        public SvgDocument(TextReader svgTextReader)
            : this(XDocument.Load(svgTextReader, SvgLoadOptions))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgDocument" /> class from a xml reader.
        /// </summary>
        /// <param name="svgXmlReader">The input xml reader for the SVG.</param>
        public SvgDocument(XmlReader svgXmlReader)
            : this(XDocument.Load(svgXmlReader, SvgLoadOptions))
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SvgDocument" /> class from a string.
        /// </summary>
        /// <param name="svgString">The input string for the SVG.</param>
        /// <returns>A <see cref="SvgDocument" /> for the parsed SVG string.</returns>
        public static SvgDocument Parse(string svgString)
        {
            return new SvgDocument(XDocument.Parse(svgString, SvgLoadOptions));
        }



        /// <summary>
        /// Gets the root element of this SVG document, i.e. the <c>&lt;svg&gt;</c> tag.
        /// </summary>
        public XElement Root => svgXDocument.Root;


        /// <summary>
        /// Gets the namespace for SVG documents, i.e. <c>http://www.w3.org/2000/svg</c>.
        /// </summary>
        public static readonly XNamespace Namespace = "http://www.w3.org/2000/svg";



        /// <summary>
        /// Saves this SVG to a file.
        /// </summary>
        /// <param name="svgFile">The output file for the SVG.</param>
        /// <param name="disableFormatting"><inheritdoc cref="GetXmlWriterSettings" path="/param[@name='disableFormatting']" /></param>
        public void Save(string svgFile, bool disableFormatting = false)
        {
            using (var xmlWriter = XmlWriter.Create(svgFile, GetXmlWriterSettings(disableFormatting)))
                svgXDocument.Save(xmlWriter);
        }

        /// <summary>
        /// Saves this SVG to a stream.
        /// </summary>
        /// <param name="svgStream">The output stream for the SVG.</param>
        /// <param name="disableFormatting"><inheritdoc cref="GetXmlWriterSettings" path="/param[@name='disableFormatting']" /></param>
        public void Save(Stream svgStream, bool disableFormatting = false)
        {
            using (var xmlWriter = XmlWriter.Create(svgStream, GetXmlWriterSettings(disableFormatting)))
                svgXDocument.Save(xmlWriter);
        }

        /// <summary>
        /// Saves this SVG to a text writer.
        /// </summary>
        /// <param name="svgTextWriter">The output text writer for the SVG.</param>
        /// <param name="disableFormatting"><inheritdoc cref="GetXmlWriterSettings" path="/param[@name='disableFormatting']" /></param>
        public void Save(TextWriter svgTextWriter, bool disableFormatting = false)
        {
            using (var xmlWriter = XmlWriter.Create(svgTextWriter, GetXmlWriterSettings(disableFormatting)))
                svgXDocument.Save(xmlWriter);
        }

        /// <summary>
        /// Saves this SVG to a xml writer.
        /// </summary>
        /// <param name="svgXmlWriter">The output xml writer for the SVG.</param>
        /// <param name="disableFormatting"><inheritdoc cref="GetXmlWriterSettings" path="/param[@name='disableFormatting']" /></param>
        public void Save(XmlWriter svgXmlWriter, bool disableFormatting = false)
        {
            using (var xmlWriter = XmlWriter.Create(svgXmlWriter, GetXmlWriterSettings(disableFormatting)))
                svgXDocument.Save(xmlWriter);
        }

        /// <summary>Returns the indented XML for this SVG.</summary>
        /// <returns>A <see cref="string" /> containing the indented XML.</returns>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <inheritdoc cref="GetXmlWriterSettings" />
        /// <summary>Returns the XML for this SVG, optionally disabling formatting.</summary>
        /// <param name="disableFormatting"></param>
        /// <returns>A <see cref="string" /> containing the XML.</returns>
        public string ToString(bool disableFormatting)
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            using (var xmlWriter = XmlWriter.Create(stringWriter, GetXmlWriterSettings(disableFormatting)))
            {
                svgXDocument.Save(xmlWriter);

                return stringWriter.ToString();
            }
        }



        private static void EnsureSvgElement(XDocument svgXDocument)
        {
            if (svgXDocument.Root == null || !svgXDocument.Root.Name.LocalName.Equals("svg", StringComparison.OrdinalIgnoreCase))
                throw new Exception("This not a valid svg file, there is no root <svg> tag !");

            var attribute = svgXDocument.Root.Attribute("xmlns");
            if (attribute == null || attribute.Value != Namespace)
            {
                var oldNs = attribute?.Value ?? string.Empty;

                svgXDocument.Root.SetAttributeValue("xmlns", Namespace);

                //fix namespace of all elements (setting xmlns is not enough)
                foreach (var element in svgXDocument.Root.DescendantsAndSelf().Where(x => x.Name.Namespace == oldNs))
                    element.Name = Namespace + element.Name.LocalName;
            }
        }
        
        /// <param name="disableFormatting">Whether to disable formatting output, i.e. not write individual elements on new lines and indent.</param>
        private static XmlWriterSettings GetXmlWriterSettings(bool disableFormatting)
        {
            var xmlWriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true };

            if (!disableFormatting)
                xmlWriterSettings.Indent = true;

            return xmlWriterSettings;
        }
    }
}
