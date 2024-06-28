using Cocona;
using CS_Git.Cli.Models;

namespace CS_Git.Cli.CliCommands;

public class BasicGitCommands
{
    [Command("add", Description = "Add file contents to the index")]
    public async Task Add(DirectoryRequiredArgument pathToAdd)
    {
        Console.WriteLine(pathToAdd.OriginalInput);
        Console.WriteLine(pathToAdd.AbsolutePath);
        var found = Repository.Find(pathToAdd.AbsolutePath);
        if (found is null)
            throw new Exception($"Unable to find '.git' above directory: {pathToAdd.AbsolutePath}");
        var repo = await Repository.New(found);
        Console.WriteLine(repo);
        
    }
    
    [Command("init", Description = "git-init - Create an empty Git repository or reinitialize an existing one")]
    // public async Task Init([Argument(Description = "If you provide a directory, the command is run inside it. If this directory does not exist, it will be created")] string? directory)
    public async Task Init(DirectoryRequiredArgument directory)
    {
        _ = await Repository.Init(directory.AbsolutePath);
        Console.WriteLine($"Finished initializing git repository at '{directory.AbsolutePath}'.");
    }
}

/// <summary>
/// Parameter used to be able to accept both (relativePath || absolutePath) -> absolute-path
/// Is optional, default to "."
/// </summary>
public class DirectoryRequiredArgument : ICommandParameterSet
{
    public string AbsolutePath { get; init; }
    public string? OriginalInput { get; init; }

    public DirectoryRequiredArgument([Argument(Description = "If you provide a directory, the command is run inside it. If this directory does not exist, it will be created")] string? directory = null)
    {
        OriginalInput = directory;
        directory ??= Directory.GetCurrentDirectory();
        AbsolutePath = Path.GetFullPath(directory);
    }
}