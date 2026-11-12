using Melville.StreamInterfaces.Streams;
using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

/// <summary>
/// bridge class to get pointers out of memory as I rewrite bitmap writing.
/// </summary>
public static class MemoryToPointerImpl
{
    /// <summary>
    /// Temporary bridge method
    /// </summary>
    public static unsafe T* GetPointer<T>(this Memory<T> source) where T: unmanaged=>
        (T*)Unsafe.AsPointer(ref source.Span[0]);
}
