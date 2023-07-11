using System.Diagnostics.CodeAnalysis;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Model.Objects2;

/// <summary>
/// this defines a Pdf Object
/// </summary>
public readonly partial struct PdfDirectValue
{
    /// <summary>
    /// Strategy object that defines the type of the PdfObject
    /// </summary>
    [FromConstructor] private readonly object? valueStrategy;
    
    /// <summary>
    /// A memento that allows most PdfObjects to be represented without allocations
    /// </summary>
    [FromConstructor] private readonly MementoUnion memento;
    private object NonNullValueStrategy() => (valueStrategy ?? PostscriptNull.Instance);


    partial void OnConstructed()
    {
        #warning put in an assert to make sure no indirect value strategy is used.
    }

    #region TypeTesters

    
    public bool IsBool => valueStrategy == PostscriptBoolean.Instance;
    public bool IsNull => valueStrategy is null || valueStrategy == PostscriptNull.Instance;
    #endregion

    #region Value Accessors

    public bool TryGet<T>([NotNullWhen(true)] out T? value) => NonNullValueStrategy() switch
    {
        T valAsT => valAsT.AsTrueValue(out value),
        IPostscriptValueStrategy<T> prov => prov.GetValue(memento).AsTrueValue(out value),
        _=> default(T).AsFalseValue(out value)
    };

    public T Get<T>() => TryGet(out T? value)
        ? value
        : throw new PdfParseException($"Value {this} is not of type {typeof(T)}");

    public override string ToString() => NonNullValueStrategy() switch
    {
        IPostscriptValueStrategy<string> vs => vs.GetValue(memento),
        _=> valueStrategy.ToString()
    };


    #endregion

    #region Operators and Factories

    public static implicit operator PdfDirectValue(bool value) =>
        new(PostscriptBoolean.Instance, new MementoUnion(value));

    #endregion
}