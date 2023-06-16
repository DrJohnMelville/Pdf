using System;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Strings;

namespace Melville.Postscript.Interpreter.Values;

internal abstract partial class PostscriptString : 
    IPostscriptValueStrategy<string>, 
    IPostscriptValueStrategy<StringKind>, 
    IByteStringSource,
    IPostscriptValueComparison,
    IPostscriptValueStrategy<IExecutionSelector>
{
    [FromConstructor] private readonly StringKind stringKind;
    public string GetValue(in Int128 memento) => 
        RenderStringValue(memento);

    private string RenderStringValue(Int128 memento) =>
        Encoding.ASCII.GetString(
            GetBytes(in memento, stackalloc byte[IByteStringSource.ShortStringLimit]));

    StringKind IPostscriptValueStrategy<StringKind>.GetValue(in Int128 memento) => stringKind;
        
    IExecutionSelector IPostscriptValueStrategy<IExecutionSelector>.GetValue(
        in Int128 memento) => stringKind.ExecutionSelector;

    public abstract ReadOnlySpan<byte> GetBytes(in Int128 memento, in Span<byte> scratch);
   
    public virtual bool Equals(in Int128 memento, object otherStrategy, in Int128 otherMemento)
    {
        if (otherStrategy is not PostscriptString otherASPss) return false;
        var myBits = GetBytes(in memento, stackalloc byte[IByteStringSource.ShortStringLimit]);
        var otherBits= otherASPss.GetBytes(in otherMemento, 
            stackalloc byte[IByteStringSource.ShortStringLimit]);
        return myBits.SequenceEqual(otherBits);
    }
}