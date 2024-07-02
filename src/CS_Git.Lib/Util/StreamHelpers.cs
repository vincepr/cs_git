using System.IO.Compression;
using System.Text;

namespace CS_Git.Lib.Util;

public static class StreamHelpers
{
    public static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        // byte order mark is just not needed here, but dotnet will try it hardest to use it by default:
        Encoding forceNoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        var writer = new StreamWriter(stream, forceNoBom);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
    
    public static void CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[8 * 1024];
        int len;
        while ( (len = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, len);
        }    
    }
    
    public static byte[] RawStreamToBytes(Stream stream)
    {
        if (stream is MemoryStream memStream)
            return memStream.ToArray();

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
    
    public static byte[] DecompressDataStreamToBytes(Stream input)
    {
        using var decompressor = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        decompressor.CopyTo(output);
        return output.ToArray();
    }
}