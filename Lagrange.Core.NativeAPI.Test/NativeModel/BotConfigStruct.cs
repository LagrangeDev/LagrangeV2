using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.Test.NativeModel
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotConfigStruct
    {
        public BotConfigStruct()
        {
        }

        public byte Protocol { get; set; } = 0b00000100;

        public bool AutoReconnect { get; set; } = true;

        public bool UseIPv6Network { get; set; } = false;

        public bool GetOptimumServer { get; set; } = true;

        public uint HighwayChunkSize { get; set; } = 1024 * 1024;

        public uint HighwayConcurrent { get; set; } = 4;

        public bool AutoReLogin { get; set; } = true;

        public ByteArrayNative addr { get; set; } = new();
    }
}