using Cocona;

namespace CS_Git.Cli.CliCommands.CoconaLogic;

// ReSharper disable once ClassNeverInstantiated.Global

/// <summary>
/// Parameter used to be able to accept both (relativePath || absolutePath) -> automatically convert to (absolute-path).
/// Is optional, default to ".".
/// </summary>
public class DirectoryRequiredArgument : ICommandParameterSet
{
    public string AbsolutePath { get; init; }
    public string? OriginalInput { get; init; }

    public DirectoryRequiredArgument([Argument(Description = "If you provide a directory, the command is run inside it. If this directory does not exist, it will be created")] string? directory = null)
    {
        OriginalInput = directory;
        directory ??= Directory.GetCurrentDirectory();
        AbsolutePath = Path.GetFullPath(directory);
    }
}