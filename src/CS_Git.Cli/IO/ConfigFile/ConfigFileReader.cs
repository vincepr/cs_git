using System.Diagnostics;

namespace CS_Git.Cli.IO.ConfigFile;

internal class ConfigFileReader : IDisposable
{
    private readonly System.IO.StreamReader _reader;

    public ConfigFileReader(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        _reader = new System.IO.StreamReader(stream);
    }
    
    public async IAsyncEnumerable<ConfigType> ParseElements()
    {
        while (_reader.EndOfStream == false)
        {
            if ((char)_reader.Peek() == '[')
            {
                yield return TryParseSection();
            }

            var configValue = await TryParseValue();
            if (configValue is not null)
            {
                yield return configValue;
            }
        }
    }

    private ConfigType TryParseSection()
    {
        if ((char)_reader.Peek() != '[') throw new UnreachableException();
        var name = String.Empty;
        while (_reader.Peek() != ']')
        {
            name += _reader.Read();
            if (_reader.EndOfStream) throw new UnreachableException("no closing ']' found.");
        }

        _ = _reader.Read(); // consume ]
        return new ConfigType.Section(name.Trim()); 
    }

    private async Task<ConfigType?> TryParseValue()
    {
        var line = await _reader.ReadLineAsync();
        if (line is null) return null; // handle nothing to read
        
        // handle comment - everything till newline gets ignored
        var idx = line.IndexOf(';');
        if (idx != -1)
        {
            line = line.Substring(0, idx);
        }

        // whitespace insensitive
        line = line.Trim();
        if (line.Length < 1) return null;
        
        // we expect 'KEY = SOME_VALUE' like 'Path=./files/my-files.txt'
        idx = line.IndexOf('=');
        if (idx == -1) throw new UnreachableException("expected 'key = value'");
        var key = line.Substring(0, idx);
        var value = line.Substring(idx + 1);
        return new ConfigType.Value(key, value);
    }

    private void SkipWhiteSpace()
    {
        while (!_reader.EndOfStream && _reader.Peek() == 's')
        {
            _reader.Read();
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}