using Melville.Parsing.StreamFilters;
using Melville.Parsing.VariableBitEncoding;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Buffers;

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

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        Convert(ref input, ref output, true);
    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        Convert(ref input, ref output, false);
    bool Convert(ref SpanParsingBuffer input, ref Span<byte> output, bool doneReading)
    {
        if (input.CurrentLength == 0 && doneReading)
        {
            if (bitWriter.FinishWrite(output) is > 0 and var delta)
                output = output[delta..];
            return true;
        }

        while (LineLoadIsIComplete() || LoadLine(ref input))
        {
            TryWriteCurrentRow(ref output);
        }

        return false;
    }

    private bool LineLoadIsIComplete() => currentReadPos >= lines.LineLength;

    private bool LoadLine(ref SpanParsingBuffer source)
    {
        for (; !LineLoadIsIComplete(); currentReadPos++)
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

    private void TryWriteCurrentRow(ref Span<byte> destination)
    {
        var writer = new CcittBitWriter(destination, bitWriter);
        if (!TryWriteLinePrefix(ref writer)) return;
        while (!DoneEncodingLine() && writer.HasRoomToWrite() && TryWriteRun(ref writer))
        {
            // do nothing
        }

        if (DoneEncodingLine() && writer.HasRoomToWrite(3))
        {
            WriteLineSuffix(ref writer);
            ResetForNextLine(ref writer);
        }

        destination = destination[writer.BytesWritten..];
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