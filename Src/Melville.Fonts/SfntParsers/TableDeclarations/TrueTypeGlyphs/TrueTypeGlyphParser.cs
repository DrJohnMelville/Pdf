using System.Buffers;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

public readonly struct TrueTypeGlyphParser(
    IGlyphSource trueTypeGlyphSource, 
    ReadOnlySequence<byte> slice, 
    ITrueTypePointTarget target,
    Matrix3x2 matrix,
    int level)
{
    public ValueTask DrawGlyphAsync()
    {
        var reader = new SequenceReader<byte>(slice);
        int numberOfContours = reader.ReadBigEndianInt16();
        ReadBoundingBox(ref reader); 
        if (numberOfContours < 0)
        {
            return DrawCompositeGlyphAsync(ref reader);
        }

        DrawSimpleGlyph(ref reader, numberOfContours);
        target.EndGlyph(level);
        return ValueTask.CompletedTask;
    }
    private ValueTask DrawCompositeGlyphAsync(ref SequenceReader<byte> reader)
    {
        while (DrawSingleCompositeGlyph(ref reader)) ;
    }

    private bool DrawSingleCompositeGlyph(ref SequenceReader<byte> reader)
    {
        var flags = (CmpositeGlyphFlags)reader.ReadBigEndianUint16();

    }

    private void DrawSimpleGlyph(ref SequenceReader<byte> reader, int numberOfContours)
    {
        Span<ushort> contourEnds = stackalloc ushort[numberOfContours];
        FieldParser.Read(ref reader, contourEnds);
        SkipInstructions(ref reader);
        DrawPoints(ref reader, contourEnds);
    }

    private void ReadBoundingBox(ref SequenceReader<byte> reader)
    {
        Span<short> points = stackalloc short[4];
        FieldParser.Read(ref reader, points);
        target.BeginGlyph(level, points[0], points[1], points[2], points[3], matrix);
    }

    private static void SkipInstructions(ref SequenceReader<byte> reader) => 
        reader.Advance(reader.ReadBigEndianUint16());

    private void DrawPoints(ref SequenceReader<byte> reader, scoped ReadOnlySpan<ushort> contourEnds)
    {
        // notice that we copy the reader each time below to get three different readers
        var (instructionBytes, xBytes) = CountDataBytes(reader, contourEnds[^1]);
        ExecuteInstructionSequence(reader, 
            reader.Slice(instructionBytes, xBytes), 
            reader.Slice(instructionBytes + xBytes),
            contourEnds);
    }

    private (int instructionBytes, int xBytes) CountDataBytes(
        SequenceReader<byte> reader, ushort contourEnd)
    {
        var initialPosition = reader.Position;
        int pointsSimulated = 0;
        int xBytesConsumed = 0;
        while (pointsSimulated <= contourEnd)
        {
            var flag = ReadFlag(ref reader);
            var xDelta = flag.ComputePointSizeAndSign(GlyphFlags.XShortVector, GlyphFlags.XIsSame);

            var repeat = ReadRepeatCount(ref reader, flag);

            xBytesConsumed += Math.Abs(xDelta)* repeat;
            pointsSimulated += repeat;
        }
        return((int)
            reader.Sequence.Slice(initialPosition, reader.Position).Length, xBytesConsumed);
    }

    private void ExecuteInstructionSequence(
        SequenceReader<byte> instructions, SequenceReader<byte> xs, SequenceReader<byte> ys, 
        ReadOnlySpan<ushort> contourEnds)
    {
        int xPos = 0;
        int yPos = 0;
        int currentContour = 0;
        bool first = true;
        for (int points = 0; points <= contourEnds[^1];)
        {
            var instruction = ReadFlag(ref instructions);
            var xOp = instruction.ComputePointSizeAndSign(
                GlyphFlags.XShortVector, GlyphFlags.XIsSame);
            var yOp = instruction.ComputePointSizeAndSign(
                GlyphFlags.YShortVector, GlyphFlags.YIsSame);
            var repeats = ReadRepeatCount(ref instructions, instruction);

            for (int i = 0; i < repeats; i++)
            {
                var onCurve = instruction.Check(GlyphFlags.OnCurve);
                xPos += GetCoordinate(ref xs, xOp);
                yPos += GetCoordinate(ref ys, yOp);
                bool isEnd = contourEnds[currentContour] == points;
                ReportPoint(xPos, yPos, first, isEnd, onCurve);
                if (isEnd)
                {
                    currentContour++;
                    first = true;
                }
                else
                {
                    first = false;
                }

                points++;
            }
        }
    }

    private static int ReadRepeatCount(ref SequenceReader<byte> instructions, GlyphFlags instruction) =>
        (instruction.Check(GlyphFlags.Repeat)) ?
            (1+instructions.ReadBigEndianUint8()) 
            : 1;

    private void ReportPoint(int xPos, int yPos, bool first, bool isEnd, bool onCurve)
    {
        var final = Vector2.Transform(new Vector2(xPos, yPos), matrix);
        target.AddPoint(final.X, final.Y, onCurve, first, isEnd);
    }

    private static GlyphFlags ReadFlag(ref SequenceReader<byte> reader)
    {
        reader.TryRead(out byte flagByte);
        var flag = (GlyphFlags)flagByte;
        return flag;
    }

    private int GetCoordinate(ref SequenceReader<byte> xs, int xOp) => xOp switch
    {
        0 => 0,
        1 => xs.ReadBigEndianUint8(),
        -1 => -xs.ReadBigEndianUint8(),
        2 => xs.ReadBigEndianInt16(),
        _ => throw new InvalidOperationException("Invalid xOp")
    };

}

internal enum CmpositeGlyphFlags : ushort
{
    Arg1And2AreWords = 0x0001,
    ArgsAreXYValues = 0x0002,
    RoundXYToGrid = 0x0004,
    WeHaveAScale = 0x0008,
    MoreComponents = 0x0020,
    WeHaveAnXAndYScale = 0x0040,
    WeHaveATwoByTwo = 0x0080,
    WeHaveInstructions = 0x0100,
    UseMyMetrics = 0x0200,
    OverlapCompound = 0x0400,
    ScaledComponentOffset = 0x0800,
    UnscaledComponentOffset = 0x1000,
}