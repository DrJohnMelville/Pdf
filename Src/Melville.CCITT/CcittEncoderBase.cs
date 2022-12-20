using System.Buffers;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.VariableBitEncoding;

namespace Melville.CCITT;

internal abstract class CcittEncoderBase : IStreamFilterDefinition
{
    protected readonly CcittParameters Parameters;
    private readonly BitWriter bitWriter = new();
    private readonly BitReader bitReader = new();
    private LinePair lines;
    private int currentReadPos;
    private int a0 = -1;

    protected CcittEncoderBase(CcittParameters parameters)
    {
        Parameters = parameters;
        lines = new LinePair(parameters);
    }

    public int MinWriteSize => 10;

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, in Span<byte> destination)
    {
        if (source.Length == 0)
            return (source.Position, bitWriter.FinishWrite(destination), true);
        if (LineLoadIsIncomplete() && !LoadLine(ref source))
            return (source.Position, 0, false);
        return (source.Position, TryWriteCurrentRow(destination), false);
    }

    private bool LineLoadIsIncomplete() => currentReadPos < lines.LineLength;

    private bool LoadLine(ref SequenceReader<byte> source)
    {
        for (; currentReadPos < lines.CurrentLine.Length; currentReadPos++)
        {
            if (!bitReader.TryRead(1, ref source, out var pixel)) return false;
            lines.CurrentLine[currentReadPos] = Parameters.IsWhiteValue(pixel);
        }
        return true;
    }

    private bool DoneEncodingLine() => a0 >= lines.LineLength;
    
    
    private void TryByteAlignEncodedOutput(ref CcittBitWriter encoding)
    {
        if (Parameters.EncodedByteAlign) encoding.PadUnusedBits();
    }

    private int TryWriteCurrentRow(Span<byte> destination)
    {
        var writer = new CcittBitWriter(destination, bitWriter);
        if (!TryWriteLinePrefix(ref writer)) return writer.BytesWritten;
        while (!DoneEncodingLine() && writer.HasRoomToWrite() && TryWriteRun(ref writer))
        {
            // do nothing
        }

        if (DoneEncodingLine() && writer.HasRoomToWrite(3))
        {
            WriteLineSuffix(ref writer);
            ResetForNextLine(ref writer);
        }
        
        return writer.BytesWritten;
    }

    private bool TryWriteLinePrefix(ref CcittBitWriter writer) =>
        CurrentLineEncoder().TryWriteLinePrefix(ref writer, ref a0, lines);

    private bool TryWriteRun(ref CcittBitWriter writer) =>
        CurrentLineEncoder().TryWriteRun(ref writer, ref a0, lines);

    protected abstract void WriteLineSuffix(ref CcittBitWriter writer);

    private void ResetForNextLine(ref CcittBitWriter writer)
    {
        lines = lines.SwapLines();
        SetCurrentLineAsUnEncoded();
        SetCurrentLineAsUnread();
        TryByteAlignEncodedOutput(ref writer);
    }
    private void SetCurrentLineAsUnEncoded() => a0 = -1;
    private void SetCurrentLineAsUnread() => currentReadPos = 0;

    protected abstract ILineEncoder CurrentLineEncoder();
}