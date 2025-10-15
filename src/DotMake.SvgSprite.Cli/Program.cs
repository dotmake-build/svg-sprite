#if DEBUG

using DotMake.SvgSprite.Cli;

ProgramTest.Run(args);

#else

using DotMake.CommandLine;
using DotMake.SvgSprite.Cli.Commands;

Cli.Run<RootCliCommand>(args, new CliSettings {EnableDefaultExceptionHandler = true});

#endif
