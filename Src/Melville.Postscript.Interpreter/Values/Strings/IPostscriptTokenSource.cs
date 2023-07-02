using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values
{
    internal interface IPostscriptTokenSource
    {
        void GetToken(OperandStack stack);
    }
}