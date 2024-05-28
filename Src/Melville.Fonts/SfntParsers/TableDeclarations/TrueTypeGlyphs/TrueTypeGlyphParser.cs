using System.Buffers;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;


public readonly struct TrueTypeGlyphParser(
    IGlyphSource trueTypeGlyphSource, 
    ReadOnlySequence<byte> slice, 
    ITrueTypePointTarget target,
    Matrix3x2 matrix)
{
    public ValueTask DrawGlyphAsync()
    {
        var reader = new SequenceReader<byte>(slice);
        int numberOfContours = reader.ReadBigEndianInt16();
        if (numberOfContours < 0)
        {
            return DrawCompositeGlyphAsync(reader);
        }

        DrawSimpleGlyph(ref reader, numberOfContours);
        return ValueTask.CompletedTask;
    }
    private ValueTask DrawCompositeGlyphAsync(SequenceReader<byte> reader)
    {

#warning implement composite glyphs
        throw new NotImplementedException("CompositeGlyphs not implemented");
    }

    private void DrawSimpleGlyph(ref SequenceReader<byte> reader, int numberOfContours)
    {
        SkipBoundingBox(ref reader); 
        Span<ushort> contourEnds = stackalloc ushort[numberOfContours];
        FieldParser.Read(ref reader, contourEnds);
        SkipInstructions(ref reader);
        DrawPoints(ref reader, contourEnds);
    }

    private static void SkipBoundingBox(ref SequenceReader<byte> reader) => reader.Advance(8);

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
                xPos += GetCoordinate(ref xs, xOp);
                yPos += GetCoordinate(ref ys, yOp);
                bool isEnd = contourEnds[currentContour] == points;
                ReportPoint(xPos, yPos, first, isEnd, instruction.Check(GlyphFlags.OnCurve));
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
        -1 => -xs.ReadBigEndianInt8(),
        2 => xs.ReadBigEndianInt16(),
        _ => throw new InvalidOperationException("Invalid xOp")
    };

}

[Flags]
public enum GlyphFlags : byte
{
    OnCurve = 0x01,
    XShortVector = 0x02,
    YShortVector = 0x04,
    Repeat = 0x08,
    XIsSame = 0x10,
    YIsSame = 0x20,
    Overlapping = 0x40,
    Reserved = 0x80
}

public static class GlyphFlagOperations
{
    public static bool Check(this GlyphFlags flags, GlyphFlags check) => 
        (flags & check) != 0;
    public static int ComputePointSizeAndSign(this GlyphFlags flag, GlyphFlags shortFlag, GlyphFlags isSameFlag) =>
        (flag.Check(shortFlag), flag.Check(isSameFlag)) switch
        {
            (false, true) => 0,
            (false, false) => 2,
            (true, true) => 1,
            (true, false) => -1,
        };

    public static SequenceReader<T> Slice<T>(
        this SequenceReader<T> reader, int initialPosition, int size) 
        where T: unmanaged, IEquatable<T> =>
        new SequenceReader<T>(reader.UnreadSequence.Slice(initialPosition, size));

    public static SequenceReader<T> Slice<T>(
        this SequenceReader<T> reader, int initialPosition) 
        where T: unmanaged, IEquatable<T> =>
        new SequenceReader<T>(reader.UnreadSequence.Slice(initialPosition));
}