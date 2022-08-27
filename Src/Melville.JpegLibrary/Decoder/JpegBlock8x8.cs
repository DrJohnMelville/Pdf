using System.Runtime.CompilerServices;

namespace Melville.JpegLibrary.Decoder;

public unsafe struct JpegBlock8x8
{
    private fixed short _data[64];

    internal void CopyTo(ref JpegBlock8x8F block)
    {
        ref short srcRef = ref _data[0];
        ref float destRef = ref Unsafe.As<JpegBlock8x8F, float>(ref block);
        for (int i = 0; i < 64; i++)
        {
            Unsafe.Add(ref destRef, i) = Unsafe.Add(ref srcRef, i);
        }
    }

    internal void LoadFrom(ref JpegBlock8x8F block)
    {
        ref short destRef = ref _data[0];
        ref float srcRef = ref Unsafe.As<JpegBlock8x8F, float>(ref block);
        for (int i = 0; i < 64; i++)
        {
            Unsafe.Add(ref destRef, i) = (short)Unsafe.Add(ref srcRef, i);
        }
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The index of the element.</param>
    /// <returns>The element value.</returns>
    public short this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= 64)
            {
                ThrowArgumentOutOfRangeException(nameof(index));
            }
            ref short selfRef = ref Unsafe.As<JpegBlock8x8, short>(ref this);
            return Unsafe.Add(ref selfRef, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if ((uint)index >= 64)
            {
                ThrowArgumentOutOfRangeException(nameof(index));
            }
            ref short selfRef = ref Unsafe.As<JpegBlock8x8, short>(ref this);
            Unsafe.Add(ref selfRef, index) = value;
        }
    }

    /// <summary>
    /// Gets or sets the element at the specified position.
    /// </summary>
    /// <param name="x">The row index of the block.</param>
    /// <param name="y">The column index of the block.</param>
    /// <returns>The element value.</returns>
    public short this[int x, int y]
    {
        get => this[(y * 8) + x];
        set => this[(y * 8) + x] = value;
    }

    private static void ThrowArgumentOutOfRangeException(string paramName)
    {
        throw new ArgumentOutOfRangeException(paramName);
    }
}