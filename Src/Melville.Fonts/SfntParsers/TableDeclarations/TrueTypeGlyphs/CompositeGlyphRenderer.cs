using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs
{
    internal readonly struct CompositeGlyphRenderer(
        ISubGlyphRenderer subGlyphRenderer,
        ITrueTypePointTarget finalTarget,
        PhantomPoints phantomPoints,
        Matrix3x2 transform)
    {
        private readonly GlyphRecorder scratchRecorder = GlyphRecorderFactory.Shared.Rent();
        public ValueTask DrawAsync(Memory<byte> source)
        {
            var reader = new SpanParsingBuffer(source.Span);
            var flags = (CompositeGlyphFlags)reader.GetUInt16BigEndian();
            var subGlyph = reader.GetUInt16BigEndian();
            var (xOffset, yOffset, parentPoint, childPoint) = ReadOffsets(ref reader, flags);

            var subGlyphTransform = ParseTransform(ref reader, flags, xOffset, yOffset);

            TrySkipInstructions(ref reader, flags);

            var nextGlyphSequence = NextGlyphSequence(flags, reader, source);

            if (parentPoint is 0xFFFF && childPoint is 0xFFFF )

            return DrawUnalignedGlyphAsync(subGlyph, subGlyphTransform, nextGlyphSequence);
            return DrawAlignedGlyphAsync(subGlyph, subGlyphTransform, nextGlyphSequence,
                parentPoint, childPoint);
        }

        private (int xOffset, int yOffset, ushort parentpoint, ushort childpoint) ReadOffsets(
            ref SpanParsingBuffer reader, CompositeGlyphFlags flags)
        {
            int x = 0;
            int y = 0;
            ushort parentPoint = 0xFFFF;
            ushort childPoint = 0xFFFF;
            switch (flags.ArgsAreXYOffsets(), flags.ArgsAreWords() )
            {
                case (true, false):
                    x = reader.GetInt8();
                    y = reader.GetInt8();
                    break;
                case (true, true):
                    x = reader.GetInt16BigEndian();
                    y = reader.GetInt16BigEndian();
                    break;
                case (false, false):
                    parentPoint = reader.GetUInt8();
                    childPoint = reader.GetUInt8();
                    break;
                case (false, true):
                    parentPoint = reader.GetUInt16BigEndian();
                    childPoint = reader.GetUInt16BigEndian();
                    break;
            }

            return (x,y,parentPoint, childPoint);
        }

        private Matrix3x2 ParseTransform(ref SpanParsingBuffer reader, CompositeGlyphFlags flags, int xOffset, int yOffset)
        {
            var scaleTransform = ReadScaleTramsform(ref reader, flags);
            var translateTransform = Matrix3x2.CreateTranslation(xOffset, yOffset);
            return flags.ShouldScaleOffset()?
                    translateTransform * scaleTransform:
                    scaleTransform* translateTransform;
        }

        private Matrix3x2 ReadScaleTramsform(
            ref SpanParsingBuffer reader, CompositeGlyphFlags flags) =>
            flags.ScaleSelector() switch
            {
                CompositeGlyphFlags.WeHaveAScale =>
                    Matrix3x2.CreateScale(reader.ReadF2Dot14()),
                CompositeGlyphFlags.WeHaveAnXAndYScale =>
                    Matrix3x2.CreateScale(reader.ReadF2Dot14(), reader.ReadF2Dot14()),
                CompositeGlyphFlags.WeHaveATwoByTwo =>
                    new Matrix3x2(reader.ReadF2Dot14(), reader.ReadF2Dot14(),
                        reader.ReadF2Dot14(), reader.ReadF2Dot14(), 0, 0),
                _ => Matrix3x2.Identity
            };


        private static Memory<byte> NextGlyphSequence(
            CompositeGlyphFlags flags, SpanParsingBuffer reader, Memory<byte> source) => flags.HasMoreGlyphs() ? source[(int)reader.RelativePosition..] :Memory<byte>.Empty;

        private void TrySkipInstructions(ref SpanParsingBuffer reader, CompositeGlyphFlags flags)
        {
            if (!flags.HasInstructions()) return;
            reader.Advance(reader.GetUInt16BigEndian());
        }


        private async ValueTask DrawUnalignedGlyphAsync(
            ushort subGlyph, Matrix3x2 subGlyphTransform, Memory<byte> nextBytes)
        {
            await subGlyphRenderer.RenderGlyphInFontUnitsAsync(
                subGlyph, scratchRecorder, subGlyphTransform*transform).CA();
            await FinishSubglyphAsync(nextBytes).CA();
        }

        private ValueTask FinishSubglyphAsync(Memory<byte> nextBytes)
        {
            if (nextBytes.Length > 0)
                return DrawAsync(nextBytes);

            FinalizeGlyph();
            return ValueTask.CompletedTask;
        }

        private void FinalizeGlyph()
        {
            scratchRecorder.Replay(finalTarget);
            GlyphRecorderFactory.Shared.Return(scratchRecorder);
            phantomPoints.Draw(finalTarget);
        }

        private async ValueTask DrawAlignedGlyphAsync(
            ushort subGlyph, Matrix3x2 transform, Memory<byte> next, 
            ushort parentIndex, ushort childIndex)
        {
            var childTarget = GlyphRecorderFactory.Shared.Rent();
            await subGlyphRenderer.RenderGlyphInFontUnitsAsync(subGlyph, childTarget, transform).CA();

            var parentPoint = scratchRecorder[parentIndex];
            var childPoint = childTarget[childIndex];
            var correction = Matrix3x2.CreateTranslation(parentPoint.X - childPoint.X,
                parentPoint.Y - childPoint.Y);

            childTarget.Replay(scratchRecorder, correction);
            GlyphRecorderFactory.Shared.Return(childTarget);

            await FinishSubglyphAsync(next).CA();
        }
    }
}