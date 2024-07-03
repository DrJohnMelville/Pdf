using System.Security.Cryptography.X509Certificates;
using Melville.Parsing.AwaitConfiguration;
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
    private readonly EexecDecryptingByteSource eexecDecryptingSource = new(source);
    private readonly Type1FontFactory factory = new();
    public async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync()
    {
        var parser = new PostscriptEngine(
            PostscriptOperatorCollections.BaseLanguage()
                .With(AddEexec));
         await parser.ExecuteAsync(new Tokenizer(eexecDecryptingSource)).CA();

         return factory.Result;
    }

    private void AddEexec(IPostscriptDictionary obj)
    {
        obj.Put("eexec", PostscriptValueFactory.Create((IExternalFunction)eexecDecryptingSource));
        obj.Put("definefont", PostscriptValueFactory.Create(factory));
    }
}