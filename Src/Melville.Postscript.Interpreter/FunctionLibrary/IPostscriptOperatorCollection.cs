using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// Defines a set of Postscript Operators that can be added to a postscript dictionary
/// </summary>
public interface IPostscriptOperatorCollection
{
    /// <summary>
    /// Add the operations defined in this class to the dictionary
    /// </summary>
    /// <param name="dictionary">The dictionary to add the items to.</param>
    void AddOperations(IPostscriptDictionary dictionary);
}

/// <summary>
/// Request autogeneration of a postscript collection
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PostscriptCollectionAttribute: Attribute {}

/// <summary>
/// Defines that a method should be auto generated as a postscript operation
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public partial class PostscriptMethodAttribute : Attribute
{
    /// <summary>
    /// The name of the operation in postscript
    /// </summary>
    [FromConstructor] public string PostScriptName { get; }
}

[PostscriptCollection]
internal partial class SystemTokens : IPostscriptOperatorCollection
{
    [PostscriptMethod("true")]
    private PostscriptValue True() => true;
    [PostscriptMethod("false")]
    private PostscriptValue False() => false;
    [PostscriptMethod("null")]
    private PostscriptValue Null() => PostscriptValueFactory.CreateNull();

    public void AddOperations(IPostscriptDictionary dictionary)
    {
        throw new NotImplementedException();
    }
}