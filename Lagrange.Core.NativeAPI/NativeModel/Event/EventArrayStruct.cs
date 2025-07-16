using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Event;

[StructLayout(LayoutKind.Sequential)]
public struct EventArrayStruct
{
    public IntPtr Events;
    public int Count;
}