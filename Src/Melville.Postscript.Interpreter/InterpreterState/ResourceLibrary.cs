using System.Collections.Generic;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

public readonly struct ResourceLibrary
{
    private readonly Dictionary<(PostscriptValue, PostscriptValue), PostscriptValue> items = new();

    public ResourceLibrary()
    {
    }

    public void Put(in PostscriptValue category, in PostscriptValue key, in PostscriptValue value) => 
        items[(category,key)] = value;

    public PostscriptValue Get(in PostscriptValue category, in PostscriptValue key) =>
        items.TryGetValue((category, key), out var ret)
            ? ret
            : throw new PostscriptNamedErrorException("resource does not exist.", "resourceundefined");

    public void Undefine(in PostscriptValue category, in PostscriptValue key) => 
        items.Remove((category, key));

    public bool ContainsKey(PostscriptValue category, PostscriptValue key) => items.ContainsKey((category, key));
}