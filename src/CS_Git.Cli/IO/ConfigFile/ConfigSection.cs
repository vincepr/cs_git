namespace CS_Git.Cli.IO.ConfigFile;

public record ConfigSection(string Name, List<KeyValuePair<string, string>> Elements);