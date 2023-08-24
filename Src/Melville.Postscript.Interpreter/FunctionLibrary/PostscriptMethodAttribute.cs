using System;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// Defines that a method should be auto generated as a postscript operation
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PostscriptMethodAttribute : Attribute
{
    private readonly string postScriptName;

    /// <summary>
    /// Creates a PostscriptMethodAttribute
    /// </summary>
    /// <param name="postScriptName">The name of the operation in postscript</param>
    public PostscriptMethodAttribute(string postScriptName)
    {
        this.postScriptName = postScriptName;
    }
}