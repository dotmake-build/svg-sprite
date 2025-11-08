using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotMake.SvgSprite;

/// <summary>
/// Options used when converting an <c>&lt;svg&gt;</c> tag to an <c>&lt;symbol&gt;</c> tag or vice versa.
/// </summary>
/// <example>
///     <code source="../DotMake.SvgSprite/UsageExamples.cs" region="SvgSpriteBuildingWithOptions" language="cs" />
///     <code source="../DotMake.SvgSprite/UsageExamples.cs" region="SvgSpriteExtractingWithOptions" language="cs" />
/// </example>
public class SvgSymbolOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SvgSymbolOptions" /> class.
    /// </summary>
    public SvgSymbolOptions()
    {
        AttributesToPreserve = new[] { "id", "viewBox", "class", "style", "fill", "stroke", "opacity", "transform" };
        ElementsToPreserve = new[] { "*" };
        IdForMissing = "symbol";
        IdReplacementChar = '-';
        IdLowerCased = true;
    }

    /// <summary>
    /// Gets or sets the attributes to preserve when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Wildcards can be used: <c>*</c> - Zero or more characters, <c>?</c> - Exactly one character.</para>
    /// <para>Default value is <c>{ "id", "viewBox", "class", "style", "fill", "stroke", "opacity", "transform" }</c>.</para>
    /// <para id="info">
    ///     Attributes that should be usually preserved on <c>&lt;symbol&gt;</c> tag:
    ///     <list type="bullet">
    ///         <item><c>viewBox</c> — must be preserved on <c>&lt;symbol&gt;</c> to define coordinate system</item>
    ///         <item><c>id</c> — required for referencing the symbol later</item>
    ///         <item><c>fill</c>, <c>stroke</c>, <c>opacity</c>, <c>transform</c> — if used for styling</item>
    ///         <item><c>class</c>, <c>style</c> — if CSS is applied</item>
    ///         <item><c>width</c> and <c>height</c> — optional; usually omitted in sprites</item>
    ///     </list>
    /// </para>
    /// </summary>
    public string[] AttributesToPreserve
    {
        get => attributesToPreserve;
        set
        {
            attributesToPreserve = value;
            attributesToPreserveRegex = ToRegex(value);
        }
    }
    private string[] attributesToPreserve;
    private Regex attributesToPreserveRegex;

    /// <summary>
    /// Gets or sets the attributes to discard when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Wildcards can be used: <c>*</c> - Zero or more characters, <c>?</c> - Exactly one character.</para>
    /// <para>Default value is <c>null</c> which means preserve all attributes set in <see cref="AttributesToPreserve"/>.</para>
    /// <inheritdoc cref="AttributesToPreserve" path="/summary/para[@id='info']" />
    /// </summary>
    public string[] AttributesToDiscard
    {
        get => attributesToDiscard;
        set
        {
            attributesToDiscard = value;
            attributesToDiscardRegex = ToRegex(value);
        }
    }
    private string[] attributesToDiscard;
    private Regex attributesToDiscardRegex;

    /// <summary>
    /// Gets or sets the attributes to preserve when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Wildcards can be used: <c>*</c> - Zero or more characters, <c>?</c> - Exactly one character.</para>
    /// <para>Default value is <c>{ "*" }</c>.</para>
    /// <para id="info">
    ///     Elements that should be usually preserved inside <c>&lt;symbol&gt;</c> tag:
    ///     <list type="bullet">
    ///         <item><c>&lt;path&gt;</c> — most common for icons</item>
    ///         <item><c>&lt;circle&gt;</c>, <c>&lt;rect&gt;</c>, <c>&lt;line&gt;</c>, <c>&lt;polygon&gt;</c>, <c>&lt;polyline&gt;</c> — basic shapes</item>
    ///         <item><c>&lt;g&gt;</c> — groups of elements (preserve if used for structure or styling)</item>
    ///         <item><c>&lt;text&gt;</c> — if your icon includes text</item>
    ///         <item><c>&lt;use&gt;</c> — if referencing other symbols</item>
    ///         <item><c>&lt;defs&gt;</c> — only if it contains reusable elements like gradients or filters used inside the symbol</item>
    ///     </list>
    /// </para>
    /// </summary>
    public string[] ElementsToPreserve
    {
        get => elementsToPreserve;
        set
        {
            elementsToPreserve = value;
            elementsToPreserveRegex = ToRegex(value);
        }
    }
    private string[] elementsToPreserve;
    private Regex elementsToPreserveRegex;

    /// <summary>
    /// Gets or sets the elements to discard when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Wildcards can be used: <c>*</c> - Zero or more characters, <c>?</c> - Exactly one character.</para>
    /// <para>Default value is <c>null</c> which means preserve all elements set in <see cref="ElementsToPreserve"/>.</para>
    /// <inheritdoc cref="ElementsToPreserve" path="/summary/para[@id='info']" />
    /// </summary>
    public string[] ElementsToDiscard
    {
        get => elementsToDiscard;
        set
        {
            elementsToDiscard = value;
            elementsToDiscardRegex = ToRegex(value);
        }
    }
    private string[] elementsToDiscard;
    private Regex elementsToDiscardRegex;

    /// <summary>
    /// Gets or sets the prefix to add to the <c>id</c> attribute when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Default value is <c>null</c> which means not to add a prefix.</para>
    /// </summary>
    public string IdPrefix { get; set; }

    /// <summary>
    /// Gets or sets the value to use when <c>id</c> attribute is missing or is not specified when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>When there are duplicates, this value will be incremented like <c>"symbol-2"</c>, <c>"symbol-3"</c> etc.</para>
    /// <para>Default value is <c>"symbol"</c>.</para>
    /// </summary>
    public string IdForMissing { get; set; }

    /// <summary>
    /// Gets or sets the replacement character when sanitizing the <c>id</c> attribute when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Default value is <c>'-'</c>. Allowed values are <c>'-'</c>, <c>'_'</c>, <c>'.'</c>. Setting any other value will disable sanitizing.</para>
    /// <para id="info">
    ///     The disallowed characters will be replaced with <see cref="IdReplacementChar"/> if it was set.
    ///     <br/>SVG uses XML rules for id attributes, so the allowed characters are:
    ///     <list type="number">
    /// 	    <item>
    /// 		    Start Character
    ///     		<list type="bullet">
    /// 	    		<item>Must begin with a letter (A–Z, a–z) or underscore (_)</item>
    /// 		    	<item>Cannot start with a number or hyphen (-)</item>
    /// 	    	</list>
    /// 	    </item>
    ///      	<item>
    /// 	    	Subsequent Characters
    /// 		    <list type="bullet">
    /// 			    <item>Letters (A–Z, a–z)</item>
    ///      			<item>Digits (0–9)</item>
    /// 	    		<item>Underscore (_)</item>
    /// 		    	<item>Hyphen (-)</item>
    /// 		    	<item>Period (.)</item>
    /// 	    	</list>
    /// 	    </item>
    ///     </list>
    /// </para>
    /// </summary>
    public char IdReplacementChar { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to convert the <c>id</c> attribute to <c>lowercase</c> when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Default value is <see langword="true"/>.</para>
    /// </summary>
    public bool IdLowerCased { get; set; }

    /// <summary>
    /// Gets or sets the override value to use for all <c>viewBox</c> attributes, e.g. <c>"0 0 24 24"</c>, when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Default value is <see langword="null"/>.</para>
    /// </summary>
    public string ViewBoxOverride { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to preserve comments when converting <c>&lt;svg&gt;</c> tag to <c>&lt;symbol&gt;</c> tag or vice versa.
    /// <para>Default value is <see langword="false"/>.</para>
    /// </summary>
    public bool CommentsPreserved { get; set; }

    /// <summary>
    /// Gets the default instance of the <see cref="SvgSymbolOptions" /> class.
    /// </summary>
    public static SvgSymbolOptions Default { get; } = new ();

    internal bool ShouldPreserveAttribute(string name)
    {
        if (attributesToDiscardRegex != null && attributesToDiscardRegex.IsMatch(name))
            return false;

        if (attributesToPreserveRegex != null)
            return attributesToPreserveRegex.IsMatch(name);

        return true;
    }

    internal bool ShouldPreserveElement(string name)
    {
        if (elementsToDiscardRegex != null && elementsToDiscardRegex.IsMatch(name))
            return false;

        if (elementsToPreserveRegex != null)
            return elementsToPreserveRegex.IsMatch(name);

        return true;
    }

    private static Regex ToRegex(string[] names)
    {
        if (names == null || names.Length == 0)
            return null;

        var patternList = new List<string>(names.Length);
        foreach (var value in names)
        {
            // (*) Zero or more characters, (?) Exactly one character. 
            var convertedPattern = Regex.Escape(value.Trim())
                .Replace(@"\?", ".")
                .Replace(@"\*", ".*");
            patternList.Add(convertedPattern);
        }

        //Element and attribute names are case-insensitive
        return new Regex(
            $"^({string.Join("|", patternList)})$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
        );
    }
}
