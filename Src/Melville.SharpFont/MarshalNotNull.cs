using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpFont;

internal class MarshalNotNull
{
    public static string PtrToStringAnsi(nint ptr) => Marshal.PtrToStringAnsi(ptr)??"";
    public static string PtrToStringAnsi(nint ptr, int len) => Marshal.PtrToStringAnsi(ptr, len)??"";
}