using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType4Encoder : IStreamFilterDefinition
{
    private readonly CcittParameters parameters;
    private readonly BitWriter bitWriter = new();
    private readonly BitReader bitReader = new();
    private CcittLinePair lines;
    private int currentReadPos;
    private int a0;
    
    public CcittType4Encoder(in CcittParameters parameters)
    {
        this.parameters = parameters;
        lines = new CcittLinePair(parameters);
    }

    public int MinWriteSize => 10;

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, ref Span<byte> destination)
    {
        if (source.Length == 0)
            return (source.Position, bitWriter.FinishWrite(destination), true);
        if (LineLoadIsIncomplete() && !LoadLine(ref source))
                return (source.Position, 0, false);
        return TryWriteCurrentRow(ref source, destination);
    }

    private (SequencePosition SourceConsumed, int bytesWritten, bool Done) TryWriteCurrentRow(
        ref SequenceReader<byte> source, Span<byte> destination)
    {
        var writer = new CcittBitWriter(destination, bitWriter);
        while (!DoneEncodingLine() && writer.HasRoomToWrite() && TryWriteRun(ref writer))
        {
            // do nothing, the TryWriteRun call does the work as long as it can
        }

        TryResetForNextLine(ref writer);

        return (source.Position, writer.BytesWritten, false);
    }
    
    private bool TryWriteRun(ref CcittBitWriter writer)
    {
        var comparison = lines.CompareLinesFrom(a0);
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
            if (!writer.WriteHorizontal(lines.ImputedCurrentColor(a0),
                    comparison.FirstHorizontalDelta(a0), comparison.SecondHorizontalDelta)) return false;
            a0 = comparison.A2;
        }

        return true;
    }

    private void TryResetForNextLine(ref CcittBitWriter writer)
    {
        if (DoneEncodingLine() && writer.HasRoomToWrite()) ResetForNextLine(ref writer);
    }

    private bool DoneEncodingLine() => a0 >= lines.LineLength;

    private void ResetForNextLine(ref CcittBitWriter encoding)
    {
        lines = lines.SwapLines();
        a0 = -1;
        currentReadPos = 0;
        TryByteAlignEncodedOutput(ref encoding);
    }

    private void TryByteAlignEncodedOutput(ref CcittBitWriter encoding)
    {
        if (parameters.EncodedByteAlign) encoding.PadUnusedBits();
    }

    private bool LineLoadIsIncomplete() => currentReadPos < lines.LineLength;

    private bool LoadLine(ref SequenceReader<byte> source)
    {
        for (; currentReadPos < lines.CurrentLine.Length; currentReadPos++)
        {
            if (!bitReader.TryRead(1, ref source, out var pixel)) return false;
            lines.CurrentLine[currentReadPos] = parameters.IsWhiteValue(pixel);
        }
        a0 = -1;
        return true;
    }
}