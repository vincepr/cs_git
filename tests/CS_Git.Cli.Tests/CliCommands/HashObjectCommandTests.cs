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

    // auto crlf might fuck these tests up. As some of these test files might have 'other' line-endings.
    // it is disabled on purpose on the whole repository. Also because CsGit will not support any auto-crlf before hashing.
    // remark for sanity reasons we just rewrite the tests for 'fixed' crlf files
    [TestCase("hashtestbinaryfile", "8c48471bdb782797a91d8fe3955a70131abc5150")]
    [TestCase("hashtestfile", "ada51d4bcc6374b76b0041aa316409938ff900c7")]
    [TestCase("README.md", "96585df1178e25ace823205e9a3730899fce4812")]
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