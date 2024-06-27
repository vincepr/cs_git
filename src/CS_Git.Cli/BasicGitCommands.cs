using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Cocona;
using CS_Git.Cli.IO.ConfigFile;

namespace CS_Git.Cli;

public class BasicGitCommands
{
    [Command("add", Description = "Add file contents to the index")]
    public async Task Add([Argument] string pathToAdd)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repo = await Repository.New(currentDir);

    }
    
    [Command("init", Description = "git-init - Create an empty Git repository or reinitialize an existing one")]
    public async Task Init([Argument(Description = "If you provide a directory, the command is run inside it. If this directory does not exist, it will be created")] string? directory)
    {
        if (directory is null)
            directory = Directory.GetCurrentDirectory();

        Console.WriteLine($"Finished initializing directory at '{directory}'.");
    }
}

class PathExistsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string path && (Directory.Exists(path) || File.Exists(path)))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult($"The path '{value}' is not found.");
    }
}

public class Repository
{
    private readonly string _worktree;
    private readonly string _gitdir;
    private readonly string _gitVersion;
    private readonly IReadOnlyList<ConfigSection> _conf;
    
    private Repository(
        string worktree,
        string gitdir,
        string gitVersion,
        IReadOnlyList<ConfigSection> conf)
    {
        _worktree = worktree;
        _gitdir = gitdir;
        _gitVersion = gitVersion;
        _conf = conf;
    }

    public static async Task<Repository> New(string path, bool force = false)
    {
        var worktree = path;
        var gitdir = Path.Combine(path, ".git");
        if (!force && !Path.Exists(gitdir))
        {
            throw new Exception($"Not a GIT repository {path}");
        }

        var sections = await ConfigFile.Parse(Path.Combine(gitdir, "config"));

        if (!force)
        {
            return new Repository(
                worktree: worktree,
                gitdir: gitdir,
                gitVersion: ReadCorrectVersion(sections),
                conf: sections);
        }

        // we want to be able to force-disable all checks, ex: git init
        return new Repository(
            worktree: worktree,
            gitdir: gitdir,
            gitVersion: "1",
            conf: []);
    }

    public static async Task<Repository> Init(string path)
    {
        const string REPO_DEFAULT_CONFIG = """
                                           [core]
                                               repositoryformatversion = 0
                                               filemode = false
                                               bare = false
                                           """;
        var repo = await New(path, true);
        
        if (Path.Exists(repo._worktree))
        {
            if (!Directory.Exists(repo._worktree))
                throw new Exception($"Not a directory: '{repo._worktree}'");
            if (File.Exists(repo._gitdir))
                throw new Exception($"Not a directory: '{repo._gitdir}'");
            if (Directory.Exists(repo._gitdir) && Directory.EnumerateFiles(repo._gitdir).Count() > 1)
                throw new Exception($".gitdir/ not empty: '{repo._gitdir}'");
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(repo._worktree)!);
        }

        // initialize .gitdir folders:
        repo.CreateRepoDir("branches");
        repo.CreateRepoDir("objects");
        repo.CreateRepoDir("refs", "tags");
        repo.CreateRepoDir("refs", "heads");

        await new StreamWriter(repo.CreateRepoFile("description"))
            .WriteAsync("Unnamed repository; edit this file 'description' to name the repository.\n");

        await new StreamWriter(repo.CreateRepoFile("HEAD"))
            .WriteAsync("ref: refs/heads/master\n");
        
        await new StreamWriter(repo.CreateRepoFile("config"))
            .WriteAsync(REPO_DEFAULT_CONFIG);
        return repo;
    }
    
    // helper functions

    public string RepoPath(params string[] parts)
        => parts.Aggregate(_gitdir, (current, part) => Path.Join(current, part));

    public DirectoryInfo CreateRepoDir(params string[] path)
        => Directory.CreateDirectory(RepoPath(path));

    public FileStream CreateRepoFile(params string[] path)
    {
        var resolvedPath = RepoPath(path);
        if (Directory.Exists(Path.GetDirectoryName(resolvedPath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath)!);
        }

        return File.Open(resolvedPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
    }

    private static string ReadCorrectVersion(List<ConfigSection> sections)
     {
         var gitVersion = sections.First(s => s.Name == "core").Elements.FirstOrDefault(
             (pair) => pair.Key == "repositoryformatversion").Value ?? "0";
         if (gitVersion != "0") throw new Exception($"Unsuppored repositoryformatversion {gitVersion}");
         return gitVersion;
     }
}