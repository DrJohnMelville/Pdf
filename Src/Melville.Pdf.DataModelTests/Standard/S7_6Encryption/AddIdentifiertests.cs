using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public class AddIdentifiertests
    {
        [Fact]
        public void EnsureBuilderHasIdentifierTest()
        {
            var builder = new LowLevelDocumentBuilder(1);
            Assert.False(builder.CreateTrailerDictionary().TryGetValue(KnownNames.ID, out _));
            builder.EnsureDocumentHasId();
            Assert.True(builder.CreateTrailerDictionary().TryGetValue(KnownNames.ID, out var idObj));
        }
        [Fact]
        public async Task DoNotAddIfAlreadyIdentified()
        {
            var builder = new LowLevelDocumentBuilder(1);
            builder.EnsureDocumentHasId();
            Assert.True(builder.CreateTrailerDictionary().TryGetValue(KnownNames.ID, out var idObj));
            builder.EnsureDocumentHasId();
            Assert.True(builder.CreateTrailerDictionary().TryGetValue(KnownNames.ID, out var idObj2));
            Assert.Same(await idObj, await idObj2);
        }
        [Fact]
        public async Task IDHas2ProperStringElements()
        {
            var builder = new LowLevelDocumentBuilder(1);
            builder.EnsureDocumentHasId();
            var ary = await builder.CreateTrailerDictionary().GetAsync<PdfArray>(KnownNames.ID);
            Assert.Equal(2, ary.Count);
            await VerifyIdMember(ary, 0);
            await VerifyIdMember(ary, 1);
        }

        private static async Task VerifyIdMember(PdfArray ary, int index)
        {
            var str1 = (PdfString) await ary[index];
            Assert.Equal(32, str1.Bytes.Length);
        }
    }
}