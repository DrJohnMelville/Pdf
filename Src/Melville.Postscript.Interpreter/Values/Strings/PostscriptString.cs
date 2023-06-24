using System;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Melville.Postscript.Interpreter.Values.Strings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Values;

internal interface IPostscriptTokenSource
{
    void GetToken(OperandStack stack);
}

internal abstract partial class PostscriptString : 
    IPostscriptValueStrategy<string>, 
    IPostscriptValueStrategy<StringKind>, 
    IPostscriptValueComparison,
    IPostscriptValueStrategy<IExecutionSelector>,
    IPostscriptValueStrategy<Memory<byte>>,
    IPostscriptValueStrategy<long>,
    IPostscriptValueStrategy<double>,
    IPostscriptValueStrategy<PostscriptLongString>,
    IPostscriptValueStrategy<StringSpanSource>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptValueStrategy<IPostscriptArray>,
    IPostscriptValueStrategy<IPostscriptTokenSource>
{
    [FromConstructor] protected  StringKind StringKind { get; }
    public string GetValue(in Int128 memento) => 
        RenderStringValue(memento);

    private string RenderStringValue(Int128 memento) =>
        Encoding.ASCII.GetString(
            GetBytes(in memento, stackalloc byte[ShortStringLimit]));

    StringKind IPostscriptValueStrategy<StringKind>.GetValue(in Int128 memento) => 
        StringKind;
        
    IExecutionSelector IPostscriptValueStrategy<IExecutionSelector>.GetValue(
        in Int128 memento) => StringKind.ExecutionSelector;

    StringSpanSource IPostscriptValueStrategy<StringSpanSource>.GetValue(
        in Int128 memento) => new(this, memento);

    internal abstract Span<byte> GetBytes(scoped in Int128 memento, scoped in Span<byte> scratch);
   
    public virtual bool Equals(in Int128 memento, object otherStrategy, in Int128 otherMemento)
    {
        if (otherStrategy is not PostscriptString otherASPss) return false;
        var myBits = GetBytes(in memento, stackalloc byte[ShortStringLimit]);
        var otherBits= otherASPss.GetBytes(in otherMemento, 
            stackalloc byte[ShortStringLimit]);
        return myBits.SequenceEqual(otherBits);
    }

    Memory<byte> IPostscriptValueStrategy<Memory<byte>>.GetValue(in Int128 memento) =>
        ValueAsMemory(memento);

    protected virtual Memory<byte> ValueAsMemory(in Int128 memento) =>
        GetBytes(in memento, stackalloc byte[ShortStringLimit])
            .ToArray();

    long IPostscriptValueStrategy<long>.GetValue(in Int128 memento) =>
        ParseAsNumber(memento).Get<long>();

    double IPostscriptValueStrategy<double>.GetValue(in Int128 memento) =>
        ParseAsNumber(memento).Get<double>();
    
    public PostscriptValue ParseAsNumber(in Int128 memento) =>
        NumberTokenizer.TryDetectNumber(
            GetBytes(in memento, stackalloc byte[ShortStringLimit]), 
            out var result)
            ? result
            : throw new PostscriptException($"Could not convert {GetValue(memento)} into a number");

    /// <summary>
    /// The longest string which can be packed into an PostscriptValue.  Strings longer than this
    /// will be stored on the heap and the heap buffer returned from GetBytes.
    /// </summary>
    public const int ShortStringLimit = 18;

    PostscriptLongString IPostscriptValueStrategy<PostscriptLongString>.GetValue(in Int128 memento) =>
        AsLongString(memento);
        
    protected PostscriptLongString AsLongString(in Int128 memento) =>
        this as PostscriptLongString ?? 
        new PostscriptLongString(StringKind, ValueAsMemory(memento));

    IPostscriptComposite IPostscriptValueStrategy<IPostscriptComposite>.GetValue(in Int128 memento)
        => AsLongString(memento);

    IPostscriptArray IPostscriptValueStrategy<IPostscriptArray>.GetValue(
        in Int128 memento) => AsLongString(memento);

    IPostscriptTokenSource IPostscriptValueStrategy<IPostscriptTokenSource>.GetValue(in Int128 memento) =>
        AsLongString(memento);
}