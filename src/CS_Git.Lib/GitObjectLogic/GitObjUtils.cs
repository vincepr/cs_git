using CS_Git.Lib.GitObjectLogic.ObjTypes;
using CS_Git.Lib.RepositoryLogic;

namespace CS_Git.Lib.GitObjectLogic;

public static class GitObjUtils
{
    /// <summary>
    /// Gets the hash for a object. Without actually writing to the objects folder.
    /// </summary>
    /// <param name="absolutePath">absolute path of the file to be hashed.</param>
    /// <returns></returns>
    public static async Task<GitSha1> HashObject(string absolutePath)
    {
        var obj = new BlobBaseGitObj(await File.ReadAllBytesAsync(absolutePath));
        return obj.Hash();
    }
        
    /// <summary>
    /// Write object to .git/objects/sha02/sha. Hash is used for folder and filename.
    /// </summary>
    /// <param name="repo">used repository. Objects get written here.</param>
    /// <param name="absolutePath">absolute path of the file to be hashed.</param>
    /// <returns></returns>
    public static async Task<GitSha1> HashObject(Repository repo, string absolutePath)
    {
        var obj = new BlobBaseGitObj(await File.ReadAllBytesAsync(absolutePath));
        return await obj.Write(repo);
    }
}