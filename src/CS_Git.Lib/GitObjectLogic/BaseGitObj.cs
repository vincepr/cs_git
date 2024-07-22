using System.Diagnostics;
using System.Text;
using CS_Git.Lib.GitObjectLogic.ObjTypes;
using CS_Git.Lib.RepositoryLogic;
using CS_Git.Lib.Util;

namespace CS_Git.Lib.GitObjectLogic;
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

public abstract record BaseGitObj(byte[] Content)
{
    /// <summary>
    /// Read specific object found in repo at path the sha is pointing to.
    /// </summary>
    /// <param name="repo">Used git repository.</param>
    /// <param name="sha">Used sha that identifies path to git-object-file.</param>
    /// <returns>The git-object.</returns>
    /// <exception cref="InvalidDataException">Encoded byte-length not as expected in header.</exception>
    /// <exception cref="UnreachableException">Unknown type encountered in header.</exception>
    public static async Task<BaseGitObj> Read(Repository repo, GitSha1 sha)
    {
        await using var stream = repo.CreateRepoFileStreamReadonly("objects", sha.FolderName, sha.FileName);
        var bytes = StreamHelpers.DecompressDataStreamToBytes(stream);

        var (start, end) = (0, 0);

        string expectedType = String.Empty;
        while (end < bytes.Length)
        {
            end++;
            expectedType = Encoding.UTF8.GetString(bytes[start..end]);
            if (expectedType.EndsWith(' '))
            {
                expectedType = expectedType.TrimEnd(' ');
                break;
            }
        }

        start = end;
        
        string expectedLenAscii = String.Empty;
        while (end < bytes.Length)
        {
            end++;
            if (expectedLenAscii.EndsWith('\0'))
            {
                expectedLenAscii = expectedLenAscii.TrimEnd('\0');
                break;
            }
            expectedLenAscii = Encoding.UTF8.GetString(bytes[start..end]);
        }

        var expectedLen = uint.Parse(expectedLenAscii);

        var content = bytes[(end-1)..];
        
        if (content.Length != expectedLen)
            throw new InvalidDataException(
                $"Malformed object. Bad content-length. Expected {expectedLen}, got {content.Length}.");

        return expectedType switch
        {
            BlobBaseGitObj.TypeName => new BlobBaseGitObj(content),
            CommitBaseGitObj.TypeName => await CommitBaseGitObj.Deserialize(content),
            TagBaseGitObj.TypeName => new TagBaseGitObj(content),
            TreeBaseGitObj.TypeName => new TreeBaseGitObj(content),
            _ => throw new UnreachableException($"Unknown GitObject type '{expectedType}'.")
        };
    }

    /// <summary>
    /// Write the object to the repository. Path from the sha.
    /// </summary>
    /// <param name="repo">Used repository.</param>
    /// <returns>Sha that identifies the git-object. Also used to identify path.</returns>
    public abstract Task<GitSha1> Serialize(Repository repo);
    
    /// <summary>
    /// Calculate the sha for the git-object.
    /// </summary>
    /// <returns>Sha that identifies the object. Also used to identify path.</returns>
    public abstract GitSha1 Hash();
    
    public static GitSha1 HashBaseImplementation(string typeName, byte[] content)
    {
        var header = Encoding.ASCII.GetBytes($"{typeName} {content.Length}\0");
        var combined = header.Concat(content).ToArray();
        return new GitSha1(combined);
    }
}