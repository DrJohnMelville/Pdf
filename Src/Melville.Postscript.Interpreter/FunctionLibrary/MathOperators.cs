using System;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class MathOperators
{
    [PostscriptMethod("add")]
    private static PostscriptValue Add(in PostscriptValue left, in PostscriptValue right) =>
        LongOrDoubleOp(left, right, (a, b) => a + b, (a, b) => a + b);

    [PostscriptMethod("div")]
    private static double Divide(double left, double right) => left / right;

    [PostscriptMethod("idiv")]
    private static long IntegerDivide(long left, long right) => left / right;

    [PostscriptMethod("mod")]
    private static PostscriptValue Modulo(in PostscriptValue left, in PostscriptValue right) =>
        LongOrDoubleOp(left, right, (a, b) => a % b, (a, b) => a % b);

    [PostscriptMethod("mul")]
    private static PostscriptValue Multiply(in PostscriptValue left, in PostscriptValue right) =>
        LongOrDoubleOp(left, right, (a, b) => a * b, (a, b) => a * b);

    [PostscriptMethod("sub")]
    private static PostscriptValue Subtract(in PostscriptValue left, in PostscriptValue right) =>
        LongOrDoubleOp(left, right, (a, b) => a - b, (a, b) => a - b);

    [PostscriptMethod("abs")]
    private static PostscriptValue AbsoluteValue(in PostscriptValue value) =>
        LongOrDoubleOp(value, i => i < 0 ? -i : i, i => i < 0 ? -i : i);

    [PostscriptMethod("neg")]
    private static PostscriptValue Negate(in PostscriptValue value) =>
        LongOrDoubleOp(value, i => -i, i =>-i);

    [PostscriptMethod("ceiling")]
    private static PostscriptValue Ceiling(in PostscriptValue value) =>
        DoubleOnlyOp(value, i => Math.Ceiling(i));

    [PostscriptMethod("floor")]
    private static PostscriptValue Floor(in PostscriptValue value) =>
        DoubleOnlyOp(value, i => Math.Floor(i));

    [PostscriptMethod("round")]
    private static PostscriptValue Round(in PostscriptValue value) =>
        DoubleOnlyOp(value, i => Math.Round(i, MidpointRounding.AwayFromZero));
    
    [PostscriptMethod("truncate")]
    private static PostscriptValue Truncate(in PostscriptValue value) =>
        DoubleOnlyOp(value, i => Math.Truncate(i));

    [PostscriptMethod("sqrt")]
    private static double SquareRoot(double val) => Math.Sqrt(val);

    [PostscriptMethod("atan")]
    private static double ArcTangent(double num, double den) =>
        Math.Atan2(num, den) * 180.0 / Math.PI;

    [PostscriptMethod("sin")]
    private static double Sine(double num) =>
        Math.Sin(num * (Math.PI / 180.0));

    [PostscriptMethod("cos")]
    private static double Cosine(double num) =>
        Math.Cos(num * (Math.PI / 180.0));

    [PostscriptMethod("exp")]
    private static double RaiseToPower(double number, double exponent) =>
        Math.Pow(number, exponent);

    [PostscriptMethod("log")]
    private static double Log10(double num) => Math.Log10(num);
    
    [PostscriptMethod("ln")]
    private static double NaturalLog(double num) => Math.Log(num);

    [PostscriptMethod("srand")]
    private static void SetRandomSeed(PostscriptEngine engine, long seed) =>
        engine.Random.State = (uint)seed;

    [PostscriptMethod("rrand")]
    private static long GetRandomSeed(PostscriptEngine engine) => engine.Random.State;

    [PostscriptMethod("rand")]
    private static long RandomNumber(PostscriptEngine engine) => engine.Random.Next();
    
    private static PostscriptValue LongOrDoubleOp(
        in PostscriptValue left, in PostscriptValue right,
        Func<long, long, PostscriptValue> longOp,
        Func<double, double, PostscriptValue> doubleOp) =>
        left.IsInteger && right.IsInteger
            ? longOp(left.Get<long>(), right.Get<long>())
            : doubleOp(left.Get<double>(), right.Get<double>());

    private static PostscriptValue LongOrDoubleOp(
        in PostscriptValue value,
        Func<long, PostscriptValue> longOp,
        Func<double, PostscriptValue> doubleOp) =>
        value.IsInteger 
            ? longOp(value.Get<long>())
            : doubleOp(value.Get<double>());

    private static PostscriptValue DoubleOnlyOp(
        in PostscriptValue value,
        Func<double, PostscriptValue> doubleOp) =>
        value.IsInteger 
            ? value
            : doubleOp(value.Get<double>());

}