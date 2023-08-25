using System;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static class EqualOperatorImpl
{
    public static bool IsEqual(PostscriptValue val1, PostscriptValue val2)
    {
        return val1.Equals(val2) || EqualAsIntAndDouble(val1, val2);
    }

    private static bool EqualAsIntAndDouble(PostscriptValue val1, PostscriptValue val2) =>
        val1.IsNumber && val2.IsNumber && val1.Get<double>() == val2.Get<double>();

    public static int Compare(PostscriptValue v1, PostscriptValue v2)
    {
        if (v1.IsNumber)
            return v1.Get<double>().CompareTo(v2.Get<double>());
        if (v1.TryGet(out StringSpanSource ss1) && v2.TryGet(out StringSpanSource ss2))
            return Compare(ss1, ss2);
        throw new PostscriptException("Values are not comparable");
    }

    private static int Compare(StringSpanSource ss1, StringSpanSource ss2) =>
        ss1.GetSpan().SequenceCompareTo(ss2.GetSpan());
}