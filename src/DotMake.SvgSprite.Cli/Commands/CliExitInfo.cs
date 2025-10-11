using System;

namespace DotMake.SvgSprite.Cli.Commands;

internal class CliExitInfo
{
    public CliExitInfo(int code, string message = null)
    {
        Code = code;
        Message = message;
    }

    public int Code { get; }

    public string Message { get; }

    public int WriteAndReturn()
    {
        switch (Code)
        {
            case 0:
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Message ?? "Command completed successfully.");
                Console.ResetColor();
                break;
            default:
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Message != null ? $"Command failed: {Message}" : "Command failed!");
                Console.ResetColor();
                break;
        }

        return Code;
    }

    public void WriteAndExit()
    {
        Environment.Exit(WriteAndReturn());
    }
}
