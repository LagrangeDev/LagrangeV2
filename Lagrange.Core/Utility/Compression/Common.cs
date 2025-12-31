using System.IO.Compression;

// ReSharper disable MustUseReturnValue

namespace Lagrange.Core.Utility.Compression;

public static class Common
{
    public static byte[] Deflate(byte[] data)
    {
        using var memoryStream = new MemoryStream();
        using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress);
        deflateStream.Write(data, 0, data.Length);
        deflateStream.Close();
        return memoryStream.ToArray();
    }

    public static byte[] Deflate(ReadOnlySpan<byte> data)
    {
        using var memoryStream = new MemoryStream();
        using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress);
        deflateStream.Write(data);
        deflateStream.Close();
        return memoryStream.ToArray();
    }

    public static byte[] Inflate(ReadOnlySpan<byte> data)
    {
        using var input = new MemoryStream(data.Length);
        input.Write(data);
        input.Position = 0;

        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflate.CopyTo(output);

        return output.ToArray();
    }
}