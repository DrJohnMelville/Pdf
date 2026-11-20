using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_6_ArraysDefined
{
    [Test]
    [Arguments("[549 3.14 false (Ralph) /SomeName]", 5)]
    [Arguments("[]", 0)]
    [Arguments("[.5]", 1)] // this is a bug check leading period should invoke number parser
    [Arguments("[123.5]", 1)]
    [Arguments("[123.5 (this is a string () inside)]", 2)]
    [Arguments("[true false null]", 3)]
    [Arguments("[[true false] null]", 2)]
    [Arguments("[/WIDTH /HGH /X1 /HEIGHT]", 4)]
    public async Task ParseArrayAsync(string src, int length)
    {
        var obj = await (await src.ParseValueObjectAsync()).LoadValueAsync<PdfArray>();
        Assert.Equal(length, obj.RawItems.Count);
    }

    [Test]
    public async Task ReadBigArrayAsync()
    {
        var expected = Enumerable.Range(0, 1000).ToArray();
        var source = $"[{string.Join(" ", expected)}]";
        var parsed = await (await source.ParseValueObjectAsync()).LoadValueAsync();
        var asInts = await parsed.Get<PdfArray>().CastAsync<int>();
        Assert.SeqEqual(expected, asInts);
    }
}