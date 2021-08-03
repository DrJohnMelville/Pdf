using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters
{
    public class S7_4_4LzwAndFlateDecodeFilter
    {
        [Fact]
        public Task WriteEncodedStream() =>
            StreamTest.Encoding(new PdfArray(KnownNames.ASCII85Decode, KnownNames.FlateDecode), null, 
                "Hello World.", "GY.Hsc,n(/#gY0H8^RV?!!*'!");

    }
}