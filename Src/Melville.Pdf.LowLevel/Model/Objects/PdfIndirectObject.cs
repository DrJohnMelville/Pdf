using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects;



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

    public bool IsNull => TryGetEmbeddedDirectValue(out var dirVal) && dirVal.IsNull;

    #region Getter

    public ValueTask<PdfDirectObject> LoadValueAsync() =>
        valueStrategy is IIndirectObjectSource source?
            source.LookupAsync(Memento):
        new(CreateDirectValueUnsafe());

    private PdfDirectObject CreateDirectValueUnsafe()
    {
        Debug.Assert(IsEmbeddedDirectValue());
        return new PdfDirectObject(valueStrategy, Memento);
    }

    public bool IsEmbeddedDirectValue() => valueStrategy is not IIndirectObjectSource;

    public bool TryGetEmbeddedDirectValue(out PdfDirectObject value) =>
        valueStrategy is IIndirectObjectSource
            ? ((PdfDirectObject)default).AsFalseValue(out value)
            : CreateDirectValueUnsafe().AsTrueValue(out value);

    public bool TryGetEmbeddedDirectValue<T>(out T value)
    {
        value = default;
        return TryGetEmbeddedDirectValue(out PdfDirectObject dv) &&
               dv.TryGet(out value);
    }

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

    public static implicit operator PdfIndirectObject(bool value) => (PdfDirectObject)value;
    public static implicit operator PdfIndirectObject(int value) => (PdfDirectObject)value;
    public static implicit operator PdfIndirectObject(long value) => (PdfDirectObject)value;
    public static implicit operator PdfIndirectObject(double value) => (PdfDirectObject)value;
    public static implicit operator PdfIndirectObject(string value) => (PdfDirectObject)value;
    public static implicit operator PdfIndirectObject(PdfArray value) => (PdfDirectObject)value;
    public static implicit operator PdfIndirectObject(PdfDictionary value) => (PdfDirectObject)value;
    public static implicit operator PdfIndirectObject(in ReadOnlySpan<byte> value) => 
        (PdfDirectObject)value;

    public override string ToString() => NonNullValueStrategy() switch
    {
        IPostscriptValueStrategy<string> vs => vs.GetValue(Memento),
        _=> valueStrategy.ToString()
    };

    public bool NeedsLeadingSpace() =>
        !TryGetEmbeddedDirectValue(out PdfDirectObject direct) || direct.NeedsLeadingSpace;

    #endregion
}

public static class IndirectValueOperations
{
    public static async ValueTask<T> LoadValueAsync<T>(this PdfIndirectObject value) =>
        (await value.LoadValueAsync().CA()).Get<T>();
}