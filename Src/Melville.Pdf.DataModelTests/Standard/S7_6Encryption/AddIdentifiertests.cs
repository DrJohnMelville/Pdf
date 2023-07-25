using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Postscript.Interpreter.Values;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption;

public class AddIdentifiertests
{
    [Fact]
    public void EnsureBuilderHasIdentifierTest()
    {
        var builder = new LowLevelDocumentBuilder();
        Assert.False(builder.CreateDocument().TrailerDictionary.TryGetValue(KnownNames.IDTName, out _));
        builder.EnsureDocumentHasId();
        Assert.True(builder.CreateDocument().TrailerDictionary.TryGetValue(KnownNames.IDTName, out var idObj));
    }
    [Fact]
    public async Task DoNotAddIfAlreadyIdentifiedAsync()
    {
        var builder = new LowLevelDocumentBuilder();
        builder.EnsureDocumentHasId();
        Assert.True(builder.CreateDocument().TrailerDictionary.TryGetValue(KnownNames.IDTName, out var idObj));
        builder.EnsureDocumentHasId();
        Assert.True(builder.CreateDocument().TrailerDictionary.TryGetValue(KnownNames.IDTName, out var idObj2));
        Assert.Same(await idObj, await idObj2);
    }
    [Fact]
    public async Task IDHas2ProperStringElementsAsync()
    {
        var builder = new LowLevelDocumentBuilder();
        builder.EnsureDocumentHasId();
        var ary = await builder.CreateDocument().TrailerDictionary.GetAsync<PdfValueArray>(KnownNames.IDTName);
        Assert.Equal(2, ary.Count);
        await VerifyIdMemberAsync(ary, 0);
        await VerifyIdMemberAsync(ary, 1);
    }

    private static async Task VerifyIdMemberAsync(PdfValueArray ary, int index)
    {
        var str1 = (await ary[index]).Get<StringSpanSource>();
        Assert.Equal(32, str1.GetSpan().Length);
    }
}