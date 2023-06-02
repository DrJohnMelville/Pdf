using System;
using System.Text;
using Melville.INPC;
using Melville.Parsing.CountingReaders;

namespace Melville.Postscript.Interpreter.Values;

internal enum StringKind
{
    String,
    Name,
    LiteralName
};

internal abstract partial class PostscriptString : 
    IPostscriptValueStrategy<string>, IPostscriptValueStrategy<StringKind>, IByteStringSource
{
    [FromConstructor] private readonly StringKind stringKind;
    public string GetValue(in Int128 memento) => 
        Encoding.ASCII.GetString(
            GetBytes(in memento, stackalloc byte[IByteStringSource.ShortStringLimit]));

    StringKind IPostscriptValueStrategy<StringKind>.GetValue(in Int128 memento) => stringKind;

    public abstract ReadOnlySpan<byte> GetBytes(in Int128 memento, in Span<byte> scratch);
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
    [FromConstructor]private readonly byte[] value;

    public override ReadOnlySpan<byte> GetBytes(in Int128 memento, in Span<byte> scratch) =>
        value.AsSpan();
}