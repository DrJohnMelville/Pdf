using Melville.Pdf.LowLevel.Encryption.Cryptography;
using Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.Cryptography
{
    public class RC4Test
    {
        [Theory]
        [InlineData("Key","Plaintext", new byte[] { 0xBB, 0xF3, 0x16, 0xE8, 0xD9, 0x40, 0xAF, 0x0A, 0xD3 })]
        [InlineData("Wiki","pedia", new byte[] { 0x10, 0x21, 0xBF, 0x04, 0x20 })]
        [InlineData("Secret","Attack at dawn", new byte[] 
            { 0x45, 0xA0, 0x1F, 0x64, 0x5F, 0xC3, 0x5B, 0x38, 0x35, 0x52, 0x54, 0x4B, 0x9B, 0xF5 })]
        public void TestRct(string key, string plaintext, byte[] ciphertext)
        {
            var xform = new RC4(key.AsExtendedAsciiBytes());
            var ret = plaintext.AsExtendedAsciiBytes();
            xform.TransfromInPlace(ret);
            Assert.Equal(ciphertext, ret);
            
        }
    }
}