using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This class allows up to push a tuple to the  operand stack which allows for syntax in the
/// postscript operator methods that almost exactly duplicates the Postscript standard syntaxx.
/// </summary>
public static class CompositePushOperations
{
    /// <summary>
    /// Push the given values onto the stack in numerical order.
    /// </summary>
    /// <param name="stack">The stack to receive the values</param>
    /// <param name="values">The values to push</param>
    public static void Push(this OperandStack stack, (PostscriptValue, PostscriptValue) values)
    {
        stack.Push(values.Item1);
        stack.Push(values.Item2);
    }

    /// <summary>
    /// Push the given values onto the stack in numerical order.
    /// </summary>
    /// <param name="stack">The stack to receive the values</param>
    /// <param name="values">The values to push</param>
    public static void Push(this OperandStack stack, 
        (PostscriptValue, PostscriptValue, PostscriptValue) values)
    {
        stack.Push(values.Item1);
        stack.Push(values.Item2);
        stack.Push(values.Item3);
    }

    /// <summary>
    /// Push the given values onto the stack in numerical order.
    /// </summary>
    /// <param name="stack">The stack to receive the values</param>
    /// <param name="values">The values to push</param>
    public static void Push(this OperandStack stack, 
        (PostscriptValue, PostscriptValue, PostscriptValue, PostscriptValue) values)
    {
        stack.Push(values.Item1);
        stack.Push(values.Item2);
        stack.Push(values.Item3);
        stack.Push(values.Item4);
    }

    /// <summary>
    /// Push the given values onto the stack in numerical order.
    /// </summary>
    /// <param name="stack">The stack to receive the values</param>
    /// <param name="values">The values to push</param>
    public static void Push(this OperandStack stack, 
        (PostscriptValue, PostscriptValue, PostscriptValue, PostscriptValue, PostscriptValue) values)
    {
        stack.Push(values.Item1);
        stack.Push(values.Item2);
        stack.Push(values.Item3);
        stack.Push(values.Item4);
        stack.Push(values.Item5);
    }
}