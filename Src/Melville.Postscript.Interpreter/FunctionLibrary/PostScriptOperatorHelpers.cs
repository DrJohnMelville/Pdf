using System;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// Accessory methods for writing postscript operations
/// </summary>
public static class PostScriptOperatorHelpers
{
    /// <summary>
    /// Push a postscriptvalue onto theoperand stack.
    /// </summary>
    /// <param name="engine">The engine to push the value into.</param>
    /// <param name="value">The value to push.</param>
    public static void Push(this PostscriptEngine engine, PostscriptValue value) =>
        engine.OperandStack.Push(value);

    /// <summary>
    /// Pop one value off the stack and cast it appropriately.
    /// </summary>
    /// <typeparam name="T">The type to cast the popped value to.</typeparam>
    /// <param name="engine">The engine containing the value.</param>
    public static T PopAs<T>(this PostscriptEngine engine) =>
        engine.OperandStack.Pop().Get<T>();

    /// <summary>
    /// Pop two values off the stact and cast them to expected types
    /// </summary>
    /// <typeparam name="T1">The first expected task.</typeparam>
    /// <typeparam name="T2">The second expected task.</typeparam>
    /// <param name="engine">The engine to get values from.</param>
    /// <param name="a">Receives the first value.</param>
    /// <param name="b">Receives the second value.</param>
    public static void PopAs<T1, T2>(this PostscriptEngine engine, out T1 a, out T2 b)
    {
        b = engine.PopAs<T2>();
        a = engine.PopAs<T1>(); 
    }

    /// <summary>
    /// Pop thre values off the stact and cast them to expected types
    /// </summary>
    /// <typeparam name="T1">The first expected type.</typeparam>
    /// <typeparam name="T2">The second expected type.</typeparam>
    /// <typeparam name="T3">The third expected type.</typeparam>
    /// <param name="engine">The engine to get values from.</param>
    /// <param name="a">Receives the first value.</param>
    /// <param name="b">Receives the second value.</param>
    /// <param name="c">Receives the third value.</param>
    public static void PopAs<T1, T2, T3>(this PostscriptEngine engine, out T1 a, out T2 b, out T3 c)
    {
        c = engine.PopAs<T3>();
        engine.PopAs(out a, out b);
    }

    /// <summary>
    /// Pop thre values off the stact and cast them to expected types
    /// </summary>
    /// <typeparam name="T1">The first expected type.</typeparam>
    /// <typeparam name="T2">The second expected type.</typeparam>
    /// <typeparam name="T3">The third expected type.</typeparam>
    /// <typeparam name="T4">The fourth expected type.</typeparam>
    /// <param name="engine">The engine to get values from.</param>
    /// <param name="a">Receives the first value.</param>
    /// <param name="b">Receives the second value.</param>
    /// <param name="c">Receives the third value.</param>
    /// <param name="d">Receives the fourth value.</param>
    public static void PopAs<T1, T2, T3, T4>(
        this PostscriptEngine engine, out T1 a, out T2 b, out T3 c, out T4 d)
    {
        engine.PopAs(out c, out d);
        engine.PopAs(out a, out b);
    }

    /// <summary>
    /// Get a span of values of the same type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="engine"></param>
    /// <param name="values"></param>
    public static void PopSpan<T>(this PostscriptEngine engine, Span<T> values)
    {
        for (int i = values.Length -1; i >= 0; i--)
        {
            values[i] = engine.PopAs<T>();
        }
    }

    /// <summary>
    /// Pop two uncasted values off the stack.
    /// </summary>
    /// <param name="engine">The engine to pop the values from.</param>
    /// <returns></returns>
    public static (PostscriptValue, PostscriptValue) PopTwo(this PostscriptEngine engine)
    {
        var b = engine.OperandStack.Pop();
        var a = engine.OperandStack.Pop();
        return (a, b);
    }
}