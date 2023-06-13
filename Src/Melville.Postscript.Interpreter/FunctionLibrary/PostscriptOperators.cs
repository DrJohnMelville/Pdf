using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values; // used in generated files
using Melville.Postscript.Interpreter.Values.Execution; // used in generated files

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
// Push constant tokens
[MacroItem("PushTrue", "engine.Push(true);", "Push true on the stack.")]
[MacroItem("PushFalse", "engine.Push(false);", "Push false on the stack.")]
[MacroItem("PushNull", "engine.Push(PostscriptValueFactory.CreateNull());",
    "Push null on the stack.")]
// Stack operators
[MacroItem("Pop", "engine.OperandStack.Pop();", "Discard the top element on the stack.")]
[MacroItem("Exchange", "engine.OperandStack.Exchange();", "Switch the top two items on the stack.")]
[MacroItem("Duplicate", "engine.OperandStack.Duplicate();", "Duplicate the top item on the stack.")]
[MacroItem("CopyTop", "engine.OperandStack.CopyTop(engine.PopAs<int>());", "Copy the top item on the stack.")]
[MacroItem("IndexOperation", "engine.OperandStack.IndexOperation(engine.PopAs<int>());", "Retrieve the item n items down the stack.")]
[MacroItem("Roll", "engine.OperandStack.Roll(engine.PopAs<int>(), engine.PopAs<int>());", "Roll the stack.")]
[MacroItem("ClearStack", "engine.OperandStack.Clear();", "Clear the stack.")]
[MacroItem("CountStack", "engine.OperandStack.PushCount();", "Count the items on the stack.")]
[MacroItem("PlaceMark", "engine.OperandStack.Push(PostscriptValueFactory.CreateMark());", "Place a mark on the stack")]
[MacroItem("ClearToMark", "engine.OperandStack.ClearToMark();", "Clear down to and including a mark")]
[MacroItem("CountToMark", "engine.OperandStack.CountToMark();", "Count items above the topmost mark mark")]
// Math Operators
[MacroItem("RealDivide", "engine.PopAs(out double a, out double b); engine.Push(a/b);", "Floating point division")]
[MacroItem("IntegerDivide", "engine.PopAs(out long a, out long b); engine.Push(a/b);", "Integer division")]
[MacroItem("SquareRoot", "engine.Push(Math.Sqrt(engine.PopAs<double>()));","Square Root")]
[MacroItem("ArcTangent", "engine.PopAs(out double num, out double den);engine.Push(Math.Atan2(num,den) * 180.0 / Math.PI);", "Arctangent")]
[MacroItem("Sine", "engine.Push(Math.Sin(engine.PopAs<double>() * Math.PI/180.0));", "Sine")]
[MacroItem("Cosine", "engine.Push(Math.Cos(engine.PopAs<double>() * Math.PI/180.0));", "Cosine")]
[MacroItem("RaiseToPower", "engine.PopAs(out double num, out double exp);engine.Push(Math.Pow(num,exp));", "Raise number to a power")]
[MacroItem("Log10", "engine.Push(Math.Log10(engine.PopAs<double>()));", "Log base 10")]
[MacroItem("NaturalLog", "engine.Push(Math.Log(engine.PopAs<double>()));", "Log base e")]
[MacroItem("SetRandomSeed", "engine.Random.State = (uint)engine.PopAs<long>();", "Set random seed")]
[MacroItem("GetRandomSeed", "engine.Push(engine.Random.State);", "get random seed")]
[MacroItem("GetNextRandom", "engine.Push(engine.Random.Next());", "get next random number")]
public static partial class PostscriptOperators
{
#if DEBUG
    private static void XX(PostscriptEngine engine)
    {
    }
#endif

    [MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: IntAndRealBinaryOperation
        {
            protected override PostscriptValue Op(long a, long b) => ~1~;
            protected override PostscriptValue Op(double a, double b) => ~1~;
        }

        """)]
    [MacroItem("Add", "a + b", "Add two numbers.")]
    [MacroItem("Modulo", "a % b", "Remainder after integer division.")]
    [MacroItem("Multiply", "a * b", "Multiply two numbers.")]
    [MacroItem("Subtract", "a - b", "difference of two numbers.")]
    static partial void NumberOpsHolder();

    [MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: IntAndRealUnaryOperation
        {
            protected override PostscriptValue Op(long a) => ~1~;
            protected override PostscriptValue Op(double a) => ~1~;
        }

        """)]
    [MacroItem("AbsoluteValue", "Math.Abs(a)", "AbsoluteValue")]
    [MacroItem("Negative", "a * -1", "Additive inverse.")]
    static partial void NumberUnaryOpsHolder();

    [MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: IntAndRealUnaryOperation
        {
            protected override PostscriptValue Op(long a) => a;
            protected override PostscriptValue Op(double a) => ~1~;
        }

        """)]
    [MacroItem("Ceiling", "Math.Ceiling(a)", "Smallest whole number bigger than item.")]
    [MacroItem("Floor", "Math.Floor(a)", "Largest whole number less than argument.")]
    [MacroItem("Round", "Math.Round(a)", "Round argument to nearest whole integer")]
    [MacroItem("Truncate", "Math.Truncate(a)", "Truncate fractional part of a number")]
    static partial void DoubleOnlyOpsHolder();
}