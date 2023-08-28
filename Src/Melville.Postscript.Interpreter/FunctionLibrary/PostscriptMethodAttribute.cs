using System;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// Defines that a method should be auto generated as a postscript operation
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PostscriptMethodAttribute : Attribute
{
    /// <summary>
    /// Creates a PostscriptMethodAttribute
    /// </summary>
    /// <param name="postScriptName">The name of the operation in postscript</param>
    public PostscriptMethodAttribute(string postScriptName)
    {
    }
}

/// <summary>
/// Define code to retrieve a given type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TypeShortcutAttribute : Attribute
{
    /// <summary>
    /// Declare a code to retrieve a given type.
    /// </summary>
    /// <param name="typeName">The complete name of the type to replace</param>
    /// <param name="code"></param>
    public TypeShortcutAttribute(string typeName, string code){}
}
