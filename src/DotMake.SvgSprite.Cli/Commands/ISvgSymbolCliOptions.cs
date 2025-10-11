using DotMake.CommandLine;

namespace DotMake.SvgSprite.Cli.Commands
{
    internal interface ISvgSymbolCliOptions
    {
        [CliOption(
            Description = "The attributes to discard when converting <svg> tag to <symbol> tag or vice versa." +
                          "\nWildcards can be used: * - Zero or more characters, ? - Exactly one character." +
                          "\nAttributes that should be usually preserved on <symbol> tag:" +
                          "\n- viewBox — must be preserved on <symbol> to define coordinate system" +
                          "\n- id — required for referencing the symbol later" +
                          "\n- fill, stroke, opacity, transform — if used for styling" +
                          "\n- class, style — if CSS is applied" +
                          "\n- width and height — optional; usually omitted in sprites",
            AllowMultipleArgumentsPerToken = true
        )]
        public string[] AttributesToDiscard { get; set; }

        [CliOption(
            Description = "The elements to discard when converting <svg> tag to <symbol> tag or vice versa." +
                          "\nWildcards can be used: * - Zero or more characters, ? - Exactly one character." +
                          "\nElements that should be usually preserved inside <symbol> tag:" +
                          "\n- <path> — most common for icons" +
                          "\n- <circle>, <rect>, <line>, <polygon>, <polyline> — basic shapes" +
                          "\n- <g> — groups of elements (preserve if used for structure or styling)" +
                          "\n- <text> — if your icon includes text" +
                          "\n- <use> — if referencing other symbols" +
                          "\n- <defs> — only if it contains reusable elements like gradients or filters used inside the symbol",
            AllowMultipleArgumentsPerToken = true
        )]
        public string[] ElementsToDiscard { get; set; }

        [CliOption(
            Description = "The prefix to add to the id attribute when converting <svg> tag to <symbol> tag or vice versa."
        )]
        public string IdPrefix { get; set; }

        [CliOption(
            Description = "The value to use when id attribute is missing or is not specified when converting <svg> tag to <symbol> tag or vice versa." +
                          "\nWhen there are duplicates, this value will be incremented like symbol-2, symbol-3 etc."
        )]
        public string IdForMissing { get; set; }

        [CliOption(
            Description = "The replacement character when sanitizing the id attribute when converting <svg> tag to <symbol> tag or vice versa." +
                          "\nAllowed values are '-', '_', '.'. Setting any other value will disable sanitizing"
        )]
        public char IdReplacementChar { get; set; }

        [CliOption(
            Description = "The value indicating whether to convert the id attribute to lowercase when converting <svg> tag to <symbol> tag or vice versa."
        )]
        public bool IdLowerCased { get; set; }

        [CliOption(
            Description = "The override value to use for all viewBox attributes, e.g. \"0 0 24 24\" when converting <svg> tag to <symbol> tag or vice versa."
        )]
        public string ViewBoxOverride { get; set; }

        [CliOption(
            Description = "The value indicating whether to preserve comments when converting <svg> tag to <symbol> tag or vice versa."
        )]
        public bool CommentsPreserved { get; set; }
    }
}
