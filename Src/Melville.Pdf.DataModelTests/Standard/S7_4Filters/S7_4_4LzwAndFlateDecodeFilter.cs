using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters
{
    public class S7_4_4LzwAndFlateDecodeFilter
    {
        [Fact]
        public Task FlateDecodeStreamRoundTrip() =>
            StreamTest.Encoding(new PdfArray(KnownNames.ASCII85Decode, KnownNames.FlateDecode), null, 
                "Hello World.", "GhVa[c,n(/#gY0H8^RV?***28~>");
        [Fact]
        public Task LZWDecodeStreamRoundTrip() =>
            StreamTest.Encoding(new PdfArray(KnownNames.ASCIIHexDecode, KnownNames.LZWDecode), null, 
                "-----A---B", "16C0A04418190A02");

        [Theory]
        [InlineData(10, 1)]
        [InlineData(100, 1)]
        [InlineData(499, 1)]
        [InlineData(10000, 1)]
        [InlineData(10000, 0)]
        [InlineData(10000, 2)]
        public async Task EncodeRandomStream(int length, int EarlySwitch)
        {
            var buffer = new byte[length];
            var rnd = new Random(10);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte) rnd.Next(256);
            }

            var creator = new LowLevelDocumentCreator();
            var param = new PdfDictionary(new Dictionary<PdfName, PdfObject>()
            {
                {KnownNames.EarlyChange, new PdfInteger(EarlySwitch)}
            });
            var str = await creator.NewCompressedStream(buffer, KnownNames.LZWDecode,
                EarlySwitch < 2? param:null);
            var destination = new byte[length];
            var decoded = await str.GetDecodedStream();
            await decoded.FillBufferAsync(destination, 0, length);
            Assert.Equal(buffer.Length, destination.Length);

            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.True(buffer[i] == destination[i], $"Position: {i} Expected: {buffer[i]} got {destination[i]}");
            }
        }
    }
}