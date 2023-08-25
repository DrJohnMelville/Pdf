using System;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class RelationalAndBitwiseOperators
{
    [PostscriptMethod("eq")]
    private static bool EqualOperation(in PostscriptValue a, in PostscriptValue b) =>
        EqualOperatorImpl.IsEqual(a, b);

    [PostscriptMethod("ne")]
    private static bool NotEqualOperation(in PostscriptValue a, in PostscriptValue b) =>
        !EqualOperatorImpl.IsEqual(a, b);

    [PostscriptMethod("not")]
    private static PostscriptValue Not(in PostscriptValue a) =>
        a.TryGet(out long asLong) ? ~asLong : !a.Get<bool>();

    [PostscriptMethod("ge")]
    private static bool GreaterOrEqual(in PostscriptValue a, in PostscriptValue b) =>
        EqualOperatorImpl.Compare(a, b) >= 0;

    [PostscriptMethod("gt")]
    private static bool GreaterThan(in PostscriptValue a, in PostscriptValue b) =>
        EqualOperatorImpl.Compare(a, b) > 0;

    [PostscriptMethod("le")]
    private static bool LessOrEqual(in PostscriptValue a, in PostscriptValue b) =>
        EqualOperatorImpl.Compare(a, b) <= 0;

    [PostscriptMethod("lt")]
    private static bool LessThan(in PostscriptValue a, in PostscriptValue b) =>
        EqualOperatorImpl.Compare(a, b) < 0;

    [PostscriptMethod("and")]
    private static PostscriptValue And(in PostscriptValue a, in PostscriptValue b) =>
        IntOrBoolOp(a, b, (x, y) => x & y, (x, y) => x & y);

    [PostscriptMethod("or")]
    private static PostscriptValue Or(in PostscriptValue a, in PostscriptValue b) =>
        IntOrBoolOp(a, b, (x, y) => x | y, (x, y) => x | y);

    [PostscriptMethod("xor")]
    private static PostscriptValue Xor(in PostscriptValue a, in PostscriptValue b) =>
        IntOrBoolOp(a, b, (x, y) => x ^ y, (x, y) => x ^ y);

    [PostscriptMethod("bitshift")]
    private static long BitShift(long baseValue, int shift) =>
        shift >= 0 ? baseValue << shift : baseValue >> -shift;

    private static PostscriptValue IntOrBoolOp(in PostscriptValue a, in PostscriptValue b,
        Func<bool, bool, PostscriptValue> asBools, Func<long, long, PostscriptValue> asLongs) =>
        a.IsNumber ? asLongs(a.Get<long>(), b.Get<long>()) : asBools(a.Get<bool>(), b.Get<bool>());
}