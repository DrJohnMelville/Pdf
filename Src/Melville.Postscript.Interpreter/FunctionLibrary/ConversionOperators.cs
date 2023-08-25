using System;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class ConversionOperators
{
    [PostscriptMethod("cvx")]
    private static PostscriptValue MakeExecutable(in PostscriptValue i) => i.AsExecutable();

    [PostscriptMethod("cvlit")]
    private static PostscriptValue MakeLiteral(in PostscriptValue i) => i.AsLiteral();

    [PostscriptMethod("xcheck")]
    private static bool IsExecutable(in PostscriptValue i) => i.ExecutionStrategy.IsExecutable;

    [PostscriptMethod("executeonly")]
    [PostscriptMethod("readonly")]
    [PostscriptMethod("noaccess")]
    private static void NoOperation(){}

    [PostscriptMethod("wcheck")]
    [PostscriptMethod("rcheck")]
    private static PostscriptValue FakeAccessCheck(in PostscriptValue _) => true;

    [PostscriptMethod("cvi")]
    private static long ConvertToLong(long i) => i;

    [PostscriptMethod("cvr")]
    private static double ConvertToDouble(double i) => i;

    [PostscriptMethod("cvn")]
    private static PostscriptValue ConvertToName(in PostscriptValue source)
    {
        var span = source.Get<StringSpanSource>().GetSpan();
        return PostscriptValueFactory.CreateString(span,
            IsExecutable(source) ? StringKind.Name : StringKind.LiteralName);
    }

    [PostscriptMethod("cvs")]
    private static PostscriptValue ConvertToString(in PostscriptValue i) =>
        PostscriptValueFactory.CreateString(i.ToString(), StringKind.String);

    [PostscriptMethod("cvrs")]
    private static PostscriptValue ConvertToRadixString(
        in PostscriptValue value, int radix, Memory<byte> target) =>
        new RadixPrinter(value, radix, target).CreateValue();

}