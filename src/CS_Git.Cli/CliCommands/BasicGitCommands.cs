using System.Diagnostics;
using Cocona;
using CS_Git.Cli.CliCommands.CoconaLogic;
using CS_Git.Cli.Hashing;
using CS_Git.Cli.RepositoryLogic;

namespace CS_Git.Cli.CliCommands;

// ReSharper disable once ClassNeverInstantiated.Global

public class BasicGitCommands
{
    [Command("add", Description = "Add file contents to the index")]
    public async Task Add(DirectoryRequiredArgument pathToAdd)
    {
        var found = Repository.Find(pathToAdd.AbsolutePath);
        var repo = await Repository.New(found);
        
        throw new UnreachableException("Unimplemented");
    }
    
    [Command("init", Description = "Create an empty Git repository or reinitialize an existing one")]
    // public async Task Init([Argument(Description = "If you provide a directory, the command is run inside it. If this directory does not exist, it will be created")] string? directory)
    public async Task Init(DirectoryRequiredArgument directory)
    {
        _ = await Repository.Init(directory.AbsolutePath);
        Console.WriteLine($"Finished initializing git repository at '{directory.AbsolutePath}'.");
    }

    [Command("hash-object", Description = "Convert existing file into a git object.")]
    public async Task HashObject()
    {
        var directory = Directory.GetCurrentDirectory();
        var AbsolutePath = Path.GetFullPath(directory);
        var repo = await Repository.New(AbsolutePath);
        var obj = new BlobGitObj("Helloworld\nwhatever;");
        obj.Write(repo);
    }

    [Command("cat-file", Description = "Write existing git object to the std-output.")]
    public async Task CatFile()
    {
        var directory = Directory.GetCurrentDirectory();
        var AbsolutePath = Path.GetFullPath(directory);
        var repo = await Repository.New(AbsolutePath);
        var sha1 = new GitSha1("0081dde56fdf7538f428fb1a2bcf8fd0df05ee7e");
        var sha2 = new GitSha1("0ac9faebed1ad25d38d0672c8fecd0bba4dcfa88");
        await GitObj.Read(repo, sha2);
    }
}