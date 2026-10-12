using Melville.Parsing.IStreamAdaptors;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.Type1TextParsers.EexecDecoding;

internal partial class EexecCommand(
    IAsyncReader source) :
    AsyncBuiltInFunction
{


    public override ValueTask ExecuteAsync(PostscriptEngine engine, in PostscriptValue value) =>
        InnerExecuteAsync(engine, value);
    public async  ValueTask InnerExecuteAsync(PostscriptEngine engine, PostscriptValue value)
    {
        engine.OperandStack.Pop();

        if (engine.TokenSource is null)
        {
            throw new InvalidOperationException("No code source found to read from.");
        }
        var oldCodeSource = engine.TokenSource.CodeSource;

        IAsyncReader newSource = await BuildNewCodeSourceAsync(oldCodeSource);

        var newCodeSource = new ParsingBuffer(newSource);
        await newCodeSource.AdvanceAsync(4).CA();

        engine.TokenSource.CodeSource = newCodeSource;
    }

    private async Task<IAsyncReader> BuildNewCodeSourceAsync(ParsingBuffer oldCodeSource)
    {
        await oldCodeSource.EnsureBytesAsync(6).CA();
        TrySkipOneLineBreak(oldCodeSource);
        IAsyncReader newSource = new PrefixingAsyncReader(source, oldCodeSource.Peek().ToArray());
        if (AllHex(oldCodeSource.Peek(4))) newSource = new HexDecodingReader(newSource);
        return new DecodingReader(newSource, eexecDecodeKey);
    }

    private static void TrySkipOneLineBreak(ParsingBuffer oldCodeSource)
    {
        if (oldCodeSource.Peek()[0] == (byte)'\r') oldCodeSource.Advance(1);
        if (oldCodeSource.Peek()[0] == (byte)'\n') oldCodeSource.Advance(1);
    }

    bool AllHex(ReadOnlySpan<byte> bytes) =>
        bytes.IndexOfAnyExcept(CharacterClassifier.HexDigits) < 0;


    private const ushort eexecDecodeKey = 55665;
}
