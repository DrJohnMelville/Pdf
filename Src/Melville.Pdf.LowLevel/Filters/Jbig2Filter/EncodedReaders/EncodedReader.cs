using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Melville.INPC;

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
    ///  In Spec: IADS
    /// </summary>
    int DeltaS(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IADT
    /// </summary>
    int DeltaT(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IADW
    /// </summary>
    int DeltaWidth(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IAEX
    /// </summary>
    int ExportFlags(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IAFS
    /// </summary>
    int FirstS(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IAID
    /// </summary>
    int SymbolId(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IAIT
    /// </summary>
    int TCoordinate(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARDH
    /// </summary>
    int RefinementDeltaHeight(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARDW
    /// </summary>
    int RefinementDeltaWidth(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARDX
    /// </summary>
    int RefinementX(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARDY
    /// </summary>
    int RefinementY(ref SequenceReader<byte> source);
    /// <summary>
    ///  In Spec: IARI
    /// </summary>
    int RIBit(ref SequenceReader<byte> source);

    bool IsOutOfBand(int item);
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
[MacroItem("RIBit")]
[MacroCode("public TContext? ~0~Context {private get; init;}")]
[MacroCode("public int ~0~(ref SequenceReader<byte> source) => Read(ref source, VerifyExists(~0~Context), state);")]
public abstract partial class EncodedReader<TContext, TState>: IEncodedReader
{
    private readonly TState state;

    public EncodedReader(TState state)
    {
        this.state = state;
    }

    private TContext VerifyExists(TContext? context, [CallerArgumentExpression("context")] string caller = "") =>
        context ?? throw new InvalidOperationException($"No context defined for {caller}.");

    public abstract bool IsOutOfBand(int item);
    protected abstract int Read(ref SequenceReader<byte> source, TContext context, TState state);
}