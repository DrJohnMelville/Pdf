using System;

namespace Melville.Postscript.Interpreter.Values.Interfaces;

/// <summary>
/// Base class for all the Postscript Exceptions
/// </summary>
public class PostscriptException : Exception
{
    /// <summary>
    /// Create a new InvalidPostscriptTypeException
    /// </summary>
    /// <param name="message">The message that describes the error.</param> 
    public PostscriptException(string? message) : base(message)
    {
    }

}