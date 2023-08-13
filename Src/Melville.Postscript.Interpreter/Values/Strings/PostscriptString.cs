using System;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// A struct for getting a hashcode out of a string
/// </summary>
public readonly partial struct PostscriptHashCode
{
    /// <summary>
    /// The hash code returned
    /// </summary>
    [FromConstructor] public readonly int HashCode;
}

/// <summary>
/// Represents a string, literal name, or executable name of various leegths
/// </summary>
public abstract partial class PostscriptString : 
    IPostscriptValueStrategy<string>, 
    IPostscriptValueStrategy<StringKind>, 
    IPostscriptValueEqualityTest,
    IPostscriptValueComparison,
    IPostscriptValueStrategy<IExecutionSelector>,
    IPostscriptValueStrategy<Memory<byte>>,
    IPostscriptValueStrategy<long>,
    IPostscriptValueStrategy<double>,
    IPostscriptValueStrategy<PostscriptLongString>,
    IPostscriptValueStrategy<StringSpanSource>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptValueStrategy<IPostscriptArray>,
    IPostscriptValueStrategy<IPostscriptTokenSource>,
    IPostscriptValueStrategy<RentedMemorySource>,
    IPostscriptValueStrategy<PostscriptHashCode>
{
    /// <summary>
    /// Specifies whether this is a string, a name, or a literal name
    /// </summary>
    [FromConstructor] public StringKind StringKind { get; }

    string IPostscriptValueStrategy<string>.GetValue(in MementoUnion memento) => 
        RenderStringValue(memento);

    private string RenderStringValue(MementoUnion memento) =>
        Encoding.UTF8.GetString(
            GetBytes(in memento, stackalloc byte[ShortStringLimit]));

    StringKind IPostscriptValueStrategy<StringKind>.GetValue(in MementoUnion memento) => 
        StringKind;
        
    IExecutionSelector IPostscriptValueStrategy<IExecutionSelector>.GetValue(
        in MementoUnion memento) => StringKind.ExecutionSelector;

    StringSpanSource IPostscriptValueStrategy<StringSpanSource>.GetValue(
        in MementoUnion memento) => ToStringSpanSource(memento);

    private StringSpanSource ToStringSpanSource(MementoUnion memento) => new(this, memento);

    internal abstract Span<byte> GetBytes(scoped in MementoUnion memento, scoped in Span<byte> scratch);

    /// <inheritdoc />
    public virtual bool Equals(in MementoUnion memento, object otherStrategy, in MementoUnion otherMemento)
    {
        if (otherStrategy is not PostscriptString otherASPss) return false;
        var source1 = ToStringSpanSource(memento);
        var myBits = source1.GetSpan();
        var source2 = otherASPss.ToStringSpanSource(otherMemento);
        var otherBits= source2.GetSpan();
        return myBits.SequenceEqual(otherBits);
    }

    Memory<byte> IPostscriptValueStrategy<Memory<byte>>.GetValue(in MementoUnion memento) =>
        ValueAsMemory(memento);

    long IPostscriptValueStrategy<long>.GetValue(in MementoUnion memento) =>
        ParseAsNumber(memento).Get<long>();

    double IPostscriptValueStrategy<double>.GetValue(in MementoUnion memento) =>
        ParseAsNumber(memento).Get<double>();
    
    /// <summary>
    /// Parse a string using the number pars   
    /// </summary>
    /// <param name="memento">The memento for the string object</param>
    /// <returns>A postscript value of the numeric value of the string.</returns>
    /// <exception cref="PostscriptNamedErrorException"></exception>
    public PostscriptValue ParseAsNumber(in MementoUnion memento) =>
        NumberTokenizer.TryDetectNumber(
            GetBytes(in memento, stackalloc byte[ShortStringLimit]), 
            out var result)
            ? result
            : throw new PostscriptNamedErrorException(
                $"Could not convert string into a number", "typecheck");

    /// <summary>
    /// The longest string which can be packed into an PostscriptValue.  Strings longer than this
    /// will be stored on the heap
    /// </summary>
    public const int ShortStringLimit = 21;

    PostscriptLongString IPostscriptValueStrategy<PostscriptLongString>.GetValue(in MementoUnion memento) =>
        AsLongString(memento);
        

    IPostscriptComposite IPostscriptValueStrategy<IPostscriptComposite>.GetValue(in MementoUnion memento)
        => AsLongString(memento);

    IPostscriptArray IPostscriptValueStrategy<IPostscriptArray>.GetValue(
        in MementoUnion memento) => AsLongString(memento);

    IPostscriptTokenSource IPostscriptValueStrategy<IPostscriptTokenSource>.GetValue(in MementoUnion memento) =>
        AsLongString(memento);

    RentedMemorySource IPostscriptValueStrategy<RentedMemorySource>.GetValue(in MementoUnion memento) =>
        InnerRentedMemorySource(memento);

    private protected abstract PostscriptLongString AsLongString(in MementoUnion memento);
    private protected abstract RentedMemorySource InnerRentedMemorySource(MementoUnion memento);
    private protected abstract Memory<byte> ValueAsMemory(in MementoUnion memento);

    /// <inheritdoc />
    public int CompareTo(in MementoUnion memento, object otherStrategy, in MementoUnion otherMemento)
    {
        if (otherStrategy is not PostscriptString otherString)
            throw new PostscriptNamedErrorException("Can only compare strings to strings", "typecheck");
        var mySource = ToStringSpanSource(memento);
        var otherSource = otherString.ToStringSpanSource(otherMemento);
        return mySource.GetSpan().SequenceCompareTo(otherSource.GetSpan());

    }

    PostscriptHashCode IPostscriptValueStrategy<PostscriptHashCode>.GetValue(
        in MementoUnion memento)
    {
        var hc = new HashCode();
        hc.AddBytes(new StringSpanSource(this, memento).GetSpan());
        return new(hc.ToHashCode());
    }
}