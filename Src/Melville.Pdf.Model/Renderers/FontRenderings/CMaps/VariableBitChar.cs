using System;
using System.Diagnostics;
using System.Numerics;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal readonly partial struct VariableBitChar: IComparable<VariableBitChar>
{
    [FromConstructor]private readonly ulong bits;

    public VariableBitChar() => bits = 1;

    public VariableBitChar(ReadOnlySpan<byte> data) : this()
    {
        Debug.Assert(data.Length < 8);
        foreach (var nextByte in data)
        {
            bits <<= 8;
            bits |= nextByte;
        }
    }

    public int Length() => BitOperations.Log2(bits) / 8;

    public VariableBitChar AddByte(byte next) =>
        new ((bits <<8) | next);

    public int CompareTo(VariableBitChar other) => 
        bits.CompareTo(other.bits);

    public static bool operator <(in VariableBitChar lhs, in VariableBitChar rhs) =>
        lhs.CompareTo(rhs) < 0;
    public static bool operator >(in VariableBitChar lhs, in VariableBitChar rhs) =>
        lhs.CompareTo(rhs) > 0;

    public static bool operator <=(in VariableBitChar lhs, in VariableBitChar rhs) =>
        lhs.CompareTo(rhs) <= 0;
    public static bool operator >=(in VariableBitChar lhs, in VariableBitChar rhs) =>
        lhs.CompareTo(rhs) >= 0;
    public static ulong operator -(in VariableBitChar lhs, in VariableBitChar rhs) =>
        lhs.bits - rhs.bits;

    public static VariableBitChar operator +(VariableBitChar item, int delta) =>
        new VariableBitChar(item.bits + (ulong)delta);

    public override string ToString() => bits.ToString("X")[1..];
}