using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs
{
    internal readonly struct CompositeGlyphRenderer(
        ISubGlyphRenderer subGlyphRenderer,
        ITrueTypePointTarget finalTarget,
        PhantomPoints phantomPoints,
        Matrix3x2 transform)
    {
        private readonly GlyphRecorder scratchRecorder = GlyphRecorderFactory.GetRecorder();
        public ValueTask DrawAsync(ReadOnlySequence<byte> source)
        {
            var reader = new SequenceReader<byte>(source);
            var flags = (CompositeGlyphFlags)reader.ReadBigEndianUint16();
            var subGlyph = reader.ReadBigEndianUint16();
            var (xOffset, yOffset, parentPoint, childPoint) = ReadOffsets(ref reader, flags);

            var subGlyphTransform = ParseTransform(ref reader, flags, xOffset, yOffset);

            TrySkipInstructions(ref reader, flags);

            var nextGlyphSequence = NextGlyphSequence(flags, reader);

            if (parentPoint is 0xFFFF && childPoint is 0xFFFF )
            return DrawUnalignedGlyphAsync(subGlyph, subGlyphTransform, nextGlyphSequence);
            return DrawAlignedGlyphAsync(subGlyph, subGlyphTransform, nextGlyphSequence,
                parentPoint, childPoint);
        }

        private (int xOffset, int yOffset, ushort parentpoint, ushort childpoint) ReadOffsets(
            ref SequenceReader<byte> reader, CompositeGlyphFlags flags)
        {
            int x = 0;
            int y = 0;
            ushort parentPoint = 0xFFFF;
            ushort childPoint = 0xFFFF;
            switch (flags.ArgsAreXYOffsets(), flags.ArgsAreWords() )
            {
                case (true, false):
                    x = reader.ReadBigEndianInt8();
                    y = reader.ReadBigEndianInt8();
                    break;
                case (true, true):
                    x = reader.ReadBigEndianInt16();
                    y = reader.ReadBigEndianInt16();
                    break;
                case (false, false):
                    parentPoint = reader.ReadBigEndianUint8();
                    childPoint = reader.ReadBigEndianUint8();
                    break;
                case (false, true):
                    parentPoint = reader.ReadBigEndianUint16();
                    childPoint = reader.ReadBigEndianUint16();
                    break;
            }

            return (x,y,parentPoint, childPoint);
        }

        private Matrix3x2 ParseTransform(ref SequenceReader<byte> reader, CompositeGlyphFlags flags, int xOffset, int yOffset)
        {
            var scaleTransform = ReadScaleTramsform(ref reader, flags);
            var translateTransform = Matrix3x2.CreateTranslation(xOffset, yOffset);
            return flags.ShouldScaleOffset()?
                    translateTransform * scaleTransform:
                    scaleTransform* translateTransform;
        }

        private Matrix3x2 ReadScaleTramsform(
            ref SequenceReader<byte> reader, CompositeGlyphFlags flags) =>
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


        private static ReadOnlySequence<byte> NextGlyphSequence(
            CompositeGlyphFlags flags, SequenceReader<byte> reader)
        {
            return flags.HasMoreGlyphs()?reader.UnreadSequence:
                new ReadOnlySequence<byte>([]);
        }

        private void TrySkipInstructions(ref SequenceReader<byte> reader, CompositeGlyphFlags flags)
        {
            if (!flags.HasInstructions()) return;
            reader.Advance(reader.ReadBigEndianUint16());
        }


        private async ValueTask DrawUnalignedGlyphAsync(
            ushort subGlyph, Matrix3x2 subGlyphTransform, ReadOnlySequence<byte> nextBytes)
        {
            await subGlyphRenderer.RenderGlyphInFontUnitsAsync(
                subGlyph, scratchRecorder, subGlyphTransform*transform).CA();
            await FinishSubglyphAsync(nextBytes).CA();
        }

        private ValueTask FinishSubglyphAsync(ReadOnlySequence<byte> nextBytes)
        {
            if (nextBytes.Length > 0)
                return DrawAsync(nextBytes);

            FinalizeGlyph();
            return ValueTask.CompletedTask;
        }

        private void FinalizeGlyph()
        {
            scratchRecorder.Replay(finalTarget);
            GlyphRecorderFactory.ReturnRecorder(scratchRecorder);
            phantomPoints.Draw(finalTarget);
        }

        private async ValueTask DrawAlignedGlyphAsync(
            ushort subGlyph, Matrix3x2 transform, ReadOnlySequence<byte> next, 
            ushort parentIndex, ushort childIndex)
        {
            var childTarget = GlyphRecorderFactory.GetRecorder();
            await subGlyphRenderer.RenderGlyphInFontUnitsAsync(subGlyph, childTarget, transform).CA();

            var parentPoint = scratchRecorder[parentIndex];
            var childPoint = childTarget[childIndex];
            var correction = Matrix3x2.CreateTranslation(parentPoint.X - childPoint.X,
                parentPoint.Y - childPoint.Y);

            childTarget.Replay(scratchRecorder, correction);
            GlyphRecorderFactory.ReturnRecorder(childTarget);

            await FinishSubglyphAsync(next).CA();
        }
    }
}