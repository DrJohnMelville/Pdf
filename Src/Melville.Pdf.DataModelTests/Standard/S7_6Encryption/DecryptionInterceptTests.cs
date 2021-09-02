using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.Decryptors;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public class DecryptionInterceptTests
    {
        [Fact]
        public async Task LoadDecodedString()
        {
            var source = new MemoryStream("(AbCd)".AsExtendedAsciiBytes());
            var reader = new ParsingReader(null!, PipeReader.Create(source), 0,
                new DecryptorFake());
            Assert.Equal("abcd", (await new PdfCompositeObjectParser().ParseAsync(reader)).ToString());
            
        }
        [Fact]
        public async Task LoadDecodedStream()
        {
            var source = new MemoryStream("<</Length 4>> stream\r\nABCD endstream".AsExtendedAsciiBytes());
            var reader = new ParsingReader(new ParsingFileOwner(new MemoryStream()), PipeReader.Create(source), 0,
                new DecryptorFake());
            var str = (PdfStream)await new PdfCompositeObjectParser().ParseAsync(reader);
            var output = await new StreamReader(await str.GetEncodedStreamAsync()).ReadToEndAsync();
            Assert.Equal("Decoded", output);
        }
        
        private class DecryptorFake: IDecryptor
        {
            public void DecryptStringInPlace(in Span<byte> input)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    input[i] |= 0x20;
                } 
            }

            public Stream WrapRawStream(Stream input) => new MemoryStream("Decoded".AsExtendedAsciiBytes());
        }

    }
}