using System.Collections.Generic;

namespace Melville.Postscript.Interpreter.Values;

internal interface IPostscriptComposite
{
    bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result);
}

internal static class PostscriptCompositeImpl
{
    public static PostscriptValue Get(
        this IPostscriptComposite composite, in PostscriptValue indexOrKey) =>
        composite.TryGet(indexOrKey, out var result)
            ? result
            : throw new KeyNotFoundException($"Cannot find key {indexOrKey}.");
}