using System;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// This strategy object represents the way to get a value of a given type out of a
/// PostscriptValue object -- this could be encoded in the memento, or it could ignore
/// the memento and have its own garbage collected value
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPostscriptValueStrategy<out T>
{
    /// <summary>
    /// Get the value of the PostscriptValue as the given type
    /// </summary>
    /// <param name="memento">the memento from the PostscriptValue</param>
    T GetValue(in MementoUnion memento);
}