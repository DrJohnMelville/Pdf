using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// This is a 128 bit buffer.  You can interpret this as Int128, UInt128, a
/// span or 2 uint64, int64, or doubles, or four  uint32,int2, or singles,
/// or 16 booleans.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public partial struct MementoUnion: IEquatable<MementoUnion>
{
    [MacroCode("""
        /// <summary>
        /// The value as an ~0~
        /// </summary>
        [FieldOffset(0)] public readonly ~0~ ~0~;
        /// <summary>
        /// Create a MementoUnion from a ~0~;
        /// </summary>
        /// <param name="value">The value to set the memento to</param>
        public MementoUnion(~0~ value) => ~0~ = value;
        """)]
    [MacroItem("Int128")]
    [MacroItem("UInt128")]
    partial void Items128Bit();



    [MacroItem("UInt64","UInt64")]
    [MacroItem("Int64","Int64")]
    [MacroItem("Double","Double")]
    [MacroItem("UInt32","UInt32")]
    [MacroItem("Int32","Int32")]
    [MacroItem("Single","Single")]
    [MacroItem("Bool","bool")]
    [MacroItem("Byte","byte")]
    [MacroCode("""
        /// <summary>
        /// The value as ~0~s
        /// </summary>
        public ReadOnlySpan<~1~> ~0~s => As<~1~>();
        """)]
    private Span<T> As<T>() where T: struct =>
        MemoryMarshal.Cast<MementoUnion, T>(
            MemoryMarshal.CreateSpan(ref this, 1));

    /// <summary>
    /// Create a memento from two values of the same type.
    /// </summary>
    /// <typeparam name="T">The element type of the memento</typeparam>
    /// <param name="i0">The first parameter</param>
    /// <returns>A memento with the given parameters</returns>
    public static MementoUnion CreateFrom<T>(T i0) where T : struct
    {
        var ret = new MementoUnion();
        var span = ret.As<T>();
        span[0] = i0;
        return ret;
    }

    /// <summary>
    /// Create a memento from two values of the same type.
    /// </summary>
    /// <typeparam name="T">The element type of the memento</typeparam>
    /// <param name="i0">The first parameter</param>
    /// <param name="i1">The second parameter</param>
    /// <returns>A memento with the given parameters</returns>
    public static MementoUnion CreateFrom<T>(T i0, T i1) where T : struct
    {
        var ret = new MementoUnion();
        var span = ret.As<T>();
        span[0] = i0;
        span[1] = i1;
        return ret;
    }

    /// <inheritdoc />
    public bool Equals(MementoUnion other) => UInt128.Equals(other.UInt128);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MementoUnion other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => UInt128.GetHashCode();

    /// <summary>
    /// Implements the == operator 
    /// </summary>
    /// <param name="a">the first value</param>
    /// <param name="b">The second value</param>
    /// <returns>True if the two values are value equal, false otherwise</returns>
    public static bool operator ==(MementoUnion a, MementoUnion b) =>
        a.Equals(b);

    /// <summary>
    /// Implements the != operator 
    /// </summary>
    /// <param name="a">the first value</param>
    /// <param name="b">The second value</param>
    /// <returns>false if the two values are value equal, true otherwise</returns>
    public static bool operator !=(MementoUnion a, MementoUnion b) =>
        !a.Equals(b);

    /// <summary>
    /// Create a memento from two ints and a long.
    /// </summary>
    /// <param name="number">The first int</param>
    /// <param name="generation">The second int</param>
    /// <param name="offset">The long</param>
    /// <returns>A memento union with two ints in Ints[0] and Ints[1] and a
    /// value in Long Long[1].</returns>
    public static MementoUnion CreateFrom(int number, int generation, long offset)
    {
        var ret = new MementoUnion();
        ret.As<int>()[0] = number;
        ret.As<int>()[1] = generation;
        ret.As<long>()[1] = offset;
        return ret;
    }

    public static MementoUnion CreateNameWithLength(in ReadOnlySpan<byte> source)
    {
        var sourceLength = source.Length;
        Debug.Assert(sourceLength is >= 0 and < 16);
        var ret = new MementoUnion();
        var bytes = ret.As<byte>();
        bytes[0] = (byte)sourceLength;
        source.CopyTo(bytes[1..]);
        return ret;
    }

    public static MementoUnion CreateFromBytes(in ReadOnlySpan<byte> data)
    {
        var ret = new MementoUnion();
        data.CopyTo(ret.As<byte>());
        return ret;
    }
}