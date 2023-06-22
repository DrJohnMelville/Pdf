using System;
using System.Buffers;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static class LoopSources
{
#pragma warning disable CS1998 // Async methods are needed to create state machine
    public static IEnumerator<PostscriptValue> For(
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

    public static IEnumerator<PostscriptValue> Repeat(
        int count, PostscriptValue proc)
    {
        var innerItem = WrapBody(proc);
        for (int i = 0; i < count; i++)
        {
            yield return innerItem;
        }
    }

    public static IEnumerator<PostscriptValue> Loop(
        PostscriptValue proc)
    {
        var innerItem = WrapBody(proc);
        while (true)
        {
            yield return innerItem;
        }
    }

    public static IEnumerator<PostscriptValue> ForAll(
        IPostscriptComposite composite, PostscriptValue proc)
    {
        var values = composite.CreateForAllCursor();
        var buffer = ArrayPool<PostscriptValue>.Shared.Rent(values.ItemsPer);
        var innerProc = WrapBody(proc);
        var position = 0;
        while (values.TryGetItr(buffer.AsSpan(0, values.ItemsPer), ref position))
        {
            for (var i = 0; i < values.ItemsPer; i++)
            {
                yield return buffer[i].AsLiteral();
            }
            yield return innerProc;
        }
        ArrayPool<PostscriptValue>.Shared.Return(buffer);
    }
}

