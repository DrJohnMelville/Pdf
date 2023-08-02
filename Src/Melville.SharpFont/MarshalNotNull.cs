using System.Runtime.InteropServices;

namespace Melville.SharpFont;

internal class MarshalNotNull
{
    public static string PtrToStringAnsi(nint ptr) => Marshal.PtrToStringAnsi(ptr)??"";
    public static string PtrToStringAnsi(nint ptr, int len) => Marshal.PtrToStringAnsi(ptr, len)??"";
}