using System.Diagnostics;
using CS_Git.Lib.RepositoryLogic.ConfigFile;

namespace CS_Git.Lib.Tests.IntegrationTests.TestFiles;

// ReSharper disable once UnusedType.Global
// ReSharper disable once InconsistentNaming
internal static class csharpfile_with_bom
{
    private static List<ConfigSection> Dummyfile(string filePath)
    {
        List<ConfigSection> sections = [];
        Console.WriteLine("This is just a dummy file for testing.");
        return sections;
    }
}