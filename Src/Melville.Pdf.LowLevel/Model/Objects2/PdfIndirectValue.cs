using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects2;

internal interface IIndirectValueSource: IPostscriptValueStrategy<string>
{
    ValueTask<PdfDirectValue> Lookup(in MementoUnion memento);
}

public readonly partial struct PdfIndirectValue
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

    internal PdfIndirectValue(IIndirectValueSource src, long objNum, long generation) :
        this(src, MementoUnion.CreateFrom(objNum, generation)){}

    #region Getter

    public ValueTask<PdfDirectValue> LoadValueAsync() =>
        valueStrategy is IIndirectValueSource source?
            source.Lookup(memento):
        new(new PdfDirectValue(valueStrategy, memento));

    #endregion

    #region Implicit Operators

    public static implicit operator PdfIndirectValue(bool value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(int value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(long value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(double value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(string value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(in ReadOnlySpan<byte> value) => 
        (PdfDirectValue)value;

    public override string ToString() => NonNullValueStrategy() switch
    {
        IPostscriptValueStrategy<string> vs => vs.GetValue(memento),
        _=> valueStrategy.ToString()
    };

    #endregion

}