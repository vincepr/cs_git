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
    [TestCase("hashtestfile", "a8dae722a40f0a5e293274d952c181e94129cbbf")]
    [TestCase("README.md", "482751db402bd760793b1237086e94535e8afa94")]
    // auto crlf might fuck these tests up. As some of these test files might have 'other' line-endings.
    // So if README.md breaks, autocrlf probably touched the readme.
    public async Task Textfile_GitHashed_ProducesExpectedHash(string file, string expectedHash)
    {
        // Act
        await _com.HashObject(new FileRequiredArgument($"{TestFilesPath}{file}"));
        
        // Assert
        var output = _stringWriter.ToString();
        Assert.That(output, Is.EqualTo(expectedHash));
    }
    
    [TestCase("hashtestbinaryfile")]
    [TestCase("hashtestfile")]
    // [TestCase("README.md")]   auto-crlf touches this. git config core.autocrlf = false will make this pass
    // hash with crlf = false                       hash with crlf = true
    // 482751db402bd760793b1237086e94535e8afa94 vs d7f17bdb602afc4309895f828780d98bc226293a
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