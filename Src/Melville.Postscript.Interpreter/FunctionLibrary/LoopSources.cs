using System;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static class LoopSources
{
#pragma warning disable CS1998 // Async methods are needed to create state machine
    public static async IAsyncEnumerator<PostscriptValue> For(
        double initial, double increment, double limit, PostscriptValue proc)
    {
        var innerItem = WrapBody(proc);
        for (double d = initial; NotDone(d, increment, limit); d+= increment)
        {
            yield return PostscriptValueFactory.Create(d);
            yield return innerItem;
        }
    }

    private static PostscriptValue WrapBody(in PostscriptValue proc) =>
        PostscriptValueFactory.Create(
            new ExecutionWrapper(proc));

    private static bool NotDone(double d, double increment, double limit) => increment > 0 ? d <= limit : d >= limit;

    public static async IAsyncEnumerator<PostscriptValue> Repeat(
        int count, PostscriptValue proc)
    {
        var innerItem = WrapBody(proc);
        for (int i = 0; i < count; i++)
        {
            yield return innerItem;
        }
    }

    public static async IAsyncEnumerator<PostscriptValue> Loop(
        PostscriptValue proc)
    {
        var innerItem = WrapBody(proc);
        while (true)
        {
            yield return innerItem;
        }
    }
}

