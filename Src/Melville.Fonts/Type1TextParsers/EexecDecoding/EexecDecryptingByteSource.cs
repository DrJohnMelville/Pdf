using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.Type1TextParsers.EexecDecoding;

internal partial class EexecDecryptingByteSource(
    IAsyncReader source) :
    AsyncBuiltInFunction
{


    public override ValueTask ExecuteAsync(PostscriptEngine engine, in PostscriptValue value) =>
        InnerExecuteAsync(engine, value);
    public async  ValueTask InnerExecuteAsync(PostscriptEngine engine, PostscriptValue value)
    {
        engine.OperandStack.Pop();
        //source = new EexeDecisionSource(source, multiplexSource,
        //    i => source = i, eexecDecodeKey);
        var oldCodeSource = engine.TokenSource.CodeSource;

        await oldCodeSource.EnsureBytesAsync(4).CA();
        var isHex = AllHex(oldCodeSource.Peek(4));

        IAsyncReader newSource = new PrefixingReader(source, oldCodeSource.Peek().ToArray());
        if (isHex) newSource = new HexDecodingReader(newSource);
        newSource = new DecodingReader(newSource, eexecDecodeKey);

        var newCodeSource = new ParsingBuffer(newSource);
        await newCodeSource.AdvanceAsync(4).CA();

        engine.TokenSource.CodeSource = newCodeSource;       
    }

    bool AllHex(ReadOnlySpan<byte> bytes) =>
        bytes.IndexOfAnyExcept(CharacterClassifier.HexDigits) < 0;


    private const ushort eexecDecodeKey = 55665;
}

public class HexDecodingReader(IAsyncReader inner) : IAsyncReader
{
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
#warning implement this
        throw new NotImplementedException("Need to implement hex decoding");
    }
}

public class PrefixingReader(IAsyncReader inner, byte[] prefix) : IAsyncReader
{
    private Memory<byte> prefix = prefix.AsMemory();

    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        if (prefix.Length > 0)
        {
            var len = Math.Min(prefix.Length, buffer.Length);
            prefix.Span[..len].CopyTo(buffer.Span);
            prefix = prefix[len..];
            return ValueTask.FromResult(len);
        }
        return inner.ReadAsync(buffer, cancellation);
    }
}

public class DecodingReader(IAsyncReader inner, ushort key) : IAsyncReader
{
    private ushort key = key;
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        int read = await inner.ReadAsync(buffer, cancellation).CA();
        DecodeType1Encoding.DecodeSegment(buffer.Span[..read], ref key);
        return read;
    }
}