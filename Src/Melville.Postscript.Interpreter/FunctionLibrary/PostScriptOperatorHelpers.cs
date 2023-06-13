using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary
{
    internal static class PostScriptOperatorHelpers
    {
        public static T PopAs<T>(this PostscriptEngine engine) =>
            engine.OperandStack.Pop().Get<T>();

        public static void Push(this PostscriptEngine engine, PostscriptValue value) =>
            engine.OperandStack.Push(value);

        public static void PopAs<T1, T2>(this PostscriptEngine engine, out T1 a, out T2 b)
        {
            b = engine.PopAs<T2>();
            a = engine.PopAs<T1>(); 
        }
    }
}