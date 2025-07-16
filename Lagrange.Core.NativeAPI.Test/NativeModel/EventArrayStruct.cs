using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.Test.NativeModel;

[StructLayout(LayoutKind.Sequential)]
public struct EventArrayStruct
{
    public IntPtr Events;
    public int Count;
}