using System;
using Melville.INPC;

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

/// <summary>
/// This is an exception which cam be caught within the postscript main loop and
/// fed into the postscript error handler.
/// </summary>
public class PostscriptNamedErrorException: PostscriptException
{
    public string ErrorName { get; }

    /// <summary>
    /// Create a NamedErrorException
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errorName"></param>
    public PostscriptNamedErrorException(string? message, string errorName) : base(message)
    {
        ErrorName = errorName;
    }
}

[Obsolete("Want to use the PostscriptNamedErrorException")]
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

