using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.FunctionLibrary
{
    internal abstract class IntOrBinaryOperation<T> : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value)
        {
            var right = engine.OperandStack.Pop();
            var left = engine.OperandStack.Pop();
            engine.Push(ComputeOperation(left, right));
        }

        private PostscriptValue ComputeOperation(PostscriptValue left, PostscriptValue right) =>
            (IsIntegerType(left) && IsIntegerType(right))
                ? Op(left.Get<long>(), right.Get<long>())
                : Op(left.Get<T>(), right.Get<T>());

        protected abstract PostscriptValue Op(long a, long b);
        protected abstract PostscriptValue Op(T a, T b);

        protected virtual bool IsIntegerType(in PostscriptValue value) =>
            value.IsInteger;
    }
}