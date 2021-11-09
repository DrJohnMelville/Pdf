using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S14_6MarkedContent;

public partial class MarkedContentParser : ParserTest
{
    [Fact]
    public Task MarkedContentPoint() =>
        TestInput("/M1 MP", i => i.MarkedContentPoint(NameDirectory.Get("M1")));
    [Fact]
    public Task MarkedContentPointWithNamedParam() =>
        TestInput("/M1 /M2 DP", i => i.MarkedContentPointAsync("M1", "M2"));
    [Fact]
    public Task BeginMarkedRangeWithTag() =>
        TestInput("/M1 BMC", i => i.BeginMarkedRange("M1"));
    [Fact]
    public Task BeginMarkedRangeWithTagAndDictName() =>
        TestInput("/M1 /M2 BDC", i => i.BeginMarkedRangeAsync("M1", "M2"));
    [Fact]
    public Task EndMarkedRange() =>
        TestInput("EMC", i => i.EndMarkedRange());

    private partial class MarkedContentPointMock: MockBase, IContentStreamOperations 
    {
        [DelegateTo()] private readonly IContentStreamOperations fake = null!;
        private readonly string expected;

        public MarkedContentPointMock(string expected)
        {
            this.expected = expected;
        }

        public ValueTask MarkedContentPointAsync(PdfName tag, PdfDictionary dict)
        {
            Assert.Equal("/M1", tag.ToString());
            Assert.Single(dict);
            
            SetCalled();
            return ValueTask.CompletedTask;

        }

        public ValueTask BeginMarkedRangeAsync(PdfName tag, PdfDictionary dictionary)
        {
            MarkedContentPointAsync(tag, dictionary);
            return ValueTask.CompletedTask;
        }
    }
    
    [Theory]
    [InlineData("<</Type/Catalog>>")]
    [InlineData("<</Type<</Type/Catalog>>>>")]
    public Task MarkedContentPointWithInlineDict(string dictStr) => 
        TestInput($"/M1{dictStr}DP", new MarkedContentPointMock(dictStr));
    [Theory]
    [InlineData("<</Type/Catalog>>")]
    [InlineData("<</Type<</Type/Catalog>>>>")]
    public Task MarkedContentRangeWithInlineDict(string dictStr) => 
        TestInput($"/M1{dictStr}BDC", new MarkedContentPointMock(dictStr));

}