namespace CS_Git.Cli.RepositoryLogic.ConfigFile;

public record ConfigSection(string Name, List<KeyValuePair<string, string>> Elements);