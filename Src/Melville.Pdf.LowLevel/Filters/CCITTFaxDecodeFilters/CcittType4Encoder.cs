using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittType4Encoder : IStreamFilterDefinition
{
    private readonly CcittParameters parameters;
    private readonly BitWriter bitWriter = new();
    private readonly BitReader bitReader = new();
    private CcittLinePair lines;
    private int currentReadPos;
    private int a0;
    private bool nextRunIsWhite = true;
    
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
        {
            return (source.Position, bitWriter.FinishWrite(destination), true);
            
        } 
        if (LineLoadIsIncomplete())
        {
            if (!LoadLine(ref source)) return (source.Position, 0, false);
        }

        var bytesWritten = 0;
        while (a0 < lines.LineLength && bytesWritten < destination.Length)
        {
            var comparison = lines.CompareLinesFrom(a0);
            if (comparison.CanPassEncode)
            {
                bytesWritten += WritePassCode(destination[bytesWritten..]);
                a0 = comparison.B2;
            }
            else if (comparison.CanVerticalEncode)
            {
                bytesWritten += WriteVerticalMode(
                    comparison.VerticalEncodingDelta, destination[bytesWritten..]);
                a0 = comparison.A1;
            }
            else
            {
                var localBytesWritten = WriteHorizontalCode(comparison, destination[bytesWritten..]);
                if (localBytesWritten < 0) break;
                bytesWritten += localBytesWritten;
                a0 = comparison.A2;
            }
        }
        if (a0 <= lines.LineLength)
        {
            if (bytesWritten >= destination.Length) return (source.Position, bytesWritten, false);
            var bw = ResetForNextLine(destination[bytesWritten..]);
            bytesWritten += bw;
        }
        return (source.Position, bytesWritten, false);
    }

    private int WriteHorizontalCode(in CcittLineComparison comparison, in Span<byte> destination)
    {
        #warning -- this does not really work because the bitwriter will have a different state next time arround
        var writer = new BitTarget(destination, bitWriter);
        return HorizontalSpanEncoder.Write(ref writer, this.nextRunIsWhite,
            comparison.A1 - (a0 + 1), comparison.A2 - comparison.A1)
            ? writer.BytesWritten
            : -1;
    }

    private int WritePassCode(Span<byte> span) => bitWriter.WriteBits(0b0001, 4, span);

    private int ResetForNextLine(in Span<byte> destination)
    {
        lines = lines.SwapLines();
        a0 = -1;
        currentReadPos = 0;
        return TryByteAlignEncodedOutput(destination);
    }

    private int TryByteAlignEncodedOutput(in Span<byte> destination) =>
        parameters.EncodedByteAlign ? bitWriter.FinishWrite(destination) : 0;

    private int WriteVerticalMode(int delta, in Span<byte> destination)
    {
        nextRunIsWhite = !nextRunIsWhite;
        return (delta) switch
        {
            -3 => bitWriter.WriteBits(0b0000010, 7, destination),
            -2 => bitWriter.WriteBits(0b000010, 6, destination),
            -1 => bitWriter.WriteBits(0b010, 3, destination),
            1 => bitWriter.WriteBits(0b011, 3, destination),
            2 => bitWriter.WriteBits(0b000011, 6, destination),
            3 => bitWriter.WriteBits(0b0000011, 7, destination),
            _ => bitWriter.WriteBits(0b1, 1, destination),
        };
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