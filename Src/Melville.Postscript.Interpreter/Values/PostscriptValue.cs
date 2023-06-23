using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Melville.Postscript.Interpreter.Values.Numbers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// This structure represents a single postscript value, most of which can be contained
/// internally, but some of which cannot.  The valueStrategy object allows for extensions like
/// dictionaries, files, and arrays.
/// </summary>
public readonly partial struct PostscriptValue : IEquatable<PostscriptValue>
{
    /// <summary>
    /// The strategy that can retrieve the value for this item.
    /// </summary>
    [FromConstructor] private readonly IPostscriptValueStrategy<string> valueStrategy;

    /// <summary>
    /// The strategy that defines how to execute this value
    /// </summary>
    [FromConstructor] public readonly IExecutePostscript ExecutionStrategy { get; }

    /// <summary>
    /// A 128 bit space that allows most values to be stored without a heap allocation.
    /// </summary>
    [FromConstructor] private readonly Int128 memento;

    /// <summary>
    /// True if this is a Postscript null object, false otherwise.
    /// </summary>
    public bool IsNull => valueStrategy is PostscriptNull;

    /// <summary>
    /// True if this is a Postscript mark object, false otherwise.
    /// </summary>
    public bool IsMark => valueStrategy is PostscriptMark;

    /// <summary>
    /// True if this is a number represented as an integer, false otherwise.
    /// </summary>
    public bool IsInteger => valueStrategy is PostscriptInteger;
    /// <summary>
    /// True if this is a number represented as an double, false otherwise.
    /// </summary>
    public bool IsDouble=> valueStrategy is PostscriptDouble;
    /// <summary>
    /// True if this is a number represented as an double, false otherwise.
    /// </summary>
    public bool IsNumber=> IsInteger || IsDouble;

    /// <summary>
    /// Gets a pdf value of a given type.
    /// </summary>
    /// <typeparam name="T">The type of result expected from this value</typeparam>
    /// <returns>The exoected value</returns>
    /// <exception cref="PostscriptInvalidTypeException">
    /// If the current value cannot be converted to the requested type.</exception>
    public T Get<T>() =>
        TryGet<T>(out var value) ? value:
        (valueStrategy as IPostscriptValueStrategy<T> ?? TypeError<T>()).GetValue(memento);

    private IPostscriptValueStrategy<T> TypeError<T>() =>
        throw new PostscriptInvalidTypeException(
            $"{valueStrategy.GetType()} does not implement IPostScriptValueStrategy<{typeof(T)}>");

    /// <summary>
    /// Try to extract a value of a given type from a PostscriptValue
    /// </summary>
    /// <typeparam name="T">The desired type</typeparam>
    /// <param name="value">Receives the converted value, if available</param>
    /// <returns>True if the value can be converted to the given type, false otherwise</returns>
    public bool TryGet<T>([NotNullWhen(true)] out T? value) =>
        valueStrategy switch
        {
            IPostscriptValueStrategy<T> ts =>
                ts.GetValue(memento).AsTrueValue(out value),
            T val => val.AsTrueValue(out value),
            _ => default(T).AsFalseValue(out value),
        };

    /// <inheritdoc />
    public bool Equals(PostscriptValue other)
    {
        if (ShallowEqual(other) || DeepEqual(other)) return true;
        return false;
    }


    private bool ShallowEqual(PostscriptValue other) =>
        valueStrategy == other.valueStrategy &&
        memento == other.memento;


    private bool DeepEqual(PostscriptValue other)
    {
        return valueStrategy is IPostscriptValueComparison psc &&
               psc.Equals(memento, other.valueStrategy, other.memento);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(valueStrategy, memento);

    /// <inheritdoc />
    public override string ToString() => 
        ExecutionStrategy.WrapTextDisplay(valueStrategy.GetValue(memento));

    [MacroItem("int")]
    [MacroItem("double")]
    [MacroItem("bool")]
    [MacroCode("""
        /// <summary>
        /// Create a PostscriptValue from a ~0~
        /// </summary>
        /// <param name="i">The value to wrap in a PostscriptValue</param>
        public static implicit operator PostscriptValue(~0~ i) => 
            PostscriptValueFactory.Create(i);
        """)]
    partial void MacroHolder();

    /// <summary>
    /// Create a PostscriptValue from a string
    /// </summary>
    /// <param name="s">The value to wrap in a PostscriptValue</param>
    public static implicit operator PostscriptValue(string s)
    {
        Span<byte> buffer = stackalloc byte[Encoding.ASCII.GetByteCount(s)];
        Encoding.ASCII.GetBytes(s, buffer);
        return (PostscriptValue)(ReadOnlySpan<byte>)buffer;
    }

    /// <summary>
    /// Create a PostscriptValue from a string
    /// </summary>
    /// <param name="s">The value to wrap in a PostscriptValue</param>
    public static implicit operator PostscriptValue(in ReadOnlySpan<byte> s) => s switch
    {
        [(byte)'/', .. var rest] =>
            PostscriptValueFactory.CreateString(rest, StringKind.LiteralName),
        [(byte)'(', .. var rest, (byte)')'] =>
            PostscriptValueFactory.CreateString(rest, StringKind.String),
        _ => PostscriptValueFactory.CreateString(s, StringKind.Name)
    };

    /// <summary>
    /// Returns this value as a literal -- as returned from cvlit
    /// </summary>
    /// <returns></returns>
    public PostscriptValue AsLiteral() =>
        new(valueStrategy, ExecutionSelector.Literal, memento);
    /// <summary>
    /// Returns this value as an executable value -- as returned by cvx
    /// </summary>
    /// <returns></returns>
    public PostscriptValue AsExecutable() =>
        new(valueStrategy, ExecutionSelector.Executable, memento);

    private IExecutionSelector ExecutionSelector =>
        TryGet(out IExecutionSelector? sel) ? sel : AlwaysLiteralSelector.Instance;

    internal PostscriptValue AsCopyableValue() =>
        valueStrategy == StringKind.String.ShortStringStraegy
            ? PostscriptValueFactory.CreateLongString(
                Get<Memory<byte>>(), StringKind.String)
            : this;
}