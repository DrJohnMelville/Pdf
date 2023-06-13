using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.FunctionLibrary
{
    internal abstract class IntAndRealUnaryOperation : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value)
        {
            engine.Push(ComputeOperation(engine.OperandStack.Pop()));
        }

        private PostscriptValue ComputeOperation(PostscriptValue left) =>
            (left.IsInteger)
                ? Op(left.Get<long>())
                : Op(left.Get<double>());

        protected abstract PostscriptValue Op(long a);
        protected abstract PostscriptValue Op(double a);
    }
}