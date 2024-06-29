using System.Diagnostics;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Text;
using CS_Git.Cli.RepositoryLogic;

namespace CS_Git.Cli.Hashing;
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
public class GitObjectParser
{

    public static void Serialize()
    {
        throw new UnreachableException("unimplemented");
    }

    public static void Deserialize()
    {
        throw new UnreachableException("unimplemented");
    }
}

public struct GitSha1
{
    public readonly byte[] ShaHash;
    public string FolderName;
    public string FileName;
    
    private const int ExpectedLength = 40;
    
    public GitSha1(byte[] shaHash)
    {
        ShaHash = shaHash;
        var str = Convert.ToHexString(shaHash);
        if (str.Length != ExpectedLength)
            throw new ArgumentOutOfRangeException(
                $"{nameof(shaHash)}", $"Expected length of {ExpectedLength}, got {str.Length}. bytes:[{shaHash}] -> string:[{str}]");

        FolderName = str[..2];
        FileName = str[2..];
    }
    
    public GitSha1(string shaHash)
    {
        ShaHash = Encoding.UTF8.GetBytes(shaHash);
        if (shaHash.Length != ExpectedLength)
            throw new ArgumentOutOfRangeException(
                $"{nameof(shaHash)}", $"Expected length of {ExpectedLength}, got {shaHash.Length}. bytes:[{shaHash}] -> string:[{shaHash}]");

        FolderName = shaHash[..2];
        FileName = shaHash[2..];
    }
}

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
        
        if (content.Length != expectedLen)
            throw new InvalidDataException($"Malformed object. Bad length. Expected {expectedLen}, got {content.Length}.");

        return expectedType switch
        {
            BlobGitObj.TYPENAME => new BlobGitObj(content),
            CommitGitObj.TYPENAME => new CommitGitObj(content),
            TagGitObj.TYPENAME => new TagGitObj(content),
            TreeGitObj.TYPENAME => new TreeGitObj(content),
            _ => throw new UnreachableException($"Unknown GitObject type {expectedType}")
        };
    }

    public abstract GitSha1 Write(Repository repo);
}

public record BlobGitObj(string Content) : GitObj
{
    internal const string TYPENAME = "blob";
    public override GitSha1 Write(Repository repo)
    {
        var sha = new GitSha1();
        var result = $"{TYPENAME} {Encoding.UTF8.GetByteCount(Content)}\0{Content}";
        
        Console.WriteLine(sha);
        using var stream = repo.CreateRepoFileStreamWriter("objects", sha.FolderName, sha.FileName);
        using var writer = new StreamWriter(new ZLibStream(stream, CompressionMode.Compress), Encoding.UTF8);
        writer.WriteAsync(result);
        return sha;
    }
}

public record CommitGitObj(string Content) : GitObj
{
    internal const string TYPENAME = "commit";
    public override GitSha1 Write(Repository repo)
    {
        throw new NotImplementedException();
    }
}

public record TagGitObj(string Content) : GitObj
{
    internal const string TYPENAME = "tag";
    public override GitSha1 Write(Repository repo)
    {
        throw new NotImplementedException();
    }
}

public record TreeGitObj(string Content) : GitObj
{
    internal const string TYPENAME = "tree";
    public override GitSha1 Write(Repository repo)
    {
        throw new NotImplementedException();
    }
}
