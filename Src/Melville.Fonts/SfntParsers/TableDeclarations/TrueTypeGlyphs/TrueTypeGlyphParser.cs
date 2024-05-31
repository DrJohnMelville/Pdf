using System.Buffers;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

internal readonly struct TrueTypeGlyphParser(
    ISubGlyphRenderer innerRenderer,
    ReadOnlySequence<byte> slice,
    ITrueTypePointTarget target,
    Matrix3x2 matrix,
    HorizontalMetric horizontalMetric)
{
    public ValueTask DrawGlyphAsync()
    {
        var reader = new SequenceReader<byte>(slice);
        int numberOfContours = reader.ReadBigEndianInt16();
        PhantomPoints boundingBox = new();
        ReadBoundingBox(ref reader, ref boundingBox);
        if (numberOfContours < 0)
        {
            return new CompositeGlyphRenderer(innerRenderer, target, boundingBox, matrix)
                .DrawAsync(reader.UnreadSequence);
        }

        DrawSimpleGlyph(ref reader, numberOfContours);
        boundingBox.Draw(target);
        return ValueTask.CompletedTask;
    }

    #region Simple Glyph Rendering

    private void DrawSimpleGlyph(ref SequenceReader<byte> reader, int numberOfContours)
    {
        Span<ushort> contourEnds = stackalloc ushort[numberOfContours];
        FieldParser.Read(ref reader, contourEnds);
        SkipInstructions(ref reader);
        DrawPoints(ref reader, contourEnds);
    }

    private void ReadBoundingBox(ref SequenceReader<byte> reader, ref PhantomPoints points)
    {
        Span<short> boundingBox = stackalloc short[4];
        FieldParser.Read(ref reader, boundingBox);
        var leftExtent = boundingBox[0] - horizontalMetric.LeftSideBearing;
        points[0] = TransformPoint(leftExtent, 0);
        points[1] = TransformPoint(leftExtent + horizontalMetric.AdvanceWidth, 0);

        // these next two should eventually reference the vmtx table, but I do not parse that yet
        // so I am just using the bounding box for now.
        points[2] = TransformPoint(0, boundingBox[3]);
        points[3] = TransformPoint(0, boundingBox[1]);
    }

    private Vector2 TransformPoint(int xPos, int yPos) =>
        Vector2.Transform(new Vector2(xPos, yPos), matrix);

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

            xBytesConsumed += Math.Abs(xDelta) * repeat;
            pointsSimulated += repeat;
        }

        return ((int)
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
        (instruction.Check(GlyphFlags.Repeat))
            ? (1 + instructions.ReadBigEndianUint8())
            : 1;

    private void ReportPoint(int xPos, int yPos, bool first, bool isEnd, bool onCurve) =>
        target.AddPoint(TransformPoint(xPos, yPos), onCurve, first, isEnd);

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

    #endregion
}