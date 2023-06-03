using System;
using System.Text;
using Melville.INPC;
using Melville.Parsing.CountingReaders;

namespace Melville.Postscript.Interpreter.Values;

internal interface IPostscriptValueComparison
{
    public bool Equals(in Int128 memento, object otherStrategy, in Int128 otherMemento);
}

internal enum StringKind
{
    String,
    Name,
    LiteralName
};

internal abstract partial class PostscriptString : 
    IPostscriptValueStrategy<string>, IPostscriptValueStrategy<StringKind>, IByteStringSource,
    IPostscriptValueComparison
{
    [FromConstructor] private readonly StringKind stringKind;
    public string GetValue(in Int128 memento) => 
        WrapString(Encoding.ASCII.GetString(
            GetBytes(in memento, stackalloc byte[IByteStringSource.ShortStringLimit])));

    private string WrapString(string stringVal) => stringKind switch
    {
        StringKind.String => $"({stringVal})",
        StringKind.Name => stringVal,
        StringKind.LiteralName => "/"+stringVal,
        _ => throw new ArgumentOutOfRangeException()
    };
    
    StringKind IPostscriptValueStrategy<StringKind>.GetValue(in Int128 memento) => stringKind;

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

internal sealed partial class PostscriptShortString : PostscriptString
{
    private PostscriptShortString(StringKind stringKind) : base(stringKind)
    {
    }

    private static PostscriptShortString StringInstance = new(StringKind.String);
    private static PostscriptShortString NameInstance = new(StringKind.Name);
    private static PostscriptShortString LiteralNameInstance = new(StringKind.LiteralName);

    public static PostscriptShortString InstanceForKind(StringKind kind) => kind switch
    {
        StringKind.String => StringInstance,
        StringKind.Name => NameInstance,
        StringKind.LiteralName => LiteralNameInstance,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    public override ReadOnlySpan<byte> GetBytes(in Int128 memento, in Span<byte> scratch)
    {
        Int128 remainingChars = memento;
        int i;
        for (i = 0; i < scratch.Length && remainingChars != 0; i++)
        {
            scratch[i] = SevenBitStringEncoding.GetNextCharacter(ref remainingChars);
        }
        return scratch[..i];
    }
}

internal sealed partial class PostscriptLongString: PostscriptString
{
    [FromConstructor]private readonly Memory<byte> value;

    public override ReadOnlySpan<byte> GetBytes(in Int128 memento, in Span<byte> scratch) =>
        value.Span;

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.AddBytes(value.Span);
        return hc.GetHashCode();
    }
}