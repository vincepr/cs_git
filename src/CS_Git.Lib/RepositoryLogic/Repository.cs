using CS_Git.Lib.RepositoryLogic.ConfigFile;

namespace CS_Git.Lib.RepositoryLogic;

public class Repository
{
    public readonly string _worktree;
    public readonly string _gitdir;
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

    /// <summary>
    /// Read repository from provided path. Able to force a empty-object with force.
    /// </summary>
    /// <exception cref="Exception">No Git repository found.</exception>
    public static async Task<Repository> New(string path, bool force = false)
    {
        var worktree = path;
        var gitdir = Path.Combine(path, ".git");
        if (!force && !Directory.Exists(gitdir))
        {
            throw new Exception($"Not a Git repository in: {path}");
        }

        if (!force)
        {
            var sections = await ConfigFile.ConfigFile.Parse(Path.Combine(gitdir, "config"));
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

    /// <summary>
    /// Initialize new repository.
    /// </summary>
    /// <param name="path">absolute path where the '/.git' folder will be created in.</param>
    /// <returns>The newly created <see cref="Repository"/>.</returns>
    /// <exception cref="Exception">Various checks, to not overwrite existing repository might fail.</exception>
    public static async Task<Repository> Init(string path)
    {
        const string REPO_DEFAULT_CONFIG = """
                                           [core]
                                               repositoryformatversion = 0
                                               filemode = false
                                               bare = false
                                           """;
        var tempRepo = await New(path, force: true);
        
        if (Path.Exists(tempRepo._worktree))
        {
            if (!Directory.Exists(tempRepo._worktree))
                throw new Exception($"Not a directory: '{tempRepo._worktree}'");
            if (File.Exists(tempRepo._gitdir))
                throw new Exception($"Not a directory: '{tempRepo._gitdir}'");
            if (Directory.Exists(tempRepo._gitdir) && Directory.EnumerateFiles(tempRepo._gitdir).Count() > 1)
                throw new Exception($".gitdir/ not empty: '{tempRepo._gitdir}'");
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(tempRepo._worktree)!);
        }

        // initialize .gitdir folders:
        tempRepo.CreateRepoDir("branches");
        tempRepo.CreateRepoDir("objects");
        tempRepo.CreateRepoDir("refs", "tags");
        tempRepo.CreateRepoDir("refs", "heads");

        await Task.WhenAll([
            tempRepo.CreateRepoFile(content: "Unnamed repository; edit this file 'description' to name the repository.\n" ,
                path: "description"),
            tempRepo.CreateRepoFile(content: "ref: refs/heads/master\n", path: "HEAD"),
            tempRepo.CreateRepoFile(content: REPO_DEFAULT_CONFIG, path: "config"),
        ]);
        
        return await Repository.New(path);
    }

    public static async Task<Repository> FindRecursiveAndRead(string path = ".") =>
        path switch
        {
            "." => await New(Find(Directory.GetCurrentDirectory())),
            var p => await New(Find(Path.GetFullPath(p)))
        };


    /// <summary>
    /// Recursively search trough parents till '.git' directory is identified. 
    /// </summary>
    /// <param name="startingPath">absolute path of directory where to start from.</param>
    /// <returns>absolute path to the project root.</returns>
    /// <exception cref="Exception">Went up to root. But found no '/.git'.</exception>
    public static string Find(string startingPath) =>
        FindRecursive(startingPath) ?? throw new Exception($"Unable to find '/.git' above path: {startingPath}");

    private static string? FindRecursive(string absolutePath)
    {
        if (Directory.Exists(absolutePath) && Directory.Exists(Path.Combine(absolutePath, ".git")))
            return absolutePath;    // found .git folder

        return  Path.GetDirectoryName(absolutePath) switch
        {
            null => null,   // reached root -> no .git folder available
            var parentDirectory => Find(parentDirectory)    // recurse parentFolder
        };
    }

    private string RepoPath(params string[] parts)
    {
        if (parts.Length < 1) throw new ArgumentOutOfRangeException(nameof(parts));
        return parts.Aggregate(_gitdir, (current, part) => Path.Combine(current, part));
    }

    private DirectoryInfo CreateRepoDir(params string[] path)
        => Directory.CreateDirectory(RepoPath(path));

    
    public Stream CreateRepoFileStreamWriter(params string[] path)
    {
        var resolvedPath = RepoPath(path);
        if (Directory.Exists(Path.GetDirectoryName(resolvedPath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath)!);
        }

        return File.Open(resolvedPath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
            FileShare.ReadWrite);
    }
    
    public Stream CreateRepoFileStreamReadonly(params string[] path)
    {
        var resolvedPath = RepoPath(path);
        if (Directory.Exists(Path.GetDirectoryName(resolvedPath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath)!);
        }

        return new FileStream(resolvedPath, FileMode.Open, FileAccess.Read,
            FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    private async Task CreateRepoFile(string content, params string[] path)
    {
        await using var writer = new StreamWriter(CreateRepoFileStreamWriter(path));
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