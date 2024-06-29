﻿using System.Diagnostics;
using System.Text;
using Cocona;
using CS_Git.Cli.CliCommands.CoconaLogic;
using CS_Git.Lib.Object;
using CS_Git.Lib.RepositoryLogic;

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
    public async Task HashObject(FileRequiredArgument path)
    {
        var repo = await Repository.FindRecursiveAndRead();
        var content = await File.ReadAllTextAsync(path.AbsolutePath, Encoding.UTF8);
        var obj = new BlobGitObj(content);
        var sha = await obj.Write(repo);
        Console.WriteLine($"created: '{sha.FolderName}/{sha.FileName}'");
    }

    [Command("cat-file", Description = "Write existing git object to the std-output.")]
    public async Task CatFile([Argument]string sha)
    {
        var repo = await Repository.FindRecursiveAndRead();
        var obj = await GitObj.Read(repo, GitSha1.FromHexString(sha));
        Console.WriteLine(obj);
    }
}