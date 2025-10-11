using DotMake.CommandLine;

namespace DotMake.SvgSprite.Cli.Commands
{
    [CliCommand(
        Children = new []
        {
            typeof(BuildCliCommand),
            typeof(ExtractCliCommand)
        }
    )]
    internal class RootCliCommand
    {
    }
}
