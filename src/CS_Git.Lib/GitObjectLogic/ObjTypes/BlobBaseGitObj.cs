using System.IO.Compression;
using System.Text;
using CS_Git.Lib.RepositoryLogic;
using CS_Git.Lib.Util;

namespace CS_Git.Lib.GitObjectLogic.ObjTypes;

public record BlobBaseGitObj : BaseGitObj
{
    internal const string TypeName = "blob";
    
    public BlobBaseGitObj(byte[] Content) : base(Content)
    {
    }

    public override async Task<GitSha1> Serialize(Repository repo)
    {
        var header = $"{TypeName} {Content.Length}\0";
        var sha = Hash();

        await using var stream = repo.CreateRepoFileStreamWriter("objects", sha.FolderName, sha.FileName);
        await using var writer = new ZLibStream(stream, CompressionMode.Compress);
        
        var headerStream = StreamHelpers.GenerateStreamFromString(header);
        StreamHelpers.CopyStream(headerStream, writer);
        var bodyStream = new MemoryStream(Content);
        StreamHelpers.CopyStream(bodyStream, writer);
        
        return sha;
    }
    
    public override string ToString() => Encoding.UTF8.GetString(Content);

    public override GitSha1 Hash() => HashBaseImplementation(TypeName, Content);
}