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
            var (xOffset, yOffset, parentpoint, childPoint) = ReadOffsets(ref reader, flags);

            var subGlyphTransform = ParseTransform(ref reader, flags, xOffset, yOffset);
            return DrawUnalignedGlyphAsync(subGlyph, subGlyphTransform, new ReadOnlySequence<byte>([]));
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
                default:
                    throw new NotImplementedException("Point alignment not implemented yet");
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


        private async ValueTask DrawUnalignedGlyphAsync(
            ushort subGlyph, Matrix3x2 subGlyphTransform, ReadOnlySequence<byte> readOnlySequence)
        {
            await subGlyphRenderer.RenderGlyphInFontUnits(
                subGlyph, scratchRecorder, subGlyphTransform*transform).CA();
            FinalizeGlyph();
        }

        private void FinalizeGlyph()
        {
            scratchRecorder.Replay(finalTarget);
            GlyphRecorderFactory.ReturnRecorder(scratchRecorder);
            phantomPoints.Draw(finalTarget);
        }
    }
}