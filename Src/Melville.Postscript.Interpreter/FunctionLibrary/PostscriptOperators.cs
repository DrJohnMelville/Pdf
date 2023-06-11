using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// This is a library of operators for the postscript interpreter
/// </summary>
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
[MacroItem("PushTrue", "engine.OperandStack.Push(true);", "Push true on the stack.")]
[MacroItem("PushFalse", "engine.OperandStack.Push(false);", "Push false on the stack.")]
[MacroItem("PushNull", "engine.OperandStack.Push(PostscriptValueFactory.CreateNull());",
    "Push null on the stack.")]
[MacroItem("Pop", "engine.OperandStack.Pop();", "Discard the top element on the stack.")]
[MacroItem("Exchange", "engine.OperandStack.Exchange();", "Switch the top two items on the stack.")]
[MacroItem("Duplicate", "engine.OperandStack.Duplicate();", "Duplicate the top item on the stack.")]
[MacroItem("CopyTop", "engine.OperandStack.CopyTop(engine.OperandStack.Pop().Get<int>());", "Copy the top item on the stack.")]
[MacroItem("IndexOperation", "engine.OperandStack.IndexOperation(engine.OperandStack.Pop().Get<int>());", "Retrieve the item n items down the stack.")]
[MacroItem("Roll", "engine.OperandStack.Roll(engine.OperandStack.Pop().Get<int>(), engine.OperandStack.Pop().Get<int>());", "Roll the stack.")]
[MacroItem("ClearStack", "engine.OperandStack.Clear();", "Clear the stack.")]
[MacroItem("CountStack", "engine.OperandStack.PushCount();", "Count the items on the stack.")]
[MacroItem("PlaceMark", "engine.OperandStack.Push(PostscriptValueFactory.CreateMark());", "Place a mark on the stack")]
[MacroItem("ClearToMark", "engine.OperandStack.ClearToMark();", "Clear down to and including a mark")]
[MacroItem("CountToMark", "engine.OperandStack.CountToMark();", "Count items above the topmost mark mark")]
public static partial class PostscriptOperators
{
#if DEBUG
    private static void XX(PostscriptEngine engine)
    {
        
    }
#endif
}