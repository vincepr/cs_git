using System.Text;
using CS_Git.Lib.GitObjectLogic;
using CS_Git.Lib.GitObjectLogic.ObjTypes;
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
    [TestCase("README.md")]
    public async Task GitObj_WriteThenRead_ProducesSameString(string file)
    {
        // Arrange
        var absFilePath = Path.Combine(_tempSubdirectory.FullName, "file");
        File.Copy(TestFilesPath + file, absFilePath);
        
        // Act
        var sha = await HashObject(absFilePath);
        
        // Assert
        var obj = await CatFile(sha.ToString());
        Assert.That(obj is BlobBaseGitObj);
        Assert.That(obj.ToString(), Is.EquivalentTo(Encoding.UTF8.GetString(File.ReadAllBytes(absFilePath))));
        
        // following assert disabled, because it will break on: [TestCase("csharpfile_with_bom.cs")]
        // BOM, byte order mark, related. Could be actually want the other bom-behavior? But this seems cleaner byte wise.
        // // Assert.That(obj.ToString(), Is.EquivalentTo(await File.ReadAllTextAsync(absFilePath, Encoding.UTF8)));
    }
    
    [Test]
    [TestCase("binaryfile")]
    [TestCase("codefile.py")]
    [TestCase("csharpfile_no_bom.cs")]
    [TestCase("csharpfile_with_bom.cs")]
    [TestCase("jsonfile.json")]
    [TestCase("README.md")]
    public async Task GitObj_WriteThenRead_ProducesSameFile_ByteComparison(string file)
    {
        // Arrange
        var absFilePath = Path.Combine(_tempSubdirectory.FullName, "file");
        File.Copy(TestFilesPath + file, absFilePath);
        
        // Act
        var sha = await HashObject(absFilePath);
        
        // Assert
        var obj = await CatFile(sha.ToString());
        Assert.That(await File.ReadAllBytesAsync(absFilePath), Is.EquivalentTo(((BlobBaseGitObj)obj).Content));
        var createdFile = ObjToFile((BlobBaseGitObj)obj, _repo);
        Assert.That(
            await File.ReadAllBytesAsync(createdFile), Is.EquivalentTo(await File.ReadAllBytesAsync(absFilePath)));
    }
    
    private async Task<GitSha1> HashObject(string absolutePath)
    {
        var obj = new BlobBaseGitObj(await File.ReadAllBytesAsync(absolutePath));
        return await obj.Write(_repo);
    }

    private async Task<BaseGitObj> CatFile(string sha) =>
        await BaseGitObj.Read(_repo, GitSha1.FromHexString(sha));

    private string ObjToFile(BlobBaseGitObj obj, Repository repo)
    {
        string filepath = repo._worktree + Slash + "file.txt";
        File.WriteAllBytes(filepath, obj.Content);
        return filepath;
    }
}