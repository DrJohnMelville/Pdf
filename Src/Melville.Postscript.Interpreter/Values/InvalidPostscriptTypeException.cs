using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// This is an exception that is thrown when a PostScriptValue is the wrong type for a given type
/// </summary>
public class InvalidPostscriptTypeException : Exception
{
    /// <summary>
    /// Create a new InvalidPostscriptTypeException
    /// </summary>
    /// <param name="message">The message that describes the error.</param> 
    public InvalidPostscriptTypeException(string? message) : base(message)
    {
    }

}