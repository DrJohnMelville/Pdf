using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime;
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
    [FromConstructor] public object ValueStrategy { get; }

    /// <summary>
    /// The strategy that defines how to execute this value
    /// </summary>
    [FromConstructor] public readonly IExecutePostscript ExecutionStrategy { get; }

    /// <summary>
    /// A 128 bit space that allows most values to be stored without a heap allocation.
    /// </summary>
    [FromConstructor] public MementoUnion Memento { get; }

    /// <summary>
    /// True if this is a Postscript null object, false otherwise.
    /// </summary>
    public bool IsNull => ValueStrategy is PostscriptNull;

    /// <summary>
    /// True if this is a Postscript mark object, false otherwise.
    /// </summary>
    public bool IsMark => ValueStrategy is PostscriptMark;

    /// <summary>
    /// True if this is a number represented as an integer, false otherwise.
    /// </summary>
    public bool IsInteger => ValueStrategy is PostscriptInteger;
    /// <summary>
    /// True if this is a number represented as an double, false otherwise.
    /// </summary>
    public bool IsDouble=> ValueStrategy is PostscriptDouble;
    
    /// <summary>
    /// True if this is a number represented as an boolean, false otherwise.
    /// </summary>
    public bool IsBoolean => ValueStrategy is PostscriptBoolean;
    /// <summary>
    /// True if this is a number represented as an double, false otherwise.
    /// </summary>
    public bool IsNumber=> IsInteger || IsDouble;

    /// <summary>
    /// Returns true if the contained item is a literal name
    /// </summary>
    public bool IsLiteralName => IsStringType(StringKind.LiteralName);

    /// <summary>
    /// Returns true if the contained item is a string
    /// </summary>
    public bool IsString => IsStringType(StringKind.String);

    private bool IsStringType(StringKind stringType) =>
        ValueStrategy is PostscriptString pss &&
        pss.StringKind == stringType;

    /// <summary>
    /// Gets a pdf value of a given type.
    /// </summary>
    /// <typeparam name="T">The type of result expected from this value</typeparam>
    /// <returns>The exoected value</returns>
    /// <exception cref="PostscriptNamedErrorException">
    /// If the current value cannot be converted to the requested type.</exception>
    public T Get<T>() =>
        TryGet<T>(out var value) ? value:
        (ValueStrategy as IPostscriptValueStrategy<T> ?? TypeError<T>()).GetValue(Memento);

    private IPostscriptValueStrategy<T> TypeError<T>() =>
        throw new PostscriptNamedErrorException(
            $"{ValueStrategy.GetType()} does not implement IPostScriptValueStrategy<{typeof(T)}>",
            "typecheck");

    /// <summary>
    /// Try to extract a value of a given type from a PostscriptValue
    /// </summary>
    /// <typeparam name="T">The desired type</typeparam>
    /// <param name="value">Receives the converted value, if available</param>
    /// <returns>True if the value can be converted to the given type, false otherwise</returns>
    public bool TryGet<T>([NotNullWhen(true)] out T? value) =>
        this is T thisAsT ? thisAsT.AsTrueValue(out value):
        ValueStrategy switch
        {
            IPostscriptValueStrategy<T> ts =>
                ts.GetValue(Memento).AsTrueValue(out value),
            T val => val.AsTrueValue(out value),
            _ => default(T).AsFalseValue(out value),
        };

    /// <inheritdoc />
    public bool Equals(PostscriptValue other) => ShallowEqual(other) || DeepEqual(other);


    private bool ShallowEqual(PostscriptValue other) =>
        ValueStrategy == other.ValueStrategy &&
        Memento == other.Memento;


    private bool DeepEqual(PostscriptValue other) =>
        ValueStrategy is IPostscriptValueEqualityTest psc &&
        psc.Equals(Memento, other.ValueStrategy, other.Memento);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(ValueStrategy, Memento);

    /// <inheritdoc />
    public override string ToString() => 
        ExecutionStrategy.WrapTextDisplay(
            (ValueStrategy as IPostscriptValueStrategy<string>)?.GetValue(Memento) ??
            ValueStrategy.ToString() ?? "<No String Value>");

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
        new(ValueStrategy, ExecutionSelector.Literal, Memento);
    /// <summary>
    /// Returns this value as an executable value -- as returned by cvx
    /// </summary>
    /// <returns></returns>
    public PostscriptValue AsExecutable() =>
        new(ValueStrategy, ExecutionSelector.Executable, Memento);

    private IExecutionSelector ExecutionSelector =>
        TryGet(out IExecutionSelector? sel) ? sel : AlwaysLiteralSelector.Instance;

    internal PostscriptValue AsCopyableValue() =>
        ValueStrategy == StringKind.String.ShortStringStraegy
            ? PostscriptValueFactory.CreateLongString(
                Get<Memory<byte>>(), StringKind.String)
            : this;
}