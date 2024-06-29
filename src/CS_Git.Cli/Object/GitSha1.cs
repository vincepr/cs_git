using System.Security.Cryptography;
using System.Text;

namespace CS_Git.Cli.GitObj;

public struct GitSha1
{
    public string FolderName;
    public string FileName;
    
    private const int ExpectedLength = 40;
    
    public GitSha1(byte[] content)
    {
        var sha = Convert.ToHexString(SHA1.HashData(content));
        ConstructorHelper(content, sha);
    }
    
    public GitSha1(char[] content)
    {
        var sha = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(content)));
        ConstructorHelper(content, sha);
    }

    private void ConstructorHelper<T>(T data, string sha)
    {
        if (sha.Length != ExpectedLength)
            throw new ArgumentOutOfRangeException(
                $"{nameof(data)}", $"Expected length of {ExpectedLength}, got {sha.Length}. from:[{data}] -> to:[{sha}]");

        FolderName = sha[..2];
        FileName = sha[2..];
    }

    public GitSha1(string content)
    {
        var sha = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(content)));
        ConstructorHelper(content, sha);
    }

    public override string ToString() => $"={FolderName}{FileName}";
}