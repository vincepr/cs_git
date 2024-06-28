using Cocona;
using CS_Git.Cli.CliCommands;

namespace CS_Git.Cli;

class Program
{
    static void Main(string[] args)
    {
        var app = CoconaLiteApp.Create();
        app.AddCommands<BasicGitCommands>();
        app.Run();
    }
}