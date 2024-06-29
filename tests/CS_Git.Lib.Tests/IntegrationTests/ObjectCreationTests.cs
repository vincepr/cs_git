using System.Text;
using CS_Git.Lib.Object;
using CS_Git.Lib.RepositoryLogic;
using NUnit.Framework;

namespace CS_Git.Lib.Tests.IntegrationTests;

public class ObjectCreationTests
{
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
    [TestCase("../../../IntegrationTests/Testfiles/", "codefile.py")]
    [TestCase("../../../IntegrationTests/Testfiles/", "binaryfile")]
    public async Task GitObj_Write_UsesShaAsFolderAndName(string basePath, string file)
    {
        // Arrange
        var absFilePath = Path.Combine(_tempSubdirectory.FullName, "file");
        File.Copy(basePath + file, absFilePath);
        
        // Act
        var sha = await HashObject(absFilePath);
        
        // Assert
        var expectedShaFilePath = Path.Combine(_tempSubdirectory.FullName, ".git","objects", sha.FolderName, sha.FileName);
        Assert.That(File.Exists(expectedShaFilePath));
    }
    
    [Test]
    [TestCase("../../../IntegrationTests/Testfiles/", "binaryfile")]
    [TestCase("../../../IntegrationTests/Testfiles/", "codefile.py")]
    [TestCase("../../../IntegrationTests/Testfiles/", "jsonfile.json")]
    public async Task GitObj_WriteThenRead_ProducesSameFile(string basePath, string file)
    {
        // Arrange
        var absFilePath = Path.Combine(_tempSubdirectory.FullName, "file");
        File.Copy(basePath + file, absFilePath);
        
        // Act
        var sha = await HashObject(absFilePath);
        
        // Assert
        var obj = await CatFile(sha.ToString());
        Assert.That(obj is BlobGitObj);
        Assert.That(await File.ReadAllTextAsync(absFilePath, Encoding.UTF8), Is.EqualTo(((BlobGitObj)obj).Content));
    }
    
    
    private async Task<GitSha1> HashObject(string absolutePath)
    {
        var obj = new BlobGitObj(await File.ReadAllTextAsync(absolutePath, Encoding.UTF8));
        return await obj.Write(_repo);
    }

    private async Task<GitObj> CatFile(string sha) =>
        await GitObj.Read(_repo, GitSha1.FromHexString(sha));
}