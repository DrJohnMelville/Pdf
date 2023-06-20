using System;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static class EqualOperatorImpl
{
    public static bool IsEqual(PostscriptEngine engine)
    {
        var (val1, val2) = engine.PopTwo();
        if (val1.Equals(val2)) return true;
        if (val1.IsNumber && val2.IsNumber &&
            val1.Get<double>() == val2.Get<double>()) return true;
        return false;
    }

    public static int Compare(PostscriptEngine engine)
    {
        var (v1,v2) = engine.PopTwo();
        if (v1.IsNumber)
            return v1.Get<double>().CompareTo(v2.Get<double>());
        if (v1.TryGet(out StringSpanSource ss1) && v2.TryGet(out StringSpanSource ss2))
            return Compare(ss1, ss2);
        throw new PostscriptException("Values are not comparable");
    }

    private static int Compare(StringSpanSource ss1, StringSpanSource ss2)
    {
        return Compare(
            ss1.GetSpan(stackalloc byte[PostscriptString.ShortStringLimit]),
            ss2.GetSpan(stackalloc byte[PostscriptString.ShortStringLimit]));
    }

    private static int Compare(Span<byte> first, Span<byte> second) => 
        first.SequenceCompareTo(second);
}