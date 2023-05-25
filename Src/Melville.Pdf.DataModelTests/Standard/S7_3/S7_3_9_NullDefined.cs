using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_9_NullDefined
{
    private static Task<PdfObject> ParsedNullAsync()
    {
        return "null ".ParseObjectAsync();
    }

    [Fact]
    public async Task CanParseArEmdAsync()
    {
        Assert.Equal(PdfTokenValues.Null, await "null".ParseObjectAsync());
            
    }

    [Fact]
    public async Task CanParseNullAsync()
    {
        Assert.Equal(PdfTokenValues.Null, await ParsedNullAsync());
            
    }

    [Fact]
    public async Task NullIsASingletonAsync()
    {
        Assert.True(ReferenceEquals(await ParsedNullAsync(), await ParsedNullAsync()));
            
    }
}