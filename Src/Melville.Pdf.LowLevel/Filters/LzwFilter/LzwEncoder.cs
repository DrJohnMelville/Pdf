using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class LzwEncoder : IStreamEncoder
    {
        public Stream Encode(Stream data, PdfObject? parameters)
        {
            return new MinimumReadSizeFilter(new LzwEncodeWrapper(PipeReader.Create(data)), 10);
        }
        
        public class LzwEncodeWrapper : ConvertingStream
        {
            private readonly BitWriter2 output = new BitWriter2();
            private readonly EncoderDictionary dictionary = new EncoderDictionary();
            private short currentDictionaryEntry = -1;
            private BitLength bits = new BitLength(9);
            public LzwEncodeWrapper(PipeReader source) : base(source)
            {
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
                    if (dictionary.GetOrCreateNode(
                        currentDictionaryEntry, nextByte, out var nextEntry))
                    {
                        currentDictionaryEntry = nextEntry;
                    }
                    else
                    {
                        destPosition += output.WriteBits(currentDictionaryEntry, bits.Length,
                            destination[destPosition..]);
                        destPosition += CheckBitLength(nextEntry, destination, destPosition);
                        currentDictionaryEntry = nextByte;
                    }
                }

                return (source.Position, destPosition, false);
            }
            
            private int CheckBitLength(short nextEntry, in Span<byte> destination, int destPos)
            {
                if (nextEntry >= LzwConstants.MaxTableSize-2)
                {
                    var len = output.WriteBits(LzwConstants.ClearDictionaryCode, bits.Length,
                        destination[destPos..]);                                      
                    dictionary.Reset();
                    bits = new BitLength(9);
                    return len;
                }else {
                    bits = bits.CheckBitLength(nextEntry);
                    return 0;
                }
            }


            protected override (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(ref SequenceReader<byte> source,
                ref Span<byte> destination)
            {
                if (!HasRoomToWrite(destination,2)) return (source.Position, 0, false);
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

        // public byte[] Encode(byte[] data, PdfObject? parameters)
        // {
        //     var output = new MemoryStream();
        //     var writer = new LzwEncodingContext(data, output);
        //     writer.Encode().GetAwaiter().GetResult();
        //     return output.ToArray();
        // }

        public class LzwEncodingContext
        {
            private readonly BitWriter output;
            private readonly byte[] input;
            private readonly EncoderDictionary dictionary;
            private short currentDictionaryEntry;
            private BitLength bits;

            public LzwEncodingContext(byte[] input, Stream output)
            {
                this.input = input;
                this.output = new BitWriter(PipeWriter.Create(output));
                dictionary = new EncoderDictionary();
                bits = new BitLength(9);
            }

            public async ValueTask Encode()
            {
                if (input.Length > 0)
                {
                    currentDictionaryEntry = input[0];
                    for (int i = 1; i < input.Length; i++)
                    {
                        if (dictionary.GetOrCreateNode(
                            currentDictionaryEntry, input[i], out var nextEntry))
                        {
                            currentDictionaryEntry = nextEntry;
                        }
                        else
                        {
                            await output.WriteBits(currentDictionaryEntry, bits.Length);
                            await CheckBitLength(nextEntry);
                            currentDictionaryEntry = input[i];
                        }
                    }

                    await output.WriteBits(currentDictionaryEntry, bits.Length);
                }

                await output.WriteBits(LzwConstants.EndOfFileCode, bits.Length);
                await output.FinishWrite();
            }

            private async ValueTask CheckBitLength(short nextEntry)
            {
                if (nextEntry >= LzwConstants.MaxTableSize-2)
                {
                    await output.WriteBits(LzwConstants.ClearDictionaryCode, bits.Length);
                    dictionary.Reset();
                    bits = new BitLength(9);
                }else {
                    bits = bits.CheckBitLength(nextEntry);
                }
            }
        }
    }
}