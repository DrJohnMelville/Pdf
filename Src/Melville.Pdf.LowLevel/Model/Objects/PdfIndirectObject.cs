using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// This class represnts a pdf object that could be either an enclosed direct object
/// or it could be a reference to an indirect object in the pdf file.
/// </summary>
public readonly partial struct PdfIndirectObject
{
    /// <summary>
    /// Strategy object that defines the type of the PdfObject
    /// </summary>
    [FromConstructor] private readonly object? valueStrategy;
    
    /// <summary>
    /// A memento that allows most PdfObjects to be represented without allocations
    /// </summary>
    [FromConstructor] public readonly MementoUnion Memento { get; }

    private object NonNullValueStrategy() => (valueStrategy ?? PostscriptNull.Instance);

    internal PdfIndirectObject(IIndirectObjectSource src, long objNum, long generation) :
        this((object?)src, MementoUnion.CreateFrom(objNum, generation)){}

    /// <summary>
    /// True if this is a null value, false otherwise.
    /// </summary>
    public bool IsNull => TryGetEmbeddedDirectValue(out var dirVal) && dirVal.IsNull;

    #region Getter
    /// <summary>
    /// Get the direct value represented by this value, loading from disk if necessary
    /// </summary>
    /// <returns></returns>
    public ValueTask<PdfDirectObject> LoadValueAsync() =>
        valueStrategy is IIndirectObjectSource source?
            source.LookupAsync(Memento):
        new(CreateDirectValueUnsafe());

    internal bool VerifyIsIndirectRefFromObjectStream(int objectStreamNumber) =>
        valueStrategy is ObjectStreamDeferredPdfStrategy  deferredStrategy &&
           deferredStrategy.ComesFromStream(objectStreamNumber, Memento);

    private PdfDirectObject CreateDirectValueUnsafe()
    {
        Debug.Assert(IsEmbeddedDirectValue());
        return new PdfDirectObject(valueStrategy, Memento);
    }

    /// <summary>
    /// True if the value contains an embedded direct value, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool IsEmbeddedDirectValue() => valueStrategy is not IIndirectObjectSource;

    /// <summary>
    /// Try to get the embedded PDF value.
    /// </summary>
    /// <param name="value">Receives the embedded value</param>
    /// <returns>True if the object has a direct value, false otherwise</returns>
    public bool TryGetEmbeddedDirectValue(out PdfDirectObject value) =>
        valueStrategy is IIndirectObjectSource
            ? ((PdfDirectObject)default).AsFalseValue(out value)
            : CreateDirectValueUnsafe().AsTrueValue(out value);


    /// <summary>
    /// Try to get the embedded PDF value and cast it do a desired type.
    /// </summary>
    /// <param name="value">Receives the embedded value</param>
    /// <returns>True if the object has a direct value, false otherwise</returns>
    public bool TryGetEmbeddedDirectValue<T>([NotNullWhen(true)]out T? value)
    {
        value = default;
        return TryGetEmbeddedDirectValue(out PdfDirectObject dv) &&
               dv.TryGet(out value);
    }

    /// <summary>
    /// Get the object number and generation of this indirect object, or
    /// throw an exception if this is a direct object
    /// </summary>
    /// <exception cref="PdfParseException">If the object has an embedded direct object</exception>
    public (int ObjectNumber, int Generation) GetObjectReference() =>
        (TryGetObjectReference(out int number, out int generation))
            ? (number, generation)
            : throw new PdfParseException("Try to get an object reference from a non reference");

    private bool TryGetObjectReference(out int objectNumber, out int generation)
    {
        if (valueStrategy is IIndirectObjectSource ivs)
        {
            return ivs.TryGetObjectReference(out objectNumber, out generation, Memento);
        }

        objectNumber = generation = 0;
        return false;
    }
    #endregion

    #region Implicit Operators

    /// <summary>
    /// Create a PdfIndirectObject from a boolean
    /// </summary>
    public static implicit operator PdfIndirectObject(bool value) => (PdfDirectObject)value;
    
    
    /// <summary>
    /// Create a PdfIndirectObject from an int
    /// </summary>
    public static implicit operator PdfIndirectObject(int value) => (PdfDirectObject)value;
    
    /// <summary>
    /// Create a PdfIndirectObject from a long
    /// </summary>
    public static implicit operator PdfIndirectObject(long value) => (PdfDirectObject)value;
    
    /// <summary>
    /// Create a PdfIndirectObject from a double
    /// </summary>
    public static implicit operator PdfIndirectObject(double value) => (PdfDirectObject)value;
    
    /// <summary>
    /// Create a PdfIndirectObject from a C# string
    /// </summary>
    public static implicit operator PdfIndirectObject(string value) => (PdfDirectObject)value;

    /// <summary>
    /// Create a PdfIndirectObject from a PdfArray
    /// </summary>
    public static implicit operator PdfIndirectObject(PdfArray value) => (PdfDirectObject)value;

    /// <summary>
    /// Create a PdfIndirectObject from a PdfDictionary
    /// </summary>
    public static implicit operator PdfIndirectObject(PdfDictionary value) => (PdfDirectObject)value;

    /// <summary>
    /// Create a PdfIndirectObject from a ReadOnlySpan of bytes
    /// </summary>
    public static implicit operator PdfIndirectObject(in ReadOnlySpan<byte> value) => 
        (PdfDirectObject)value;

    /// <summary>
    /// Present the indirectObject as a string.  This is intended for debug and test
    /// so it may not be very effficient.
    /// </summary>
    public override string ToString() => NonNullValueStrategy() switch
    {
        IPostscriptValueStrategy<string> vs => vs.GetValue(Memento),
        var x => x.ToString()??""
    };

    /// <summary>
    /// True if this item needs a leading space when printed in a dictionary, false otherwise.
    /// </summary>
    /// <returns></returns>
    public bool NeedsLeadingSpace() =>
        !TryGetEmbeddedDirectValue(out PdfDirectObject direct) || direct.NeedsLeadingSpace;

    #endregion
}

/// <summary>
/// Extensions object for IndirectValueOperations.
/// </summary>
public static class IndirectValueOperations
{
    /// <summary>
    /// Load the indirect value and cast to a desired type in a single operation.
    /// </summary>
    /// <typeparam name="T">The desired type</typeparam>
    /// <param name="value">The PdfIndirectObject to get the value from</param>
    /// <returns>The value pointed to by the indirect object -- casted to T</returns>
    public static async ValueTask<T> LoadValueAsync<T>(this PdfIndirectObject value) =>
        (await value.LoadValueAsync().CA()).Get<T>();
}