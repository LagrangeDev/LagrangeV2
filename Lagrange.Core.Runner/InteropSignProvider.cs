using System.Runtime.InteropServices;
using Lagrange.Core.Common;

namespace Lagrange.Core.Runner;

public partial class InteropSignProvider : BotSignProvider
{
    private static readonly HashSet<string> WhiteListCommand =
    [
        "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey",
        "trpc.o3.ecdh_access.EcdhAccess.SsoSecureAccess",
        "trpc.o3.report.Report.SsoReport",
        "MessageSvc.PbSendMsg",
        "wtlogin.trans_emp",
        "wtlogin.login",
        "wtlogin.exchange_emp",
        "trpc.login.ecdh.EcdhService.SsoKeyExchange",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginNewDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginRefreshTicket",
        "trpc.login.ecdh.EcdhService.SsoNTLoginRefreshA2",
        "OidbSvcTrpcTcp.0x11ec_1",
        "OidbSvcTrpcTcp.0x758_1", // create group
        "OidbSvcTrpcTcp.0x7c1_1",
        "OidbSvcTrpcTcp.0x7c2_5", // request friend
        "OidbSvcTrpcTcp.0x10db_1",
        "OidbSvcTrpcTcp.0x8a1_7", // request group
        "OidbSvcTrpcTcp.0x89a_0",
        "OidbSvcTrpcTcp.0x89a_15",
        "OidbSvcTrpcTcp.0x88d_0", // fetch group detail
        "OidbSvcTrpcTcp.0x88d_14",
        "OidbSvcTrpcTcp.0x112a_1",
        "OidbSvcTrpcTcp.0x587_74",
        "OidbSvcTrpcTcp.0x1100_1",
        "OidbSvcTrpcTcp.0x1102_1",
        "OidbSvcTrpcTcp.0x1103_1",
        "OidbSvcTrpcTcp.0x1107_1",
        "OidbSvcTrpcTcp.0x1105_1",
        "OidbSvcTrpcTcp.0xf88_1",
        "OidbSvcTrpcTcp.0xf89_1",
        "OidbSvcTrpcTcp.0xf57_1",
        "OidbSvcTrpcTcp.0xf57_106",
        "OidbSvcTrpcTcp.0xf57_9",
        "OidbSvcTrpcTcp.0xf55_1",
        "OidbSvcTrpcTcp.0xf67_1",
        "OidbSvcTrpcTcp.0xf67_5",
        "OidbSvcTrpcTcp.0x6d9_4"
    ];
    
    private const string MicroblockSign = "libMicroblockSign";
    
    [LibraryImport(MicroblockSign, EntryPoint = "attach")] [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool Attach();

    [LibraryImport(MicroblockSign, EntryPoint = "signData")] [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool SignData(
        IntPtr  cmdName,                     // const char*
        IntPtr  srcData,                     // const uint8_t*
        nuint   srcDataSize,                 // size_t  → nuint (platform-native)
        long    seq,                         // int64_t
        IntPtr  extraOut, ref nuint extraOutSize,
        IntPtr  tokenOut, ref nuint tokenOutSize,
        IntPtr  signOut,  ref nuint signOutSize);

    public InteropSignProvider()
    {
        bool ok = Attach();
        if (!ok) throw new Exception("Failed to attach to MicroblockSign library.");
    }

    public override bool IsWhiteListCommand(string cmd) => WhiteListCommand.Contains(cmd);

    public override Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
    {
        nuint extraOutSize = 0u;
        nuint tokenOutSize = 0u;
        nuint signOutSize  = 0u;

        var extraOut = Marshal.AllocHGlobal(600);
        var tokenOut = Marshal.AllocHGlobal(200);
        var signOut  = Marshal.AllocHGlobal(400);
        
        var srcData = Marshal.AllocHGlobal(body.Length);
        var cmdName = Marshal.StringToHGlobalAnsi(cmd);
        Marshal.Copy(body.Span.ToArray(), 0, srcData, body.Length);
        
        try
        {
            if (SignData(cmdName, srcData, (nuint)body.Length, seq, extraOut, ref extraOutSize, tokenOut, ref tokenOutSize, signOut, ref signOutSize))
            {
                var extra = new byte[extraOutSize];
                var token = new byte[tokenOutSize];
                var sign  = new byte[signOutSize];

                Marshal.Copy(extraOut, extra, 0, (int)extraOutSize);
                Marshal.Copy(tokenOut, token, 0, (int)tokenOutSize);
                Marshal.Copy(signOut, sign, 0, (int)signOutSize);

                return Task.FromResult<SsoSecureInfo?>(new SsoSecureInfo
                {
                    SecSign = sign.ToArray(),
                    SecToken = token.ToArray(),
                    SecExtra = extra.ToArray()
                });
            }
            else
            {
                return Task.FromResult<SsoSecureInfo?>(null);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(extraOut);
            Marshal.FreeHGlobal(tokenOut);
            Marshal.FreeHGlobal(signOut);
            Marshal.FreeHGlobal(srcData);
            Marshal.FreeHGlobal(cmdName);
        }
    }
}