using System.Diagnostics;
using CS_Git.Cli.CliCommands;
using CS_Git.Cli.CliCommands.CoconaLogic;
using NUnit.Framework;

namespace CS_Git.Cli.Tests.CliCommands;

public class HashObjectCommandTests
{
    private static readonly char Slash = Path.DirectorySeparatorChar;
    private static readonly string TestFilesPath = $"..{Slash}..{Slash}..{Slash}CliCommands{Slash}Testfiles{Slash}";
    private StringWriter _stringWriter = null!;
    private static BasicGitCommands _com = new();

    [SetUp]
    public void Setup()
    {
        _stringWriter = new StringWriter();
        Console.SetOut(_stringWriter);
    }

    [TestCase("hashtestbinaryfile", "8c48471bdb782797a91d8fe3955a70131abc5150")]
    [TestCase("hashtestfile", "f10745c515e5f1d2739d9ce3856e492e0729828e")]
    [TestCase("README.md", "d99b9f43089191d561943ba3fa10541c6fc769c2")]
    public async Task Textfile_GitHashed_Equals_CsgitHashed(string file, string expectedHash)
    {
        // Act
        await _com.HashObject(new FileRequiredArgument($"{TestFilesPath}{file}"));
        
        // Assert
        var output = _stringWriter.ToString();
        Assert.That(output, Is.EqualTo(expectedHash));
    }
    
    [TestCase("hashtestbinaryfile")]
    [TestCase("hashtestfile")]
    [TestCase("README.md")]
    public async Task ProducesEqualHash_Git_CsGit(string file)
    {
        // Arrange
        var expectedHash = CaptureGitHashObject($"{TestFilesPath}{file}");
        
        // Act
        await _com.HashObject(new FileRequiredArgument($"{TestFilesPath}{file}"));
        
        // Assert
        var output = _stringWriter.ToString();
        Assert.That(expectedHash, Is.EqualTo(output));
    }

    // auto crlf might fuck these tests up. As some of these test files might have 'other' line-endings.
    // it is disabled on purpose on the whole repository. Also because CsGit will not support any auto-crlf before hashing.
    private string CaptureGitHashObject(string path)
    {
        var proc = new Process 
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"hash-object {path}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        
        proc.Start();
        string output = String.Empty;
        while (!proc.StandardOutput.EndOfStream)
        {
            output += proc.StandardOutput.ReadLine()!;
        }

        return output;
    }
}