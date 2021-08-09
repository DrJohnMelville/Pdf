using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters
{
    public class S7_4_4LzwAndFlateDecodeFilter
    {
        [Fact]
        public Task FlateDecodeStreamRoundTrip() =>
            StreamTest.Encoding(new PdfArray(KnownNames.ASCII85Decode, KnownNames.FlateDecode), null, 
                "Hello World.", "GhV^Zc,n(/#gY0H8^RV?!!*'!!rs<A\"A8~>");
        [Fact]
        public Task LZWDecodeStreamRoundTrip() =>
            StreamTest.Encoding(new PdfArray(KnownNames.ASCIIHexDecode, KnownNames.LZWDecode), null, 
                "-----A---B", "800B6050220C0C8501");

        [Theory]
        [InlineData(100)]
        [InlineData(499)]
        public async Task EncodeRandomStream(int length)
        {
            var buffer = new byte[length];
            var rnd = new Random(10);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte) rnd.Next(256);
            }

            var creator = new LowLevelDocumentCreator();
            var str = creator.NewCompressedStream(buffer, KnownNames.LZWDecode);
            var destination = new byte[length];
            var decoded = await str.GetDecodedStream();
            await decoded.FillBufferAsync(destination, 0, length);
            Assert.Equal(buffer, destination);
            
        }
    }
}