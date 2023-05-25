using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption;

public class AddIdentifiertests
{
    [Fact]
    public void EnsureBuilderHasIdentifierTest()
    {
        var builder = new LowLevelDocumentBuilder();
        Assert.False(builder.CreateDocument().TrailerDictionary.TryGetValue(KnownNames.ID, out _));
        builder.EnsureDocumentHasId();
        Assert.True(builder.CreateDocument().TrailerDictionary.TryGetValue(KnownNames.ID, out var idObj));
    }
    [Fact]
    public async Task DoNotAddIfAlreadyIdentifiedAsync()
    {
        var builder = new LowLevelDocumentBuilder();
        builder.EnsureDocumentHasId();
        Assert.True(builder.CreateDocument().TrailerDictionary.TryGetValue(KnownNames.ID, out var idObj));
        builder.EnsureDocumentHasId();
        Assert.True(builder.CreateDocument().TrailerDictionary.TryGetValue(KnownNames.ID, out var idObj2));
        Assert.Same(await idObj, await idObj2);
    }
    [Fact]
    public async Task IDHas2ProperStringElementsAsync()
    {
        var builder = new LowLevelDocumentBuilder();
        builder.EnsureDocumentHasId();
        var ary = await builder.CreateDocument().TrailerDictionary.GetAsync<PdfArray>(KnownNames.ID);
        Assert.Equal(2, ary.Count);
        await VerifyIdMemberAsync(ary, 0);
        await VerifyIdMemberAsync(ary, 1);
    }

    private static async Task VerifyIdMemberAsync(PdfArray ary, int index)
    {
        var str1 = (PdfString) await ary[index];
        Assert.Equal(32, str1.Bytes.Length);
    }
}