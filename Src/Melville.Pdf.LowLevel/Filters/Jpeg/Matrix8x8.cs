using System;
using System.Diagnostics;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public readonly partial struct Matrix8x8<T>
{
    [FromConstructor] private readonly T[] data;

    public Matrix8x8() : this(new T[64])
    {
    }

    partial void OnConstructed()
    {
        Debug.Assert(data.Length == 64);
    }
    public ref T this[int row, int column] => ref data[MapArray(row, column)];
    public ref T this[int offset] => ref data[offset];
    private static int MapArray(int row, int column) => (8 * row) + column;
}