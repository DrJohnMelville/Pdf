using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

/// <summary>
/// This is a simple struct that abstracts the process of renting and returning an
/// array into a IDisposable operation.  We have to keep a reference to the
/// underlying array so we can return it when we are done.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct RentedBuffer1<T>: IDisposable
{
    /// <summary>
    /// The rented buffer as a Memory
    /// </summary>
    public Memory<T> Memory { get; }
    /// <summary>
    /// The rented buffer as a Span.
    /// </summary>
    public Span<T> Span => Memory.Span;
    private readonly T[] array;

    /// <summary>
    /// Create a render buffer with a requested length.
    ///
    /// Note that the exposed memory will be exactly the requested length.
    /// This struct hides the fact tha ArrayPool can return a larger array
    /// than requested.
    /// </summary>
    /// <param name="length">The length of the requested buffer.</param>
    public RentedBuffer1(int length)
    {
        array = ArrayPool<T>.Shared.Rent(length);
        Memory = array.AsMemory(0, length);
    }

    /// <summary>
    /// Returns the rented buffer to the ArrayPool.
    /// </summary>
    public void Dispose() => ArrayPool<T>.Shared.Return(array);
}