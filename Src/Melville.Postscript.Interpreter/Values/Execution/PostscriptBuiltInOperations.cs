using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values.Execution;

/// <summary>
/// Executable operations that are built into the tokenizer.
/// </summary>
public static partial class PostscriptBuiltInOperations
{
    /// <summary>
    /// Push the argument onto the stack. 
    /// </summary>
    public static IExternalFunction PushArgument = new PushArgumentBuiltInFuncImpl();
    private sealed class PushArgumentBuiltInFuncImpl : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value)
        {
            engine.OperandStack.Push(value);
        }

        public override bool IsExecutable => false;
    }

    /// <summary>
    /// Lookup the name in the dictionary and executing the resulting object. 
    /// </summary>
    public static IExternalFunction ExecuteFromDictionary = new ExecuteFromDictionaryBuiltInFuncImpl();
    private sealed class ExecuteFromDictionaryBuiltInFuncImpl : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value)
        {
            var referencedValue = engine.DictionaryStack.Get(value);
            referencedValue.ExecutionStrategy.Execute(engine, value);
        }
    }
}

