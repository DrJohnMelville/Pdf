using System;
using System.Threading.Tasks;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;
using Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public class S7_6_5CryptFilters
    {
        [Fact]
        public async Task CanUseDefaultStringsIn()
        {
            var src = new EncryptedV4StreamsPlain();
            var str = await src.AsString();
            Assert.Contains(str, "plaintext stream");
        }
    }
}