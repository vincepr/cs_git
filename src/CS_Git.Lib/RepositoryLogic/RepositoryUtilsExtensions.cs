using CS_Git.Lib.GitObjectLogic;

namespace CS_Git.Lib.RepositoryLogic;

public static class RepositoryUtilsExtensions
{
    /// <summary>
    /// Resolve a sha or similar pattern to find the git object it resolves to.
    /// </summary>
    /// <param name="repo">the active repository.</param>
    /// <param name="objectName">sha or similar string pattern to identify the object.</param>
    /// <returns></returns>
    public static async Task<BaseGitObj> ObjectFind(this Repository repo, string objectName) 
        => await BaseGitObj.Read(repo, GitSha1.FromHexString(objectName));
}