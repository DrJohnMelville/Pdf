using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Execution;
using Microsoft.CodeAnalysis.Text;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton]
public partial class StringExecutor : IExecutePostscript
{
    public void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        var tokenizer = new Tokenizer(value.Get<Memory<byte>>());
        engine.ExecutionStack.Push(tokenizer.GetAsyncEnumerator(), value);

    }

    public string WrapTextDisplay(string text) => text;
    public bool IsExecutable => true;
}
