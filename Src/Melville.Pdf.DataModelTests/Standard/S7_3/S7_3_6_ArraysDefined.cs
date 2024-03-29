﻿using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_6_ArraysDefined
{
    [Theory]
    [InlineData("[549 3.14 false (Ralph) /SomeName]", 5)]
    [InlineData("[]", 0)]
    [InlineData("[.5]", 1)] // this is a bug check leading period should invoke number parser
    [InlineData("[123.5]", 1)]
    [InlineData("[123.5 (this is a string () inside)]", 2)]
    [InlineData("[true false null]", 3)]
    [InlineData("[[true false] null]", 2)]
    [InlineData("[/WIDTH /HGH /X1 /HEIGHT]", 4)]
    public async Task ParseArrayAsync(string src, int length)
    {
        var obj = await (await src.ParseValueObjectAsync()).LoadValueAsync<PdfArray>();
        Assert.Equal(length, obj.RawItems.Count);
    }

    [Fact]
    public async Task ReadBigArrayAsync()
    {
        var expected = Enumerable.Range(0, 1000).ToArray();
        var source = $"[{string.Join(" ", expected)}]";
        var parsed = await (await source.ParseValueObjectAsync()).LoadValueAsync();
        var asInts = await parsed.Get<PdfArray>().CastAsync<int>();
        Assert.Equal(expected, asInts);
    }
}