using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    
    public bool IsBool => valueStrategy == PostscriptBoolean.Instance;
    public bool IsNull => valueStrategy is null || valueStrategy == PostscriptNull.Instance;
    public bool IsInteger => valueStrategy == PostscriptInteger.Instance;
    public bool IsDouble => valueStrategy == PostscriptDouble.Instance;
    public bool IsNumber => IsInteger || IsDouble;
    public bool IsString =>
        valueStrategy is PostscriptString ps && ps.StringKind == StringKind.String;
    public bool IsName =>
        valueStrategy is PostscriptString ps && ps.StringKind == StringKind.LiteralName;

    public bool NeedsLeadingSpace => IsNumber || IsBool || IsNull;

    #endregion

    #region Value Accessors

    public bool TryGet<T>([NotNullWhen(true)] out T? value) => NonNullValueStrategy() switch
    {
        T valAsT => valAsT.AsTrueValue(out value),
        IPostscriptValueStrategy<T> prov => prov.GetValue(memento).AsTrueValue(out value),
        _=> default(T).AsFalseValue(out value)
    };

    public T Get<T>() => TryGet(out T? value)
        ? value
        : throw new PdfParseException($"Value {this} is not of type {typeof(T)}");

    public override string ToString() => NonNullValueStrategy() switch
    {
        IPostscriptValueStrategy<string> vs => vs.GetValue(memento),
        _=> valueStrategy.ToString()
    };


    #endregion

    #region Operators and Factories

    public static implicit operator PdfDirectObject(bool value) =>
        new(PostscriptBoolean.Instance, MementoUnion.CreateFrom(value));
    public static implicit operator PdfDirectObject(int value) =>
        new(PostscriptInteger.Instance, MementoUnion.CreateFrom((long)value));
    public static implicit operator PdfDirectObject(long value) =>
        new(PostscriptInteger.Instance, MementoUnion.CreateFrom(value));
    public static implicit operator PdfDirectObject(double value) =>
        new(PostscriptDouble.Instance, MementoUnion.CreateFrom(value));
    public static implicit operator PdfDirectObject(PdfArray array) =>
        new(array, default);
    public static implicit operator PdfDirectObject(PdfDictionary array) =>
        new(array, default);

    public static implicit operator PdfDirectObject(string value)
    {
        Span<byte> intemediate = stackalloc byte[value.Length];
        ExtendedAsciiEncoding.EncodeToSpan(value, intemediate);
        return (PdfDirectObject)(ReadOnlySpan<byte>)intemediate;
    }

    public static implicit operator PdfIndirectObject(PdfDirectObject value) =>
        new(value.valueStrategy, value.memento);

    public static implicit operator PdfDirectObject(in ReadOnlySpan<byte> value) => value switch
    {
        [(byte)'/', .. var name] => CreateName(name),
        var str => CreateString(str)
    };

    public static PdfDirectObject CreateName(string name)
    {
        var len = Encoding.UTF8.GetByteCount(name);
        Span<byte> data = stackalloc byte[len];
        Encoding.UTF8.GetBytes(name.AsSpan(), data);
        return CreateName(data);
    }

    public static PdfDirectObject CreateLongName(byte[] value)
    {
        Debug.Assert(value.Length > 18);
        var (strategy, memento) = StringEncoder.CreateLongString(StringKind.LiteralName, value);
        return new PdfDirectObject(strategy, memento);
    }
     public static PdfDirectObject CreateName(ulong low, ulong high)
    {
        var (strategy, memento) = StringEncoder.FromULongs(StringKind.LiteralName, low, high);
        return new PdfDirectObject(strategy, memento);
    }

    public static PdfDirectObject CreateName(in ReadOnlySpan<byte> name) => 
        CreateStringOrName(name, StringKind.LiteralName);

    public static readonly PdfDirectObject EmptyString = CreateString(ReadOnlySpan<byte>.Empty);

    public static PdfDirectObject CreateUtf8String(string source) =>
        CreateString(source, UnicodeEncoder.Utf8, StringKind.String);
    public static PdfDirectObject CreateUtf16String(string source) =>
        CreateString(source, UnicodeEncoder.BigEndian, StringKind.String);
    public static PdfDirectObject CreateUtf16LittleEndianString(string source) =>
        CreateString(source, UnicodeEncoder.LittleEndian, StringKind.String);

    private static PdfDirectObject CreateString(
        string source, UnicodeEncoder encoder, StringKind stringKind)
    {
        Span<byte> text = stackalloc byte[encoder.EncodedLength(source)];
        encoder.FillEncodedSpan(source, text);
        return CreateStringOrName(text, stringKind);
    }

    public static PdfDirectObject CreateString(in ReadOnlySpan<byte> str) => 
        CreateStringOrName(str, StringKind.String);

    private static PdfDirectObject CreateStringOrName(in ReadOnlySpan<byte> str, StringKind kind)
    {
        var (strategy, memento) = str.PostscriptEncode(kind);
        return new(strategy, memento);
    }

    public static PdfDirectObject CreateNull() => new(PostscriptNull.Instance, default);
    #endregion

    public static PdfDirectObject FromArray(params PdfIndirectObject[] values) => 
        new PdfArray(values);

    /// <inheritdoc />
    public bool Equals(PdfDirectObject other) => 
        ShallowEquals(other) || DeepEquals(other);


    private bool ShallowEquals(in PdfDirectObject other) => 
        Equals(valueStrategy, other.valueStrategy) && memento.Equals(other.memento);

    private bool DeepEquals(in PdfDirectObject other) =>
        valueStrategy is IPostscriptValueEqualityTest pvet && 
        pvet.Equals(memento, other.valueStrategy, other.memento);

    public int CompareTo(PdfDirectObject other)
    {
        return Get<IPostscriptValueComparison>().CompareTo(
            memento, other.NonNullValueStrategy(), other.memento);
    }

    public override bool Equals(object? obj) => 
        obj is PdfDirectObject other && Equals(other);

    public override int GetHashCode() => 
        HashCode.Combine(valueStrategy, memento);
}