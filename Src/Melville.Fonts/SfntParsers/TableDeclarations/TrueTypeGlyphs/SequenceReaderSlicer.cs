using System.Buffers;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs
{
    internal static class SequenceReaderSlicer{

        public static SequenceReader<T> Slice<T>(
            this SequenceReader<T> reader, int initialPosition, int size) 
            where T: unmanaged, IEquatable<T> =>
            new SequenceReader<T>(reader.UnreadSequence.Slice(initialPosition, size));

        public static SequenceReader<T> Slice<T>(
            this SequenceReader<T> reader, int initialPosition) 
            where T: unmanaged, IEquatable<T> =>
            new SequenceReader<T>(reader.UnreadSequence.Slice(initialPosition));
    }
}