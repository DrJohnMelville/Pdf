using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;

public interface IEncodedReader
{
    /// <summary>
    ///  In Spec: IAAI
    /// </summary>
    int AggregationSymbolInstances(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: SBHUFFBMSIZE
    /// </summary>
    int BitmapSize(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IADH
    /// </summary>
    int DeltaHeight(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IADS OR SBHUFFDS
    /// </summary>
    int DeltaS(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IADT or SBHUFFDT
    /// </summary>
    int DeltaT(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IADW
    /// </summary>
    int DeltaWidth(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IAEX or Table B1
    /// </summary>
    int ExportFlags(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IAFS OR SBHUFFS
    /// </summary>
    int FirstS(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IAID or SBSYMCODES
    /// </summary>
    int SymbolId(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IAIT or ReadDirectFromBitStream
    /// </summary>
    int TCoordinate(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARDH SBHUFFRDH
    /// </summary>
    int RefinementDeltaHeight(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARDW or SBHUFFRDW
    /// </summary>
    int RefinementDeltaWidth(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARDX or SBHUFFRDX
    /// </summary>
    int RefinementX(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARDY or SBHUFFRDY
    /// </summary>
    int RefinementY(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec:  SBHUFFRSIZE
    /// </summary>
    int RefinementSize(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARI
    /// </summary>
    int RIBit(ref SequenceReader<byte> source);

    bool IsOutOfBand(int item);

    void ReadBitmap(ref SequenceReader<byte> source, BinaryBitmap target);

    public void PrepareForRefinementSymbolDictionary(uint totalSymbols);
    void InvokeSymbolRefinement(BinaryBitmap destination, IBinaryBitmap reference, int deltaX, int deltaY,
        bool useTypicalPrediction,
        in RefinementTemplateSet refinementTemplate, ref SequenceReader<byte> source);
}

[MacroItem("AggregationSymbolInstances")]
[MacroItem("BitmapSize")]
[MacroItem("DeltaHeight")]
[MacroItem("DeltaS")]
[MacroItem("DeltaT")]
[MacroItem("DeltaWidth")]
[MacroItem("ExportFlags")]
[MacroItem("FirstS")]
[MacroItem("SymbolId")]
[MacroItem("TCoordinate")]
[MacroItem("RefinementDeltaHeight")]
[MacroItem("RefinementDeltaWidth")]
[MacroItem("RefinementX")]
[MacroItem("RefinementY")]
[MacroItem("RefinementSize")]
[MacroItem("RIBit")]
[MacroCode("public TContext? ~0~Context {protected get; set;}")]
[MacroCode("public int ~0~(ref SequenceReader<byte> source) => Read(ref source, VerifyExists(~0~Context));")]
public abstract partial class EncodedReader<TContext, TState>: IEncodedReader
{
    protected TState State { get; }

    public EncodedReader(TState state)
    {
        this.State = state;
    }

    protected TContext VerifyExists(TContext? context, [CallerArgumentExpression("context")] string caller = "") =>
        context ?? throw new InvalidOperationException($"No context defined for {caller}.");

    public abstract bool IsOutOfBand(int item);
    protected abstract int Read(ref SequenceReader<byte> source, TContext context);
    public abstract void ReadBitmap(ref SequenceReader<byte> source, BinaryBitmap target);
    public abstract void PrepareForRefinementSymbolDictionary(uint totalSymbols);

    public abstract void InvokeSymbolRefinement(BinaryBitmap destination, IBinaryBitmap reference, int deltaX,
        int deltaY, bool useTypicalPrediction,
        in RefinementTemplateSet refinementTemplate, ref SequenceReader<byte> source);
}