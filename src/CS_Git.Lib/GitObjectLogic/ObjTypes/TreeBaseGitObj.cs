using System.Text;
using CS_Git.Lib.RepositoryLogic;

namespace CS_Git.Lib.GitObjectLogic.ObjTypes;

public record TreeBaseGitObj : BaseGitObj
{
    public TreeBaseGitObj(byte[] Content) : base(Content)
    {
    }

    internal const string TypeName = "tree";
    
    public override Task<GitSha1> Serialize(Repository repo)
    {
        throw new NotImplementedException();
    }

    public override GitSha1 Hash() => HashBaseImplementation(TypeName, Content);

    public override string ToString() => Encoding.UTF8.GetString(Content);
}