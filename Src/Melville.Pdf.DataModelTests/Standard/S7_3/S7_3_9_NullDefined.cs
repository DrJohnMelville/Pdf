using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_9_NullDefined
{
    private static Task<PdfObject> ParsedNull()
    {
        return "null ".ParseObjectAsync();
    }

    [Fact]
    public async Task CanParseArEmd()
    {
        Assert.Equal(PdfTokenValues.Null, await "null".ParseObjectAsync());
            
    }

    [Fact]
    public async Task CanParseNull()
    {
        Assert.Equal(PdfTokenValues.Null, await ParsedNull());
            
    }

    [Fact]
    public async Task NullIsASingleton()
    {
        Assert.True(ReferenceEquals(await ParsedNull(), await ParsedNull()));
            
    }
}