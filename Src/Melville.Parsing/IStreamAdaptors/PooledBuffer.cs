using Melville.INPC;
using Melville.StreamInterfaces;
using System;
using System.Buffers;

namespace Melville.Parsing.IStreamAdaptors;

/// <summary>
/// This is a disposable struct that rents a buffer from the ArrayPool,
/// and then returns it in the destructor.  
/// </summary>
/// <param name="minLength">The minimum length of the desired array.</param>
public readonly partial struct PooledBuffer<T>(int minLength) : IDisposable
{
    readonly T[] bytes = ArrayPool<T>.Shared.Rent(minLength);

    ///<inheritdoc/>
    public void Dispose() => ArrayPool<T>.Shared.Return(bytes);

    /// <summary>
    /// Get a reference to an element of the buffer
    /// </summary>
    /// <param name="index">Index of the element to get.</param>
    /// <returns>A reference to the specific location in the array.</returns>
    public ref T this[int index] => ref bytes[index];
    
    /// <summary>
    /// The length of the buffer, which may be bigger than the requested size.
    /// </summary>
    public int Length => bytes.Length;

    /// <summary>
    /// A span as the underlying array.
    /// </summary>
    public Span<T> AsSpan() => bytes.AsSpan();
    
    /// <summary>
    /// A span of the underlying array
    /// </summary>
    /// <param name="start">Starting position</param>
    public Span<T> AsSpan(int start) => bytes.AsSpan(start);

    /// <summary>
    /// A span of the underlying array
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="length">The length of the requested span</param>
    public Span<T> AsSpan(int start, int length) => bytes.AsSpan(start, length);

    /// <summary>
    /// A memory of the underlying array
    /// </summary>
    public Memory<T> AsMemory() => bytes.AsMemory();

    /// <summary>
    /// A memory of the underlying array
    /// </summary>
    /// <param name="start">Starting position</param>
    public Memory<T> AsMemory(int start) => bytes.AsMemory(start);

    /// <summary>
    /// A memory of the underlying array
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="length">The length of the requested span</param>
    public Memory<T> AsMemory(int start, int length) => bytes.AsMemory(start, length);

    /// <summary>
    /// Implicitly convert to array of byte by returning the internal array
    /// </summary>
    /// <param name="pab">The PooledByteBuffer to implicitly convert.</param>
    public static implicit operator T[](PooledBuffer<T> pab) => pab.bytes;
}
