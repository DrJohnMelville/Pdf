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
        TestInput("/M1 /M2 DP", i => i.MarkedContentPoint("M1", "M2"));
    [Fact]
    public Task BeginMarkedRangeWithTag() =>
        TestInput("/M1 BMC", i => i.BeginMarkedRange("M1"));
    [Fact]
    public Task BeginMarkedRangeWithTagAndDictName() =>
        TestInput("/M1 /M2 BDC", i => i.BeginMarkedRange("M1", "M2"));
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

        public void MarkedContentPoint(PdfName tag, in UnparsedDictionary dict)
        {
            Assert.Equal("/M1", tag.ToString());
            Assert.Equal(expected, StringFromDict(dict));
            SetCalled();
        }

        public void BeginMarkedRange(PdfName tag, in UnparsedDictionary dictionary) =>
            MarkedContentPoint(tag, dictionary);

        private static string StringFromDict(UnparsedDictionary dict) => 
            ExtendedAsciiEncoding.ExtendedAsciiString(dict.Text.Span);
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