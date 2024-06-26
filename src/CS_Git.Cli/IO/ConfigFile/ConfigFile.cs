using System.Diagnostics;

namespace CS_Git.Cli.IO.ConfigFile;

public static class ConfigFile
{

    public static async Task<List<ConfigSection>> Parse(string filePath)
    {
        List<ConfigSection> sections = [];
        using var reader = new ConfigFileReader(filePath);
        
        await foreach (var configType in reader.ParseElements())
        {
            switch (configType)
            {
                case ConfigType.Section section:
                    sections.Add(new ConfigSection(section.Name, []));
                    break;
                case ConfigType.SubSection subSection:
                    throw new UnreachableException("Unimplemented");
                    break;
                case ConfigType.Value value:
                    sections.Last().Elements.Add(new KeyValuePair<string, string>(value.Key, value.Val));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(configType), "Unimplemented ConfigType");
            }
        }

        return sections;
    }
}