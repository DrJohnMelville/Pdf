using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Fonts.Type1TextParsers;

internal partial class EexecDecryptingByteSource : 
    BuiltInFunction, IByteSourceWithGlobalPosition
{
    private readonly IMultiplexSource multiplexSource;
    [DelegateTo] IByteSourceWithGlobalPosition source;

    public EexecDecryptingByteSource(IMultiplexSource multiplexSource)
    {
        this.multiplexSource = multiplexSource;
        source = new ByteSourceWithGlobalPosition(multiplexSource.ReadPipeFrom(0), 0);
    }

    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        engine.OperandStack.Pop();
        source = new ByteSourceWithGlobalPosition(
            PipeReader.Create(DecodedStream()), source.Position);
    }

    private const ushort eexecDecodeKey = 55665;
    private ExecDecodeStream DecodedStream() => 
        new(multiplexSource.ReadFrom(source.Position), eexecDecodeKey);
}