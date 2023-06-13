using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.FunctionLibrary
{
    internal abstract class IntAndRealBinaryOperation : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value)
        {
            var right = engine.OperandStack.Pop();
            var left = engine.OperandStack.Pop();
            engine.Push(ComputeOperation(left, right));
        }

        private PostscriptValue ComputeOperation(PostscriptValue left, PostscriptValue right) =>
            (left.IsInteger && right.IsInteger)
                ? Op(left.Get<long>(), right.Get<long>())
                : Op(left.Get<double>(), right.Get<double>());

        protected abstract PostscriptValue Op(long a, long b);
        protected abstract PostscriptValue Op(double a, double b);
    }
}