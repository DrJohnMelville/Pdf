using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters
{
    public ref struct RleEncoderEngine
    {
        private SequenceReader<byte> source;
        private Span<byte> destination;
        private int destPosition;

        public RleEncoderEngine(SequenceReader<byte> source, Span<byte> destination) : this()
        {
            this.source = source;
            this.destination = destination;
            destPosition = 0;
        }

        public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(bool final)
        {
            while (true)
            {
                var (controlByte, length, endOfSpan) = SearchRleSequence();
                if (endOfSpan && !final) return (source.Position, destPosition, false);
                switch (controlByte)
                {
                    case RleConstants.EndOfStream: return (source.Position, destPosition, true);
                    case <RleConstants.EndOfStream:
                        if (!HasRoom(length)) return (source.Position, destPosition, false);
                        WriteLiteralRun(controlByte, length);
                        break;
                    case > RleConstants.EndOfStream:
                        if (!HasRoom(2)) return (source.Position, destPosition, false);
                        WriteRepeatedRun(controlByte, length);
                        break;
                }
            }
        }

        private void WriteRepeatedRun(byte controlByte, int length)
        {
            source.TryPeek(out var repeatedValue);
            destination[destPosition++] = controlByte;
            destination[destPosition++] = repeatedValue;
            source.Advance(length);
        }

        private void WriteLiteralRun(byte controlByte, int length)
        {
            destination[destPosition++] = controlByte;
            source.TryCopyTo(destination.Slice(destPosition, length));
            destPosition += length;
            source.Advance(length);
        }

        private bool HasRoom(int length) => 
            destPosition + length < destination.Length;

        private (byte ControlByte, int Length, bool EndOfReader) SearchRleSequence()
        {
            if (!source.TryPeek(out var first)) return (128, 0, true);
            if (!source.TryPeek(1, out var second)) return (0, 1, true);
            if (!source.TryPeek(2, out var third)) return (1, 2, true);
            return ShouldGenerateRepeatedRun(first, second, third) ? 
                FindRepeatedRun(first) : 
                FindLiteralRun(second, third);
        }

        private (byte ControlByte, int Length, bool EndOfReader) FindLiteralRun(byte second, byte third)
        {
            byte first;
            int len = 1;
            while (true)
            {
                first = second;
                second = third;
                if (len == 128) return ((byte)(len - 1), len, false); 
                if (!source.TryPeek(2 + len, out third)) return ((byte)(1 + len), 2 + len, true);
                if (ShouldGenerateRepeatedRun(first, second, third))
                    return ((byte)(len - 1), len, false);
                len++;
            }
        }

        private static bool ShouldGenerateRepeatedRun(byte first, byte second, byte third) => 
            first == second && second == third;

        private (byte ControlByte, int Length, bool EndOfReader) FindRepeatedRun(byte item)
        {
            int len = 3;
            while (true)
            {
                if (!(source.TryPeek(len, out var newItem)))
                    return RunCode(len, true);
                if (newItem != item)
                    return RunCode(len, false);
                len++;
                if (len == 129)
                    return RunCode(128, false);

            }
        }

        private static (byte, int, bool) RunCode(int len, bool endOfReader) => 
            ((byte)RleConstants.RepeatedRunLength(len), len, endOfReader);
    }
}