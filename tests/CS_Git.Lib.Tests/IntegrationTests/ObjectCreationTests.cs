using System.Text;
using CS_Git.Lib.Object;
using CS_Git.Lib.RepositoryLogic;
using NUnit.Framework;

namespace CS_Git.Lib.Tests.IntegrationTests;

[Parallelizable(ParallelScope.Self)]
public class ObjectCreationTests
{
    private static readonly char Slash = Path.DirectorySeparatorChar;
    private static readonly string TestFilesPath = $"..{Slash}..{Slash}..{Slash}IntegrationTests{Slash}Testfiles{Slash}";
    private DirectoryInfo _tempSubdirectory = null!;
    private Repository _repo = null!;

    [SetUp]
    public async Task Setup()
    {
        _tempSubdirectory = Directory.CreateTempSubdirectory();
        _repo = await Repository.Init(_tempSubdirectory.FullName);
    }

    [TearDown]
    public void TearDown()
    {
        _tempSubdirectory.Delete(recursive: true);
    }

    [Test]
    [TestCase("codefile.py")]
    [TestCase("binaryfile")]
    public async Task GitObj_Write_UsesShaAsFolderAndName(string file)
    {
        // Arrange
        var absFilePath = Path.Combine(_tempSubdirectory.FullName, "file");
        File.Copy(TestFilesPath + file, absFilePath);
        
        // Act
        var sha = await HashObject(absFilePath);
        
        // Assert
        var expectedShaFilePath = Path.Combine(_tempSubdirectory.FullName, ".git","objects", sha.FolderName, sha.FileName);
        Assert.That(File.Exists(expectedShaFilePath));
    }
    
    [Test]
    [TestCase("binaryfile")]
    [TestCase("codefile.py")]
    [TestCase("csharpfile_no_bom.cs")]
    [TestCase("csharpfile_with_bom.cs")]
    [TestCase("jsonfile.json")]
    public async Task GitObj_WriteThenRead_ProducesSameString(string file)
    {
        // Arrange
        var absFilePath = Path.Combine(_tempSubdirectory.FullName, "file");
        File.Copy(TestFilesPath + file, absFilePath);
        
        // Act
        var sha = await HashObject(absFilePath);
        
        // Assert
        var obj = await CatFile(sha.ToString());
        Assert.That(obj is BlobGitObj);
        Assert.That(await File.ReadAllTextAsync(absFilePath, Encoding.UTF8), Is.EqualTo(((BlobGitObj)obj).Content));
    }
    
    [Test]
    [TestCase("binaryfile")]
    [TestCase("codefile.py")]
    [TestCase("csharpfile_no_bom.cs")]
    [TestCase("csharpfile_with_bom.cs")]    // TODO: even when this one should break it doesn't. Find out why
    [TestCase("jsonfile.json")]
    public async Task GitObj_WriteThenRead_ProducesSameFile(string file)
    {
        // Arrange
        var absFilePath = Path.Combine(_tempSubdirectory.FullName, "file");
        File.Copy(TestFilesPath + file, absFilePath);
        
        // Act
        var sha = await HashObject(absFilePath);
        
        // Assert
        var obj = await CatFile(sha.ToString());
        Assert.That(obj is BlobGitObj);
        Assert.That(await File.ReadAllTextAsync(absFilePath, Encoding.UTF8), Is.EqualTo(((BlobGitObj)obj).Content));
        var createdFile = ObjToFile((BlobGitObj)obj, _repo);
        Assert.That(File.ReadAllBytes(absFilePath), Is.EqualTo(File.ReadAllBytes(createdFile)));
    }
    
    private async Task<GitSha1> HashObject(string absolutePath)
    {
        var obj = new BlobGitObj(await File.ReadAllTextAsync(absolutePath, Encoding.UTF8));
        return await obj.Write(_repo);
    }

    private async Task<GitObj> CatFile(string sha) =>
        await GitObj.Read(_repo, GitSha1.FromHexString(sha));

    private string ObjToFile(BlobGitObj obj, Repository repo)
    {
        string filepath = repo._worktree + Slash + "file.txt";
        // TODO we should probably refactor to use bytes[] instead of string generally to get rid of encoding!
        File.WriteAllText(filepath, obj.Content, Encoding.UTF8);
        return filepath;
    }
}