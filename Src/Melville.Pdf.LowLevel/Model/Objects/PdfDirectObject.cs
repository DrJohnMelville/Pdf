using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Numbers;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// this defines a Pdf Object that is not an indirect reference
/// </summary>
public readonly partial struct PdfDirectObject: IEquatable<PdfDirectObject>, 
    IComparable<PdfDirectObject>
{
    /// <summary>
    /// Strategy object that defines the type of the PdfObject
    /// </summary>
    [FromConstructor] private readonly object? valueStrategy;
    
    /// <summary>
    /// A memento that allows most PdfObjects to be represented without allocations
    /// </summary>
    [FromConstructor] private readonly MementoUnion memento;
    private object NonNullValueStrategy() => (valueStrategy ?? PostscriptNull.Instance);


    partial void OnConstructed()
    {
        if (valueStrategy is IIndirectObjectSource)
            throw new PdfParseException("An indirect value source may not be the strategy for a PdfDirectObject");
    }

    #region TypeTesters

    
    /// <summary>
    /// True if the value holds a boolean, false otherwise;
    /// </summary>
    public bool IsBool => valueStrategy == PostscriptBoolean.Instance;

    /// <summary>
    /// True if the value holds a null value, false otherwise;
    /// </summary>
    public bool IsNull => valueStrategy is null || valueStrategy == PostscriptNull.Instance;

    /// <summary>
    /// True if the value holds an integer, false otherwise;
    /// </summary>
    public bool IsInteger => valueStrategy == PostscriptInteger.Instance;
    
    /// <summary>
    /// True if the value holds a double, false otherwise;
    /// </summary>
    public bool IsDouble => valueStrategy == PostscriptDouble.Instance;

    /// <summary>
    /// True if the value holds a integer or a double, false otherwise.
    /// Conversion between numeric types is implicit.
    /// </summary>
    public bool IsNumber => IsInteger || IsDouble;

    /// <summary>
    /// True if the value holds a string value, false otherwise;
    /// </summary>
    public bool IsString =>
        valueStrategy is PostscriptString ps && ps.StringKind == StringKind.String;
    
    /// <summary>
    /// True if the value holds a name, false otherwise;
    /// </summary>
    public bool IsName =>
        valueStrategy is PostscriptString ps && ps.StringKind == StringKind.LiteralName;

    /// <summary>
    /// True if the item requires a leading space when written as part of a dictionary
    /// </summary>
    public bool NeedsLeadingSpace => IsNumber || IsBool || IsNull;

    #endregion

    #region Value Accessors

    /// <summary>
    /// Attempt to convert the value to the given type.
    /// </summary>
    /// <typeparam name="T">The desired type</typeparam>
    /// <param name="value">Out variable that receives the output.</param>
    /// <returns>True if the conversion succeeds, false otherwise.</returns>
    public bool TryGet<T>([NotNullWhen(true)] out T? value) => NonNullValueStrategy() switch
    {
        T valAsT => valAsT.AsTrueValue(out value),
        IPostscriptValueStrategy<T> prov => prov.GetValue(memento).AsTrueValue(out value),
        _=> default(T).AsFalseValue(out value)
    };

    /// <summary>
    /// GEt the value as a given type and throw an exception if the conversion fails.
    /// </summary>
    /// <typeparam name="T">The desired type to convert to.</typeparam>
    /// <returns>the desired valule.</returns>
    /// <exception cref="PdfParseException">If the conversion fails.</exception>
    public T Get<T>() => TryGet(out T? value)
        ? value
        : throw new PdfParseException($"Value {this} is not of type {typeof(T)}");

    /// <summary>
    /// Printe the value to a string.  This is not necessarially efficient and is intended to
    /// facilitate debugging and testing
    /// </summary>
    /// <returns>The string representation of the object</returns>
    public override string ToString() => NonNullValueStrategy() switch
    {
        IPostscriptValueStrategy<string> vs => vs.GetValue(memento),
        _=> NonNullValueStrategy().ToString()??""
    };


    #endregion

    #region Operators and Factories
    /// <summary>
    /// Implictly convert a PdfDirectObject to a PdfIndirectObject
    /// </summary>
    public static implicit operator PdfIndirectObject(PdfDirectObject value) =>
        new(value.valueStrategy, value.memento);

    /// <summary>
    /// Create a Pdf object from a c# bool
    /// </summary>
    public static implicit operator PdfDirectObject(bool value) =>
        new(PostscriptBoolean.Instance, MementoUnion.CreateFrom(value));

    /// <summary>
    /// Create a Pdf object from a c# int
    /// </summary>
    public static implicit operator PdfDirectObject(int value) =>
        new(PostscriptInteger.Instance, MementoUnion.CreateFrom((long)value));
    
    /// <summary>
    /// Create a Pdf object from a c# long
    /// </summary>
    public static implicit operator PdfDirectObject(long value) =>
        new(PostscriptInteger.Instance, MementoUnion.CreateFrom(value));
    
    /// <summary>
    /// Create a Pdf object from a c# double
    /// </summary>
    public static implicit operator PdfDirectObject(double value) =>
        new(PostscriptDouble.Instance, MementoUnion.CreateFrom(value));
    
    /// <summary>
    /// Create a Pdf object from a PdfArray class
    /// </summary>
    public static implicit operator PdfDirectObject(PdfArray array) =>
        new(array, default);


    /// <summary>
    /// Create a Pdf object from a PdfDictionary class
    /// </summary>
    public static implicit operator PdfDirectObject(PdfDictionary array) =>
        new(array, default);

    /// <summary>
    /// Create a Pdf object from a c# string.  Creates a name if it starts with
    /// a / otherwise in creates a string.
    /// </summary>
    public static implicit operator PdfDirectObject(string value)
    {
        Span<byte> intemediate = stackalloc byte[value.Length];
        ExtendedAsciiEncoding.EncodeToSpan(value, intemediate);
        return (PdfDirectObject)(ReadOnlySpan<byte>)intemediate;
    }

    /// <summary>
    /// Create a Pdf object from a c# ReadOnlySpan&lt;T&gt;.  Creates a name if it starts with
    /// a / otherwise in creates a string.
    /// </summary>
    public static implicit operator PdfDirectObject(in ReadOnlySpan<byte> value) => value switch
    {
        [(byte)'/', .. var name] => CreateName(name),
        var str => CreateString(str)
    };

    /// <summary>
    /// Create a PdfName from a c# string
    /// </summary>
    /// <param name="name">The name to create.</param>
    /// <returns>The direct object representing the name</returns>
    public static PdfDirectObject CreateName(string name)
    {
        var len = Encoding.UTF8.GetByteCount(name);
        Span<byte> data = stackalloc byte[len];
        Encoding.UTF8.GetBytes(name.AsSpan(), data);
        return CreateName(data);
    }

    /// <summary>
    /// Create a PDF name from a low and high uint64 value.  This is used by the precomputed names
    /// </summary>
    /// <param name="low">The low 64 bits of the name buffer</param>
    /// <param name="high">The high 64 bits of the name buffer</param>
    /// <returns></returns>
    public static PdfDirectObject CreateName(ulong low, ulong high)
    {
        var (strategy, memento) = StringEncoder.FromULongs(StringKind.LiteralName, low, high);
        return new PdfDirectObject(strategy, memento);
    }

    /// <summary>
    /// Create a PdfName from a readonly span of bytes
    /// </summary>
    public static PdfDirectObject CreateName(in ReadOnlySpan<byte> name) => 
        CreateStringOrName(name, StringKind.LiteralName);

    /// <summary>
    /// An empty PDF string.
    /// </summary>
    public static readonly PdfDirectObject EmptyString = CreateString(ReadOnlySpan<byte>.Empty);

    /// <summary>
    /// Create a Pdf String using Utf-8 encoding.
    /// </summary>
    /// <param name="source">The string to create</param>
    public static PdfDirectObject CreateUtf8String(string source) =>
        CreateString(source, UnicodeEncoder.Utf8, StringKind.String);

    /// <summary>
    /// Create a Pdf String using Utf-16 Big Endian encoding.
    /// </summary>
    /// <param name="source">The string to create</param>
    public static PdfDirectObject CreateUtf16String(string source) =>
        CreateString(source, UnicodeEncoder.BigEndian, StringKind.String);

    /// <summary>
    /// Create a Pdf String using Utf-16 little endian encoding.
    /// </summary>
    /// <param name="source">The string to create</param>
    public static PdfDirectObject CreateUtf16LittleEndianString(string source) =>
        CreateString(source, UnicodeEncoder.LittleEndian, StringKind.String);

    private static PdfDirectObject CreateString(
        string source, UnicodeEncoder encoder, StringKind stringKind)
    {
        Span<byte> text = stackalloc byte[encoder.EncodedLength(source)];
        encoder.FillEncodedSpan(source, text);
        return CreateStringOrName(text, stringKind);
    }

    /// <summary>
    /// Create a PdfString from a ReadOnlySpan of bytes
    /// </summary>
    /// <param name="str">The text of the desired string.</param>
    public static PdfDirectObject CreateString(in ReadOnlySpan<byte> str) => 
        CreateStringOrName(str, StringKind.String);

    private static PdfDirectObject CreateStringOrName(in ReadOnlySpan<byte> str, StringKind kind)
    {
        var (strategy, memento) = str.PostscriptEncode(kind);
        return new(strategy, memento);
    }

    /// <summary>
    /// Create a null PdfObject
    /// </summary>
    public static PdfDirectObject CreateNull() => new(PostscriptNull.Instance, default);
    #endregion

    /// <summary>
    /// Create a PdfDirectObject from an array of PdfIndirectObjects
    /// </summary>
    /// <param name="values">The values to include in the array.</param>
    public static PdfDirectObject FromArray(params PdfIndirectObject[] values) => 
        new PdfArray(values);

    /// <inheritdoc />
    public bool Equals(PdfDirectObject other) => 
        ShallowEquals(other) || DeepEquals(other);

    private bool ShallowEquals(in PdfDirectObject other) => 
        Equals(valueStrategy, other.valueStrategy) && memento.Equals(other.memento);

    private bool DeepEquals(in PdfDirectObject other) =>
        other.valueStrategy is not null &&
        valueStrategy is IPostscriptValueEqualityTest pvet && 
        pvet.Equals(memento, other.valueStrategy, other.memento);

    /// <inheritdoc />
    public int CompareTo(PdfDirectObject other) =>
        Get<IPostscriptValueComparison>().CompareTo(
            memento, other.NonNullValueStrategy(), other.memento);

    /// <inheritdoc />
    public override bool Equals(object? obj) => 
        obj is PdfDirectObject other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => 
        HashCode.Combine(valueStrategy, memento);
}