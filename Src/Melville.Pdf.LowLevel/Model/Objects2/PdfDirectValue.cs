using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers2;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Numbers;

namespace Melville.Pdf.LowLevel.Model.Objects2;

/// <summary>
/// this defines a Pdf Object that is not an indirect reference
/// </summary>
public readonly partial struct PdfDirectValue: IEquatable<PdfDirectValue>, 
    IComparable<PdfDirectValue>
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
        if (valueStrategy is IIndirectValueSource)
            throw new PdfParseException("An indirect value source may not be the strategy for a PdfDirectValue");
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

    public static implicit operator PdfDirectValue(bool value) =>
        new(PostscriptBoolean.Instance, MementoUnion.CreateFrom(value));
    public static implicit operator PdfDirectValue(int value) =>
        new(PostscriptInteger.Instance, MementoUnion.CreateFrom((long)value));
    public static implicit operator PdfDirectValue(long value) =>
        new(PostscriptInteger.Instance, MementoUnion.CreateFrom(value));
    public static implicit operator PdfDirectValue(double value) =>
        new(PostscriptDouble.Instance, MementoUnion.CreateFrom(value));
    public static implicit operator PdfDirectValue(PdfValueArray array) =>
        new(array, default);
    public static implicit operator PdfDirectValue(PdfValueDictionary array) =>
        new(array, default);

    public static implicit operator PdfDirectValue(string value)
    {
        Span<byte> intemediate = stackalloc byte[value.Length];
        ExtendedAsciiEncoding.EncodeToSpan(value, intemediate);
        return (PdfDirectValue)(ReadOnlySpan<byte>)intemediate;
    }

    public static implicit operator PdfIndirectValue(PdfDirectValue value) =>
        new(value.valueStrategy, value.memento);

    public static implicit operator PdfDirectValue(in ReadOnlySpan<byte> value) => value switch
    {
        [(byte)'/', .. var name] => CreateName(name),
        var str => CreateString(str)
    };

    public static PdfDirectValue CreateName(string name)
    {
        var len = Encoding.UTF8.GetByteCount(name);
        Span<byte> data = stackalloc byte[len];
        Encoding.UTF8.GetBytes(name.AsSpan(), data);
        return CreateName(data);
    }

    public static PdfDirectValue CreateName(in ReadOnlySpan<byte> name) => 
        CreateStringOrName(name, StringKind.LiteralName);

    public static readonly PdfDirectValue EmptyString = CreateString(ReadOnlySpan<byte>.Empty);


    public static PdfDirectValue CreateString(in ReadOnlySpan<byte> str) => 
        CreateStringOrName(str, StringKind.String);

    private static PdfDirectValue CreateStringOrName(in ReadOnlySpan<byte> str, StringKind kind)
    {
        var (strategy, memento) = str.PostscriptEncode(kind);
        return new(strategy, memento);
    }

    public static PdfDirectValue CreateNull() => new(PostscriptNull.Instance, default);
    #endregion

    public static PdfDirectValue FromArray(params PdfIndirectValue[] values) => 
        new PdfValueArray(values);

    /// <inheritdoc />
    public bool Equals(PdfDirectValue other) => 
        Equals(valueStrategy, other.valueStrategy) && memento.Equals(other.memento);

    public int CompareTo(PdfDirectValue other)
    {
        return Get<IPostscriptValueComparison>().CompareTo(
            memento, other.NonNullValueStrategy(), other.memento);
    }

    public override bool Equals(object? obj) => 
        obj is PdfDirectValue other && Equals(other);

    public override int GetHashCode() => 
        HashCode.Combine(valueStrategy, memento);

}

public interface ITemporaryConverter
{
    PdfObject TemporaryConvert();
}

public static class TemporaryObjectBridge
{
    public static PdfObject AsOldObject(in this PdfIndirectValue value)
    {
        if (value.TryGetEmbeddedDirectValue(out var dv))
            return dv.AsOldObject();
        else
            throw new NotImplementedException("cannot convert indirect objects");
    }

    public static PdfObject AsOldObject(in this PdfDirectValue value) => value switch
    {
        { IsInteger: true } => new PdfInteger(value.Get<long>()),
        { IsDouble: true } => new PdfDouble(value.Get<double>()),
        { IsBool: true } => value.Get<bool>() ? PdfBoolean.True : PdfBoolean.False,
        { IsNull: true } => PdfTokenValues.Null,
        { IsString: true } => new PdfString(value.Get<StringSpanSource>().GetSpan().ToArray()),
        { IsName: true } => NameDirectory.Get(value.Get<StringSpanSource>().GetSpan()),
        _ when value.TryGet(out ITemporaryConverter cvt) => cvt.TemporaryConvert(),
    };

    public static T AsOldObject<T>(in this PdfDirectValue value) where T : PdfObject =>
        (T)value.AsOldObject();

}