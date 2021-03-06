using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Xunit;
using PdfDictionary = Melville.Pdf.LowLevel.Model.Objects.PdfDictionary;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_7_DictionaryDefined
{
    [Theory]
    [InlineData("  << /Height 213 /Width 456  >>  ")]
    [InlineData("<</Height 213/Width 456>>")]
    public async Task ParseSimpleDictionary(string input)
    {
        var dict = (PdfDictionary)(await input.ParseObjectAsync());
        Assert.Equal(2, dict.RawItems.Count);
        Assert.Equal(213, ((PdfNumber)dict.RawItems[KnownNames.Height]).IntValue);
        Assert.Equal(456, ((PdfNumber)dict.RawItems[KnownNames.Width]).IntValue);
    }
    [Theory]
    [InlineData("  << >>  ", 0)]
    [InlineData("<</Height 213 /Width 456 /ASPECT null >>", 2)] // PDF Spec  nulls make the entry be ignored
    [InlineData(" << /DICT << /InnerDict 121.22 >>>>", 1)] // dictionary can contain dictionaries
    public async Task SpecialCases(string input, int size)
    {
        var dict = (PdfDictionary)(await input.ParseObjectAsync());
        Assert.Equal(size, dict.RawItems.Count);
    }
    [Theory]
    [InlineData("  <<  213 /Height /Width 456  >>  ")]
    [InlineData("<</Height 213/Width>>")]
    public Task Exceptions(string input) => 
        Assert.ThrowsAsync<PdfParseException>(input.ParseObjectAsync);
}