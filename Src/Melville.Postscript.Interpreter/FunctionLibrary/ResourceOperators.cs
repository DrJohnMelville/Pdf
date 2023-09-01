using System;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class ResourceOperators
{
    [PostscriptMethod("defineresource")]
    public static PostscriptValue DefineResource(
        ResourceLibrary lib,
        in PostscriptValue key, in PostscriptValue instance, in PostscriptValue category)
    {
        lib.Put(category, key, instance);
        return instance;
    }

    [PostscriptMethod("findresource")]
    public static PostscriptValue FindResource(ResourceLibrary lib,
        in PostscriptValue key, in PostscriptValue category) =>
        lib.Get(category, key);
    [PostscriptMethod("undefineresource")]
    public static void UndefineResource(ResourceLibrary lib,
        in PostscriptValue key, in PostscriptValue category) =>
        lib.Undefine(category, key);
 
    [PostscriptMethod("resourcestatus")]
    public static void ResourceStatus(ResourceLibrary lib, OperandStack stack,
        in PostscriptValue key, in PostscriptValue category)
    {
        if (lib.ContainsKey(category, key))
        {
            stack.Push(1);
            stack.Push(-1);
            stack.Push(true);
        }
        else
        {
            stack.Push(false);
        }
    }
}