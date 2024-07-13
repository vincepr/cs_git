using System.IO.Compression;
using System.Text;
using CS_Git.Lib.RepositoryLogic;
using CS_Git.Lib.Util;

namespace CS_Git.Lib.GitObjectLogic.ObjTypes;

public record BlobBaseGitObj : BaseGitObj
{
    internal const string TypeName = "blob";
    
    public byte[] Content { get; init; }
    
    /// <inheritdoc />
    public BlobBaseGitObj(byte[] Content)
    {
        this.Content = Content;
    }

    /// <inheritdoc />
    public override async Task<GitSha1> Write(Repository repo)
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

    
    /// <inheritdoc />
    public override string ToString() => Encoding.UTF8.GetString(Content);
    
    /// <inheritdoc />
    public override GitSha1 Hash()
    {
        var header = Encoding.ASCII.GetBytes($"{TypeName} {Content.Length}\0");
        var combined = header.Concat(Content).ToArray();
        return new GitSha1(combined);
    }
}