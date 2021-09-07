using System.Collections.Immutable;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public class WriteEncrytpedDocumentTest
    {
        [Fact]
        public async Task WriteRC4V3EncryptedDocument()
        {
            var builder = new LowLevelDocumentBuilder(1);
            builder.Add(new PdfArray(new PdfString("Encrypted String")));
            builder.Add(builder.NewStream("This is an encrypted stream"));
            Assert.True(false, "Implement encryption here");
        }

    }
}