using System.Buffers;
using Melville.INPC;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.GenericRegionRefinements;

namespace Melville.JBig2.EncodedReaders;

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
    void InvokeSymbolRefinement(IBinaryBitmap destination, IBinaryBitmap reference,
        int predictionContext,
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

    public TContext? SymbolIdContext {protected get; set;}
    public int SymbolId(ref SequenceReader<byte> source) => 
        ReadSymbol(ref source, VerifyExists(SymbolIdContext));
    
    protected TContext VerifyExists(TContext? context) =>
        context ??throw new InvalidOperationException($"No context defined.");

    public abstract bool IsOutOfBand(int item);
    protected abstract int Read(ref SequenceReader<byte> source, TContext context);
    protected abstract int ReadSymbol(ref SequenceReader<byte> source, TContext context);
    public abstract void ReadBitmap(ref SequenceReader<byte> source, BinaryBitmap target);
    public abstract void PrepareForRefinementSymbolDictionary(uint totalSymbols);

    public abstract void InvokeSymbolRefinement(IBinaryBitmap destination, IBinaryBitmap reference, 
        int predictionContext,
        in RefinementTemplateSet refinementTemplate, ref SequenceReader<byte> source);
}