using System.Buffers.Binary;
using System.Text;

namespace Lagrange.Core.Utility.Compression;

public static class ZCompression
{
    public static byte[] ZCompress(byte[] data, byte[]? header = null)
    {
        using var stream = new MemoryStream();
        var deflate = Common.Deflate(data);

        stream.Write(header);
        stream.WriteByte(0x78); // Zlib header
        stream.WriteByte(0xDA); // Zlib header

        stream.Write(deflate.AsSpan());

        Span<byte> checksum = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksum, Adler32(data));
        stream.Write(checksum);

        return stream.ToArray();
    }

    public static byte[] ZCompress(string data, byte[]? header = null) => ZCompress(Encoding.UTF8.GetBytes(data), header);

    public static byte[] ZCompress(ReadOnlySpan<byte> data, ReadOnlySpan<byte> header = default)
    {
        using var stream = new MemoryStream();
        var deflate = Common.Deflate(data);

        stream.Write(header);
        stream.WriteByte(0x78); // Zlib header
        stream.WriteByte(0xDA); // Zlib header

        stream.Write(deflate.AsSpan());

        Span<byte> checksum = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksum, Adler32(data));
        stream.Write(checksum);

        return stream.ToArray();
    }

    public static byte[] ZDecompress(ReadOnlySpan<byte> data, bool validate = true)
    {
        uint expectedChecksum = BinaryPrimitives.ReadUInt32BigEndian(data[^4..]);

        var inflate = Common.Inflate(data[2..^4]);
        if (validate && Adler32(inflate) != expectedChecksum) throw new Exception("Checksum mismatch");

        return inflate;
    }

    private static uint Adler32(ReadOnlySpan<byte> data)
    {
        uint a = 1, b = 0;
        foreach (byte t in data)
        {
            a = (a + t) % 65521;
            b = (b + a) % 65521;
        }
        return (b << 16) | a;
    }
}
