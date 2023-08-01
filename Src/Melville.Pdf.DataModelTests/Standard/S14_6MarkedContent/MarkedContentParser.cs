using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S14_6MarkedContent;

public partial class MarkedContentParser : ParserTest
{
    [Fact]
    public Task MarkedContentPointAsync() =>
        TestInputAsync("/M1 MP", i => i.MarkedContentPoint(PdfDirectValue.CreateName("M1")));
    [Fact]
    public Task MarkedContentPointWithNamedParamAsync() =>
        TestInputAsync("/M1 /M2 DP", i => i.MarkedContentPointAsync("/M1", "/M2"));
    [Fact]
    public Task BeginMarkedRangeWithTagAsync() =>
        TestInputAsync("/M1 BMC", i => i.BeginMarkedRange("/M1"));
    [Fact]
    public Task BeginMarkedRangeWithTagAndDictNameAsync() =>
        TestInputAsync("/M1 /M2 BDC", i => i.BeginMarkedRangeAsync("/M1", "/M2"));
    [Fact]
    public Task EndMarkedRangeAsync() =>
        TestInputAsync("EMC", i => i.EndMarkedRange());
    [Fact]
    public Task ArrayInDictionaryTest() =>
        TestInputAsync("/Artifact <</Attached [/Bottom ]/BBox [31.4126 35.5546 95.7888 47.7571 ]/Subtype /Footer /Type /Pagination >>BDC", 
            i => i.BeginMarkedRangeAsync(
                PdfDirectValue.CreateName("Artifact"), It.IsAny<PdfValueDictionary>()));

    private partial class MarkedContentPointMock: MockBase, IContentStreamOperations 
    {
        [DelegateTo()] private readonly IContentStreamOperations fake = null!;
        private readonly string expected;

        public MarkedContentPointMock(string expected)
        {
            this.expected = expected;
        }

        public ValueTask MarkedContentPointAsync(PdfDirectValue tag, PdfValueDictionary dict)
        {
            Assert.Equal("M1", tag.ToString());
            Assert.Single(dict);
            
            SetCalled();
            return ValueTask.CompletedTask;

        }

        public ValueTask BeginMarkedRangeAsync(PdfDirectValue tag, PdfValueDictionary dictionary)
        {
            MarkedContentPointAsync(tag, dictionary);
            return ValueTask.CompletedTask;
        }
    }
    
    [Theory]
    [InlineData("<</Type/Catalog>>")]
    [InlineData("<</Type<</Type/Catalog>>>>")]
    public Task MarkedContentPointWithInlineDictAsync(string dictStr) => 
        TestInputAsync($"/M1{dictStr}DP", new MarkedContentPointMock(dictStr));
    [Theory]
    [InlineData("<</Type/Catalog>>")]
    [InlineData("<</Type<</Type/Catalog>>>>")]
    public Task MarkedContentRangeWithInlineDictAsync(string dictStr) => 
        TestInputAsync($"/M1{dictStr}BDC", new MarkedContentPointMock(dictStr));

}