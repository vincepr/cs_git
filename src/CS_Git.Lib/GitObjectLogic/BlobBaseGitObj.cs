using System.IO.Compression;
using System.Text;
using CS_Git.Lib.RepositoryLogic;
using CS_Git.Lib.Util;

namespace CS_Git.Lib.GitObjectLogic;

public record BlobBaseGitObj(byte[] Content) : BaseGitObj
{
    internal const string TypeName = "blob";
    public override async Task<GitSha1> Write(Repository repo)
    {
        var header = $"{TypeName} {Content.Length}\0";
        var body = Encoding.UTF8.GetString(Content);
        var combined = header + body;
        var sha = new GitSha1(combined);

        await using var stream = repo.CreateRepoFileStreamWriter("objects", sha.FolderName, sha.FileName);
        await using var writer = new ZLibStream(stream, CompressionMode.Compress);
        
        var headerStream = StreamHelpers.GenerateStreamFromString(header);
        StreamHelpers.CopyStream(headerStream, writer);
        var bodyStream = new MemoryStream(Content);
        StreamHelpers.CopyStream(bodyStream, writer);
        
        return sha;
    }
    public override string ToString() => Encoding.UTF8.GetString(Content);
}