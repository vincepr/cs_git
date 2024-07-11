using CS_Git.Lib.GitObjectLogic;
using CS_Git.Lib.GitObjectLogic.ObjTypes;
using CS_Git.Lib.RepositoryLogic;

namespace CS_Git.Cli.CliCommands;

public static class GitObjUtils
{
        
    public static async Task<GitSha1> HashObject(string absolutePath)
    {
        var obj = new BlobBaseGitObj(await File.ReadAllBytesAsync(absolutePath));
        return obj.Hash();
    }
        
    public static async Task<GitSha1> HashObject(Repository repo, string absolutePath)
    {
        var obj = new BlobBaseGitObj(await File.ReadAllBytesAsync(absolutePath));
        return await obj.Write(repo);
    }
}