using CS_Git.Cli.IO.ConfigFile;

namespace CS_Git.Cli.Models;

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

        if (!force)
        {
            var sections = await ConfigFile.Parse(Path.Combine(gitdir, "config"));
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

        await Task.WhenAll([
            repo.CreateRepoFile(content: "Unnamed repository; edit this file 'description' to name the repository.\n" ,
                path: "description"),
            repo.CreateRepoFile(content: "ref: refs/heads/master\n", path: "HEAD"),
            repo.CreateRepoFile(content: REPO_DEFAULT_CONFIG, path: "config"),
        ]);
        
        return repo;
    }
    
    // helper functions
    /// <summary>
    /// Recursively search trough parents till '.git' directory is identified. 
    /// </summary>
    public static string? Find(string absolutePath)
    {
        if (Directory.Exists(absolutePath) && Directory.Exists(Path.Join(absolutePath, ".git")))
            return absolutePath;    // found .git folder

        return  Path.GetDirectoryName(absolutePath) switch
        {
            null => null,   // reached root -> no .git folder available
            var parentDirectory => Find(parentDirectory)    // recurse parentFolder
        };
    }
    
    public string RepoPath(params string[] parts)
    {
        if (parts.Length < 1) throw new ArgumentOutOfRangeException(nameof(parts));
        return parts.Aggregate(_gitdir, (current, part) => Path.Join(current, part));
    }

    public DirectoryInfo CreateRepoDir(params string[] path)
        => Directory.CreateDirectory(RepoPath(path));

    private StreamWriter CreateRepoFileWriter(params string[] path)
    {
        var resolvedPath = RepoPath(path);
        if (Directory.Exists(Path.GetDirectoryName(resolvedPath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath)!);
        }

        return new StreamWriter(File.Open(resolvedPath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
            FileShare.ReadWrite));
    }

    public async Task CreateRepoFile(string content, params string[] path)
    {
        await using var writer = CreateRepoFileWriter(path);
        await writer.WriteAsync(content);
    }

    private static string ReadCorrectVersion(List<ConfigSection> sections)
    {
        var gitVersion = sections.First(s => s.Name == "core").Elements.FirstOrDefault(
            (pair) => pair.Key == "repositoryformatversion").Value ?? "0";
        if (gitVersion != "0") throw new Exception($"Unsuppored repositoryformatversion {gitVersion}");
        return gitVersion;
    }
}