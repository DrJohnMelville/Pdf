using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Markup;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Numbers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Model.Objects2;

/// <summary>
/// this defines a Pdf Object that is not an indirect reference
/// </summary>
public readonly partial struct PdfDirectValue
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

    public static PdfDirectValue CreateName(in ReadOnlySpan<byte> name) => 
        CreateStringOrName(name, StringKind.LiteralName);

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
        new(new PdfValueArray(values), default);
}
