using Melville.StreamInterfaces.Streams;
using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

// code stolen from https://github.com/mgravell/Pipelines.Sockets.Unofficial/blob/master/src/Pipelines.Sockets.Unofficial/UnsafeMemory.cs
/// <summary>
/// This wraps a pointer and a length in a MemoryManager that can create a Memory&lt;T&gt; this is a little
/// bit of a shim until I can create proper memory managers at all of the call sites.
/// </summary>
/// <typeparam name="T">The element type of the memory's created</typeparam>
[Obsolete("See if I can come up with some safer alternatives.")]
public sealed unsafe class UnmanagedMemoryManager<T> : MemoryManager<T>
     where T : unmanaged
{
    private readonly T* _pointer;
    private readonly int _length;


    /// <summary>
    /// Create a new UnmanagedMemoryManager instance at the given pointer and size
    /// </summary>
    public UnmanagedMemoryManager(T* pointer, int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        _pointer = pointer;
        _length = length;
    }

    /// <summary>
    /// Create a new UnmanagedMemoryManager instance at the given pointer and size
    /// </summary>
    public UnmanagedMemoryManager(IntPtr pointer, int length) : this((T*)pointer.ToPointer(), length) { }

    /// <summary>
    /// Obtains a span that represents the region
    /// </summary>
    public override Span<T> GetSpan() => new Span<T>(_pointer, _length);

    /// <summary>
    /// Provides access to a pointer that represents the data (note: no actual pin occurs)
    /// </summary>
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if ((uint)elementIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));
        return new MemoryHandle(_pointer + elementIndex);
    }
    /// <summary>
    /// Has no effect
    /// </summary>
    public override void Unpin() { }

    /// <summary>
    /// Releases all resources associated with this object
    /// </summary>
    protected override void Dispose(bool disposing) { }
}