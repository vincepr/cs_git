using System.Security.Cryptography;

namespace CS_Git.Lib.GitObjectLogic;

public struct GitSha1
{
    /// <summary>
    /// Not the bytes hashed but the hash stored in bytes.
    /// </summary>
    public readonly byte[] HashAsBytes { get; init; }
    public string HashAsString { get; init; }
    public string FolderName => HashAsString[..2];
    public string FileName => HashAsString[2..];
    
    private const int ExpectedLength = 40;
    
    public GitSha1(byte[] content)
    {
        var bytes = SHA1.HashData(content);
        var sha = Convert.ToHexString(bytes).ToLower();
        CheckExpectedLength(content, sha);
        HashAsBytes = content;
        HashAsString = sha;
    }
    

    private static void CheckExpectedLength<T>(T data, string sha)
    {
        if (sha.Length != ExpectedLength) 
            throw new ArgumentOutOfRangeException(
                $"{nameof(data)}", $"Expected length of {ExpectedLength}, got {sha.Length}. from:[{data}] -> to:[{sha}]");
    }

    public static GitSha1 FromHexString(string shaHexString)
    {
        var data = Convert.FromHexString(shaHexString);
        CheckExpectedLength(data, shaHexString);
        return new GitSha1
        {
            HashAsString = shaHexString,
            HashAsBytes = data,
        };
    }

    public override string ToString() => HashAsString;
}