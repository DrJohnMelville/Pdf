using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values
{
    [StaticSingleton()]
    internal sealed partial class ArrayExecutor : IExecutePostscript
    {
        public void Execute(PostscriptEngine engine, in PostscriptValue value)
        {
            var array = value.Get<PostscriptArray>();
            engine.ExecutionStack.Push(array.GetAsyncEnumerator());
        }

        public string WrapTextDisplay(string text) => text;
   
        public bool IsExecutable => true;
    }
}