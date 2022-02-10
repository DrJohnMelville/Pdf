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
    private bool[] priorLine;
    private int LineLength => priorLine.Length;
    private bool[] currentLine;
    private int currentReadPos;
    private int a0;
    private int a1;
    private int a2;
    private int b1;
    private int b2;
    
    public CcittType4Encoder(in CcittParameters parameters)
    {
        this.parameters = parameters;
        priorLine = this.parameters.CreateWhiteRow();
        currentLine = new bool[LineLength];
    }

    public int MinWriteSize => 5;

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, ref Span<byte> destination)
    {
        if (HasIncompleteLine())
        {
            if (!LoadLine(ref source)) return (source.Position, 0, false);
        }

        var bytesWritten = 0;
        while (a0 < LineLength)
        {
            a1 = a0 + 1;
            while (a1 < LineLength && IsContinuationBlock(currentLine, a1)) a1++;
            a2 = Math.Min(LineLength, a1 + 1);
            while (a2 < LineLength && IsContinuationBlock(currentLine, a2)) a2++;
            b1 = Math.Min(LineLength, a0 + 1);
            while (b1 < LineLength && (IsContinuationBlock(priorLine, b1) ||
                                       currentLine[a0] == priorLine[b1])) b1++;
            b2 = Math.Min(LineLength, b1 + 1);
            while (b2 < LineLength && IsContinuationBlock(priorLine, b2)) b2++;
            if (Math.Abs(a1 - b1) <= 3)
            {
                bytesWritten += WriteVerticalMode(a1, b1, destination);
                a0 = a1;
            }
            else
            {
                throw new NotImplementedException("Only vertical mode is implemented");
            }
        }
        if (a0 <= LineLength)
        {
            if (bytesWritten >= destination.Length) return (source.Position, bytesWritten, false);
            var bw = ResetForNextLine(destination[bytesWritten..]);
            bytesWritten += bw;
        }
        return (source.Position, bytesWritten, false);
    }

    private int ResetForNextLine(in Span<byte> destination)
    {
        SwapLines();
        a0 = -1;
        currentReadPos = 0;
        return TryByteAlignEncodedOutput(destination);
    }

    private void SwapLines() => (currentLine, priorLine) = (priorLine, currentLine);

    private int TryByteAlignEncodedOutput(in Span<byte> destination) =>
        parameters.EncodedByteAlign ? bitWriter.FinishWrite(destination) : 0;

    private int WriteVerticalMode(int a, int b, in Span<byte> destination) =>
        (a - b) switch
        {
            -3 => bitWriter.WriteBits(0b0000010, 7, destination),
            -2 => bitWriter.WriteBits(0b000010, 6, destination),
            -1 => bitWriter.WriteBits(0b010, 3, destination),
            1 => bitWriter.WriteBits(0b011, 3, destination),
            2 => bitWriter.WriteBits(0b000011, 6, destination),
            3 => bitWriter.WriteBits(0b0000011, 7, destination),
            _ => bitWriter.WriteBits(0b1, 1, destination),
        };

    private bool IsContinuationBlock(bool[] line, int index) => 
        (index <= 0 || line[index] == line[index - 1]);

    private bool HasIncompleteLine() => currentReadPos < LineLength;

    private bool LoadLine(ref SequenceReader<byte> source)
    {
        for (; currentReadPos < currentLine.Length; currentReadPos++)
        {
            if (!bitReader.TryRead(1, ref source, out var pixel)) return false;
            currentLine[currentReadPos] = parameters.IsWhiteValue(pixel == 1);
        }

        a0 = -1;
        return true;
    }
}