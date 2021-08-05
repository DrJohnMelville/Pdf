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
            private int inputPosition;
            private EncoderDictionary dictionary;

            public LzwEncodingContext(byte[] input, Stream output)
            {
                this.input = input;
                this.output = new BitWriter(PipeWriter.Create(output));
                inputPosition = 0;
                dictionary = new EncoderDictionary();
            }

            public async ValueTask Encode()
            {
                #warning check if the initial output code is needed
                await output.WriteBits(LZWConstants.ClearDictionaryCode, 9);
                await output.FinishWrite();
            }
        }
    }
}