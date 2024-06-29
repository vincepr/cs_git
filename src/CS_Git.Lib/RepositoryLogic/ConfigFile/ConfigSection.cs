namespace CS_Git.Lib.RepositoryLogic.ConfigFile;

public record ConfigSection(string Name, List<KeyValuePair<string, string>> Elements);