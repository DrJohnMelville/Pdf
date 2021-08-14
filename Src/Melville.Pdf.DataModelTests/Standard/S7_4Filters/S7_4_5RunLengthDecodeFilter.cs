using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters
{
    public class S7_4_5RunLengthDecodeFilter
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("xyz", "\x2xyz")]
        [InlineData("xy", "\x1xy")]
        [InlineData("x", "\x0x")]
        [InlineData("zzzzzzzzzz", "\xF7z")]
        [InlineData("zzzzzzzzzzxyz", "\xF7z\x2xyz")]
        [InlineData("xyAzzzzzzzzzz", "\x2xyA\xF7z")]
        [InlineData("xyzzzzzzzzzz", "\x1xy\xF7z")]
        [InlineData("xzzzzzzzzzz", "\x0x\xF7z")]
        public Task EncodeString(string plain, string encoded) => 
            StreamTest.Encoding(KnownNames.RunLengthDecode, null, plain, encoded);

        [Fact]
        public Task LongRun() => EncodeString(string.Join("", Enumerable.Repeat('z', 138)), "\x81z\xf7z");

        [Fact]
        public Task LongUnique() =>
            EncodeString(
                string.Join("", Enumerable.Repeat("xy", 67)), "\x7fxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxy\x05xyxyxy");
    }
}