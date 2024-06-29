using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using CS_Git.Lib.RepositoryLogic;

namespace CS_Git.Lib.Object;
// place for object files is decided by it's SHA-1 hash:
//      ./.git/objects/first 2 chars as folder name / rest of the chars as filename
//      ./.git/objects/e6/73d1b7eaa0aa01b5bc2442d570a765bdaae751

// -- object files structure in git --
//
// objects starts with head that specifies it's type:
//      blob || commit || tag || tree
// followed by ASCII space 0x20
// then the size of the object in bytes as an ASCII number
// then 0x00 (null byte because of c)
// then the contents of the object
// 
// example first 48 bytes of an object:
//      nr        actual hexa-values                                | translation    |
//      ------------------------------------------------------------|----------------|
//      00000000  63 6f 6d 6d 69 74 20 31  30 38 36 00 74 72 65 65  |commit 1086.tree|
//      00000010  20 32 39 66 66 31 36 63  39 63 31 34 65 32 36 35  | 29ff16c9c14e265|
//      00000020  32 62 32 32 66 38 62 37  38 62 62 30 38 61 35 61  |2b22f8b78bb08a5a|

public abstract record GitObj
{
    public static async Task<GitObj> Read(Repository repo, GitSha1 sha)
    {
        await using var stream = repo.CreateRepoFileStreamReadonly("objects", sha.FolderName, sha.FileName);
        using var reader = new StreamReader(new ZLibStream(stream, CompressionMode.Decompress), Encoding.UTF8);

        char ch;
        string expectedType = String.Empty;
        while (reader.Peek() != -1)
        {
            ch = (char)reader.Read();
            if (ch == ' ') break;
            expectedType += ch;
        }
        
        string expectedLenAscii = String.Empty;
        while (reader.Peek() != -1)
        {
            ch = (char)reader.Read();
            if (ch == '\0') break;
            expectedLenAscii += ch;
        }

        var expectedLen = uint.Parse(expectedLenAscii);
        
        if (reader.Peek() == '\uFEFF')
        {
#if DEBUG
            await Console.Error.WriteLineAsync("DEBUG-FLAG Notice: byteOrderMark encountered. Might fuck shit up. But probably handled.");
#endif
            // consume the byte-order-mark. Rider adds those in way to often...
            // We just add some magic length here, probably some \0\0 of utf32-bom that get eaten by the stream magically or smth.
            // TODO: check again without Ecoding != ASCII on FileStream(). Should we even get utf8 or utf16 to work?
            expectedLen -= 2;
        }

        var content = await reader.ReadToEndAsync();
        
#if !DEBUG 
        if (content.Length != expectedLen)
            throw new InvalidDataException($"Malformed object. Bad length. Expected {expectedLen}, got {content.Length}.");
#endif
#if DEBUG
        if (content.Length != expectedLen) await Console.Error.WriteLineAsync($"Malformed object. Bad length. Expected {expectedLen}, got {content.Length}.");
#endif

        return expectedType switch
        {
            BlobGitObj.TypeName => new BlobGitObj(content),
            CommitGitObj.TypeName => new CommitGitObj(content),
            TagGitObj.TypeName => new TagGitObj(content),
            TreeGitObj.TypeName => new TreeGitObj(content),
            _ => throw new UnreachableException($"Unknown GitObject type {expectedType}")
        };
    }

    public abstract Task<GitSha1> Write(Repository repo);
}

public record BlobGitObj(string Content) : GitObj
{
    internal const string TypeName = "blob";
    public override async Task<GitSha1> Write(Repository repo)
    {
        var result = $"{TypeName} {Encoding.UTF8.GetByteCount(Content)}\0{Content}";
        var sha = new GitSha1(result);

        await using var stream = repo.CreateRepoFileStreamWriter("objects", sha.FolderName, sha.FileName);
        await using var writer = new StreamWriter(new ZLibStream(stream, CompressionMode.Compress), Encoding.UTF8);
        await writer.WriteAsync(result);
        return sha;
    }
    public override string ToString() => Content;
}

public record CommitGitObj(string Content) : GitObj
{
    internal const string TypeName = "commit";
    public override Task<GitSha1> Write(Repository repo)
    {
        throw new NotImplementedException();
    }
    public override string ToString() => Content;
}

public record TagGitObj(string Content) : GitObj
{
    internal const string TypeName = "tag";
    public override Task<GitSha1> Write(Repository repo)
    {
        throw new NotImplementedException();
    }
    public override string ToString() => Content;
}

public record TreeGitObj(string Content) : GitObj
{
    internal const string TypeName = "tree";
    public override Task<GitSha1> Write(Repository repo)
    {
        throw new NotImplementedException();
    }
    public override string ToString() => Content;
}
