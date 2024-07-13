using System.Diagnostics;
using Cocona;
using CS_Git.Cli.CliCommands.CoconaLogic;
using CS_Git.Lib.GitObjectLogic;
using CS_Git.Lib.GitObjectLogic.ObjTypes;
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
        Console.WriteLine(repo);

        throw new UnreachableException("Unimplemented");
    }

    [Command("init", Description = "Create an empty Git repository or reinitialize an existing one")]
    public async Task Init(DirectoryRequiredArgument directory)
    {
        _ = await Repository.Init(directory.AbsolutePath);
        Console.WriteLine($"Finished initializing git repository at '{directory.AbsolutePath}'.");
    }

    [Command("hash-object", Description = "Convert existing file into a git object")]
    public async Task HashObject(
        FileRequiredArgument path,
        [Option('t', Description = "Specify the type")] EnumGitObj type = EnumGitObj.blob,
        [Option('w', Description = "Actually write the object into the database")]bool write = false)
    {
        if (write == false)
        {
            Console.Write(await GitObjUtils.HashObject(path.AbsolutePath));
        }
        else
        {
            var repo = await Repository.FindRecursiveAndRead();
            var sha = await GitObjUtils.HashObject(repo, path.AbsolutePath);
            Console.WriteLine($"created file: '{sha.FolderName}/{sha.FileName}'");
        }
    }

    [Command("cat-file", Description = "Write existing git object to the std-output.")]
    public async Task CatFile(
        [Argument(Description = "Specify the type")] EnumGitObj type,
        [Argument(Description = "The object to display. Ex the full sha")] string name)
    {
        var repo = await Repository.FindRecursiveAndRead();
        var obj = await repo.ObjectFind(name);
        switch (obj, type)
        {
            case (BlobBaseGitObj, EnumGitObj.blob):
                break;
            case (CommitBaseGitObj, EnumGitObj.commit):
                break;
            case (TagBaseGitObj, EnumGitObj.tag):
                break;
            case (TreeBaseGitObj, EnumGitObj.tree):
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(obj),
                    $"argument type:{type} not matching found git-object-type: {nameof(obj)}");
        }

        Console.WriteLine(obj.ToString());
    }
}