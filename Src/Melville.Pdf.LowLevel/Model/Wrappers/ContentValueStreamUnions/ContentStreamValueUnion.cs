using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

public readonly struct ContentStreamValueUnion
{
    /// <summary>
    /// The type of thevalue represented by this struct
    /// </summary>
    public ContentStreamValueType Type { get; }
    /// <summary>
    /// A CLR object represented by this item.
    /// </summary>
    public object? Object { get; }
    /// <summary>
    /// A floating point number represented by this item.
    /// </summary>
    public double Floating { get; }
    /// <summary>
    /// An integer value represented by this item.
    /// </summary>
    public long Integer { get; }
    /// <summary>
    /// A Memory of bytes represented by this item.
    /// </summary>
    public Memory<byte> Bytes { get; }

    /// <summary>
    /// Initialize this structure from an object
    /// </summary>
    /// <param name="obj">The object to store in the union structure</param>
    public ContentStreamValueUnion(object obj)
    {
        Type = ContentStreamValueType.Object;
        this.Object = obj;
        Floating = 0;
        Integer = 0;
        Bytes = Memory<byte>.Empty;
    }
    /// <summary>
    /// Initialize this struct from a Memory of bytes
    /// </summary>
    /// <param name="memory"></param>
    public ContentStreamValueUnion(in Memory<byte> memory)
    {
        Type = ContentStreamValueType.Memory;
        Object = null;
        Floating = 0;
        Integer = 0;
        Bytes = memory;
    }
    /// <summary>
    /// Initialize this struct with a numeric value.
    /// </summary>
    /// <param name="floating">A double value to store in the struct</param>
    /// <param name="longValue">A long value to store in the struct.</param>
    public ContentStreamValueUnion(double floating, long longValue)
    {
        Type = ContentStreamValueType.Number;
        Object = null;
        Floating = floating;
        Integer = longValue;
        Bytes = Memory<byte>.Empty;
    }
}