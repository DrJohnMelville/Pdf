using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

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


/// <summary>
/// This is an exception that is thrown when a PostscriptValue is the wrong type for a given type
/// </summary>
public class PostscriptInvalidTypeException : Exception
{
    /// <summary>
    /// Create a new PostscriptInvalidTypeException
    /// </summary>
    /// <param name="message">The message that describes the error.</param> 
    public PostscriptInvalidTypeException(string? message) : base(message)
    {
    }

}

/// <summary>
/// This is an exception that is thrown when the interpreter cannot lex or parse the program.
/// </summary>
public class PostscriptParseException : Exception
{
    /// <summary>
    /// Create a new PostscriptParseException
    /// </summary>
    /// <param name="message">The message that describes the error.</param> 
    public PostscriptParseException(string? message) : base(message)
    {
    }

}

