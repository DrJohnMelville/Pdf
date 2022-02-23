using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public abstract class CcittEncoderBase : IStreamFilterDefinition
{
    protected readonly CcittParameters Parameters;
    protected readonly BitWriter BitWriter = new();
    private readonly BitReader BitReader = new();
    protected CcittLinePair Lines;
    private int currentReadPos;
    protected int a0 = -1;

    protected CcittEncoderBase(CcittParameters parameters)
    {
        Parameters = parameters;
        Lines = new CcittLinePair(parameters);
    }

    public int MinWriteSize => 10;

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, ref Span<byte> destination)
    {
        if (source.Length == 0)
            return (source.Position, BitWriter.FinishWrite(destination), true);
        if (LineLoadIsIncomplete() && !LoadLine(ref source))
            return (source.Position, 0, false);
        return (source.Position, TryWriteCurrentRow(destination), false);
    }

    private bool LineLoadIsIncomplete() => currentReadPos < Lines.LineLength;

    private bool LoadLine(ref SequenceReader<byte> source)
    {
        for (; currentReadPos < Lines.CurrentLine.Length; currentReadPos++)
        {
            if (!BitReader.TryRead(1, ref source, out var pixel)) return false;
            Lines.CurrentLine[currentReadPos] = Parameters.IsWhiteValue(pixel);
        }
        return true;
    }
    
    protected bool DoneEncodingLine() => a0 >= Lines.LineLength;


    private void SetCurrentLineAsUnEncoded() => a0 = -1;

    private void SetCurrentLineAsUnread() => currentReadPos = 0;
    
    private void TryByteAlignEncodedOutput(ref CcittBitWriter encoding)
    {
        if (Parameters.EncodedByteAlign) encoding.PadUnusedBits();
    }
    protected void TryResetForNextLine(ref CcittBitWriter writer)
    {
        if (writer.HasRoomToWrite(3)) ResetForNextLine(ref writer);
    }
    protected virtual void ResetForNextLine(ref CcittBitWriter writer)
    {
        Lines = Lines.SwapLines();
        SetCurrentLineAsUnEncoded();
        SetCurrentLineAsUnread();
        TryByteAlignEncodedOutput(ref writer);
    }
    protected abstract int TryWriteCurrentRow(Span<byte> destination);
}

public class CcittType4Encoder : CcittEncoderBase
{
   
    public CcittType4Encoder(in CcittParameters parameters): base(parameters)
    {
    }
    
    protected override int TryWriteCurrentRow(Span<byte> destination)
    {
        var writer = new CcittBitWriter(destination, BitWriter);
        while (!DoneEncodingLine() && writer.HasRoomToWrite() && TryWriteRun(ref writer))
        {
            // do nothing, the TryWriteRun call does the work as long as it can
        }

        if (DoneEncodingLine()) TryResetForNextLine(ref writer);

        return writer.BytesWritten;
    }
    
    private bool TryWriteRun(ref CcittBitWriter writer)
    {
        var comparison = Lines.CompareLinesFrom(a0);
        if (comparison.CanPassEncode)
        {
            writer.WritePass();
            a0 = comparison.B2;
        }
        else if (comparison.CanVerticalEncode)
        {
            writer.WriteVertical(comparison.VerticalEncodingDelta);
            a0 = comparison.A1;
        }
        else
        {
            if (!writer.WriteHorizontal(Lines.ImputedCurrentColor(a0),
                    comparison.FirstHorizontalDelta(a0), comparison.SecondHorizontalDelta)) return false;
            a0 = comparison.A2;
        }
        return true;
    }
}