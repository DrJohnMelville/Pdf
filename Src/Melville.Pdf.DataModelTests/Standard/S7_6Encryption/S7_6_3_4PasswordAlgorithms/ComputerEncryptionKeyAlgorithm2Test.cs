using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Encryption;
using Melville.Pdf.LowLevel.Encryption.Readers;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms
{
    public class ComputerEncryptionKeyAlgorithm2Test
    {
        private string Render(byte[] item)
        {
            var ret = new StringBuilder();
            ret.Append("<");
            foreach (var b in item)
            {
                ret.Append(b.ToString("X2"));
            }
            ret.Append(">");
            return ret.ToString();
        }

        [Theory]
        [InlineData("()", "<28BF4E5E4E758A4164004E56FFFA01082E2E00B6D0683E802F0CA9FE6453697A>")]
        [InlineData("(A)", "<4128BF4E5E4E758A4164004E56FFFA01082E2E00B6D0683E802F0CA9FE645369>")]
        [InlineData("(AB)", "<414228BF4E5E4E758A4164004E56FFFA01082E2E00B6D0683E802F0CA9FE6453>")]
        [InlineData("(ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz)", 
            "<4142434445464748494A4B4C4D4E4F505152535455565758595A616263646566>")]
        public async Task PadBytes(string source, string result)
        {
            var ret = BytePadder.Pad(await source.PdfStringBytesAsync());
            Assert.Equal(result, Render(ret));
            
        }
    }

    public static class QuickStringParser
    {
        public static async ValueTask<byte[]> PdfStringBytesAsync(this string s) =>
            ((PdfString)await s.ParseObjectAsync()).Bytes;
    }
}