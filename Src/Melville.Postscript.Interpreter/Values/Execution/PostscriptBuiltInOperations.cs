using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values.Execution;

/// <summary>
/// Executable operations that are built into the tokenizer.
/// </summary>
[MacroItem("PushArgument", "engine.OperandStack.Push(value);", "Push the argument onto the stack.")]
[MacroItem("ExecuteFromDictionary", """
        var referencedValue = engine.DictionaryStack.Get(value);
        referencedValue.Get<IExecutePostscript>().Execute(engine, value);
        """, "Lookup the name in the dictionary and executing the resulting object.")]
[MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
                public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: BuiltInFunction
        {
            public override void Execute(PostscriptEngine engine, in PostscriptValue value)
            {
                ~1~
            }
        }

        """)]
public static partial class PostscriptBuiltInOperations
{
}