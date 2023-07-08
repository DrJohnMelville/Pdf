namespace Melville.Postscript.Interpreter.Values.Interfaces;

/// <summary>
/// This is an exception which cam be caught within the postscript main loop and
/// fed into the postscript error handler.
/// </summary>
public class PostscriptNamedErrorException: PostscriptException
{
    /// <summary>
    /// The postscript name for the error that occurred.
    /// </summary>
    public string ErrorName { get; }

    /// <summary>
    /// Create a NamedErrorException
    /// </summary>
    /// <param name="message">Error name</param>
    /// <param name="errorName">Postscript na</param>
    public PostscriptNamedErrorException(string? message, string errorName) : base(message)
    {
        ErrorName = errorName;
    }
}