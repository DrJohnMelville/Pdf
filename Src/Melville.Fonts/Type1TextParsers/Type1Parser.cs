using System.IO.Pipelines;
using System.Security.Cryptography.X509Certificates;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Fonts.Type1TextParsers;

internal readonly struct Type1Parser(IMultiplexSource source)
{
    private readonly DecryptingByteSource decryptingSource = new(source);
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync()
    {
        var parser = new PostscriptEngine(
            PostscriptOperatorCollections.BaseLanguage()
                .With(AddEexec));
         await parser.ExecuteAsync(new Tokenizer(decryptingSource)).CA();
        if (!parser.OperandStack[1].TryGet(out IPostscriptDictionary dict))
            throw new InvalidDataException(
                "Type 1 Text Font did not evaluate to a dictionary");
        return new Type1GenericFont(dict);
    }

    private void AddEexec(IPostscriptDictionary obj)
    {
        obj.Put("eexec", PostscriptValueFactory.Create((IExternalFunction)decryptingSource));
    }
}

public partial class DecryptingByteSource : 
    BuiltInFunction, IByteSourceWithGlobalPosition
{
    private readonly IMultiplexSource multiplexSource;
    [DelegateTo] IByteSourceWithGlobalPosition source;

    public DecryptingByteSource(IMultiplexSource multiplexSource)
    {
        this.multiplexSource = multiplexSource;
        source = new ByteSourceWithGlobalPosition(multiplexSource.ReadPipeFrom(0), 0);
    }


    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        engine.OperandStack.Pop();
        var result = new StreamReader(DecodedStream()).ReadToEnd();
        source = new ByteSourceWithGlobalPosition(
            PipeReader.Create(DecodedStream()), source.Position);
    }

    private ExecDecodeStream DecodedStream()
    {
        return new ExecDecodeStream(multiplexSource.ReadFrom(source.Position), 
            55665);
    }
}