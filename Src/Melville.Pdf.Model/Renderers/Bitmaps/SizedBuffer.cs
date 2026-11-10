using System;
using System.Numerics;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public unsafe readonly struct SizedBuffer<T>
{
    private readonly T* basePtr;
    public int Length { get; }

    public SizedBuffer(T* basePtr, int size)
    {
        if (size < 0)
            throw new ArgumentException("Size cannot be less than 0");
        this.basePtr = basePtr;
        Length = size;
    }

    public SizedBuffer<T> Slice(int start)
    {
        if ((uint)start > Length)
            throw new InvalidOperationException("Start must be >= 0 and <= Length");
        return new SizedBuffer<T>(basePtr + start, Length - start);
    }
    public SizedBuffer<T> Slice(int start, int length) => Slice(start).Take(length);

    public SizedBuffer<T> Take(int length)
    {
        if ((uint)length > Length)
            throw new InvalidOperationException("new Length must be >= 0 and <= old length >Length");
        return new SizedBuffer<T>(basePtr, length);

    }

    public SizedBuffer<T> this[Range range]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(Length);
            // all range checking is done in the slice method.
            return Slice(offset, length);
        }
    }

    public Span<T> AsSpan() => new Span<T>(basePtr, Length);

    public static implicit operator Span<T>(SizedBuffer<T> src) => src.AsSpan();
}

[Obsolete("This class is unsafe.  Try not to use it")]
public unsafe struct UnsafeSizedBufferFactory<T>(T* pointer, int size): IDisposable
{
    public readonly SizedBuffer<T> Buffer { get; } = new SizedBuffer<T>(pointer, size);

    public void Dispose()
    {
        // we have nothing to dispose, but this is the pattern for the buffer factories.
    }
}
