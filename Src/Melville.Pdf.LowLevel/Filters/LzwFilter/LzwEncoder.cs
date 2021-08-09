using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class LzwEncoder: IEncoder
    {
        public byte[] Encode(byte[] data, PdfObject? parameters)
        {
            var output = new MemoryStream();
            var writer = new LzwEncodingContext(data, output);
            writer.Encode().GetAwaiter().GetResult();
            return output.ToArray();
        }
        
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
                #warning check if the initial output code is needed
                await output.WriteBits(LzwConstants.ClearDictionaryCode, bits.Length);
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
                            bits = bits.CheckBitLength(nextEntry)
                                ;                            currentDictionaryEntry = input[i];
                        }
                    }
                    await output.WriteBits(currentDictionaryEntry, bits.Length);
                }
                await output.WriteBits(LzwConstants.EndOfFileCode, bits.Length);
                await output.FinishWrite();
            }
        }
    }
}