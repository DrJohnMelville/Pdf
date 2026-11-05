using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;

namespace Melville.Parsing.StreamFilters;

/// <summary>
/// This is a temporaruy for shimming
/// </summary>
public delegate bool ShimmedDelegate(ref SpanParsingBuffer input, ref Span<byte> output);
/// <summary>
/// This is a temporaruy for shimming
/// </summary>
public delegate T SkinnyDelegate<T>(ref SpanParsingBuffer input);
/// <summary>
/// This is a temporaruy for shimming
/// </summary>
public delegate void SkinnyActionDelegate(ref SpanParsingBuffer input);


/// <summary>
/// This is a temporaruy for shimming
/// </summary>
public delegate void ReverseAction(ref SequenceReader<byte> sr);


#warning get rid of this when we are done with the sequencereader.
/// <summary>
/// This is a temporaruy for shimming
/// </summary>
[Obsolete("This is a shim that needs to go awat")]
public static class SequenceReaderWrap
{
    extension(ref SequenceReader<byte> self)
    {
                /// <summary>
                /// This is a temporaruy for shimming
                /// </summary>
                public (SequencePosition SourceConsumed, int bytesWritten, bool Done) TempBridge(
                    Span<byte> destination, ShimmedDelegate method)
                {
        #warning very inefficient
                    var span = self.UnreadSequence.ToArray();
                    var endDest = destination;
                    var reader = new SpanParsingBuffer(span);
                    var done = method(ref reader, ref endDest);
                    self.Advance(reader.RelativePosition);
                    return (self.UnreadSequence.Start, destination.Length - endDest.Length, done);
                }

        /// <summary>
        /// This is a temporaruy for shimming
        /// </summary>
        public T TempBridge<T>(SkinnyDelegate<T> method)
        {
#warning very inefficient
            var span = self.UnreadSequence.ToArray();
            var reader = new SpanParsingBuffer(span);
            var done = method(ref reader);
            self.Advance(reader.RelativePosition);
            return done;
        }
        /// <summary>
        /// This is a temporaruy for shimming
        /// </summary>
        public void TempBridge(SkinnyActionDelegate method)
        {
#warning very inefficient
            var span = self.UnreadSequence.ToArray();
            var reader = new SpanParsingBuffer(span);
            method(ref reader);
            self.Advance(reader.RelativePosition);
        }
    }

    extension (ref SpanParsingBuffer self)
    {
        public void TempBridge(ReverseAction action)
        {
            var seq = new SequenceReader<byte>(new ReadOnlySequence<byte>(
                self.Peek().ToArray()));
            action(ref seq);
            self.Advance((int)seq.Consumed);
        }
    }
}

