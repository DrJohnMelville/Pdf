using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public static class LzwParameterParser
    {
        public static async ValueTask<int> EarlySwitchLength(this PdfObject? parameters)
        {
            return (parameters is PdfDictionary dict &&
                    dict.TryGetValue(KnownNames.EarlyChange, out var ec) &&
                    await ec is PdfNumber num)
                ? (int)num.IntValue
                : 1;
        }

    }
    public class LzwEncoder : IStreamEncoder
    {
        public async ValueTask<Stream> Encode(Stream data, PdfObject? parameters) =>
             new MinimumReadSizeFilter(new 
                LzwEncodeWrapper(PipeReader.Create(data), await parameters.EarlySwitchLength()), 10);
        
        public class LzwEncodeWrapper : ConvertingStream
        {
            private readonly BitWriter output = new BitWriter();
            private readonly EncoderDictionary dictionary = new EncoderDictionary();
            private short currentDictionaryEntry = -1;
            private readonly BitLength bits;

            public LzwEncodeWrapper(PipeReader source, int earlySwitchLength) : base(source)
            {
                bits = new BitLength(9, earlySwitchLength);
            }

            protected override (SequencePosition SourceConsumed, int bytesWritten, bool Done)
                Convert(ref SequenceReader<byte> source, ref Span<byte> destination)
            {
                if (currentDictionaryEntry < 0)
                {
                    if (!source.TryRead(out var firstByte)) return (source.Sequence.Start, 0, false);
                    currentDictionaryEntry = firstByte;
                }

                int destPosition = 0;
                while (HasRoomToWrite(destination, destPosition) && source.TryRead(out var nextByte))
                {
                    if (dictionary.GetOrCreateNode(currentDictionaryEntry, nextByte, out var nextEntry))
                    {
                        currentDictionaryEntry = nextEntry;
                    }
                    else
                    {
                        destPosition += WriteCodeForCurrentChain(
                            destination[destPosition..], nextEntry, nextByte);
                    }
                }

                return (source.Position, destPosition, false);
            }

            private int WriteCodeForCurrentChain(Span<byte> destination, short nextEntry, byte nextByte)
            {
                var len= output.WriteBits(currentDictionaryEntry, bits.Length,
                    destination);
                len  += UpdateEncodingState(nextEntry, destination, len);
                currentDictionaryEntry = nextByte;
                return len;
            }

            private int UpdateEncodingState(short nextEntry, in Span<byte> destination, int destPos) =>
                HitMaximumDictionarySize(nextEntry) ? ResetDictionary(destination, destPos) : CheckBitLength(nextEntry);

            private int CheckBitLength(short nextEntry)
            {
                bits.CheckBitLength(nextEntry);
                return 0;
            }

            private int ResetDictionary(Span<byte> destination, int destPos)
            {
                var len = output.WriteBits(LzwConstants.ClearDictionaryCode, bits.Length,
                    destination[destPos..]);
                dictionary.Reset();
                bits.SetBitLength(9);
                return len;
            }

            private static bool HitMaximumDictionarySize(short nextEntry) =>
                nextEntry >= LzwConstants.MaxTableSize - 2;


            protected override (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
                ref SequenceReader<byte> source,
                ref Span<byte> destination)
            {
                if (!HasRoomToWrite(destination, 2)) return (source.Position, 0, false);
                var len = output.WriteBits(currentDictionaryEntry, bits.Length, destination);
                len += output.WriteBits(LzwConstants.EndOfFileCode, bits.Length, destination[len..]);
                len += output.FinishWrite(destination[len..]);
                return (source.Position, len, true);
            }

            private static bool HasRoomToWrite(Span<byte> destination, int destPosition)
            {
                return destPosition + 4 < destination.Length;
            }
        }
    }
}