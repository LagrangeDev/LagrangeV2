using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotConfigStruct
    {
        public BotConfigStruct() { }
        public byte Protocol { get; set; } = 0b00000100;

        public bool AutoReconnect { get; set; } = true;

        public bool UseIPv6Network { get; set; } = false;

        public bool GetOptimumServer { get; set; } = true;

        public uint HighwayChunkSize { get; set; } = 1024 * 1024;

        public uint HighwayConcurrent { get; set; } = 4;

        public bool AutoReLogin { get; set; } = true;

        public ByteArrayNative SignAddress { get; set; } = new();

        public static implicit operator BotConfig(BotConfigStruct config)
        {
            return new BotConfig()
            {
                Protocol = (Protocols)config.Protocol,
                AutoReconnect = config.AutoReconnect,
                UseIPv6Network = config.UseIPv6Network,
                GetOptimumServer = config.GetOptimumServer,
                HighwayChunkSize = config.HighwayChunkSize,
                HighwayConcurrent = config.HighwayConcurrent,
                AutoReLogin = config.AutoReLogin,
                SignAddress = Encoding.UTF8.GetString(config.SignAddress.ToByteArrayWithoutFree())
            };
        }

        public static implicit operator BotConfigStruct(BotConfig config)
        {
            return new BotConfigStruct()
            {
                Protocol = (byte)config.Protocol,
                AutoReconnect = config.AutoReconnect,
                UseIPv6Network = config.UseIPv6Network,
                GetOptimumServer = config.GetOptimumServer,
                HighwayChunkSize = config.HighwayChunkSize,
                HighwayConcurrent = config.HighwayConcurrent,
                AutoReLogin = config.AutoReLogin,
                SignAddress = Encoding.UTF8.GetBytes(config.SignAddress ?? "")
            };
        }
    }
}