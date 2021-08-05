using System;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
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
                "-----A---B", "800B6050220C8501");

        [Fact]
        public void AddlerTest()
        {
            var sut = new Adler32Computer(1);
            sut.AddData(ExtendedAsciiEncoding.AsExtendedAsciiBytes("Wikipedia"));
            Assert.Equal(0x11e60398u, sut.GetHash());
            
        }

        [Fact]
        public void AdlerHandlesMultipleOuterLoops()
        {
            var data = new byte[20000];
            for (int i = 0; i < 20000; i++)
            {
                data[i] = (byte) i;
            }

            var af = new Adler32Computer(1);
            af.AddData(data);
            Assert.Equal(2851790123u, af.GetHash());
            
        }

    }
}