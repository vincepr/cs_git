namespace CS_Git.Lib.RepositoryLogic.ConfigFile;

internal record ConfigType
{
    internal record Section(string Name): ConfigType;

    internal record SubSection(string Parent, string Name): ConfigType;

    internal record Value(string Key, string Val): ConfigType;
}