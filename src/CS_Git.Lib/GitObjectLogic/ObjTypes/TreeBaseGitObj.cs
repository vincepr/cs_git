using System.Text;
using CS_Git.Lib.RepositoryLogic;

namespace CS_Git.Lib.GitObjectLogic.ObjTypes;

public record TreeBaseGitObj(byte[] Content) : BaseGitObj
{
    internal const string TypeName = "tree";
    
    public override Task<GitSha1> Write(Repository repo)
    {
        throw new NotImplementedException();
    }

    public override GitSha1 Hash()
    {
        throw new NotImplementedException();
    }

    public override string ToString() => Encoding.UTF8.GetString(Content);
}