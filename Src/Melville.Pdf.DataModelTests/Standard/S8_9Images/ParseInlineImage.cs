using System;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Objects;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_9Images;

public partial class ParseInlineImage:ParserTest
{
    private partial class DoImpl : MockBase, IContentStreamOperations
    {
        [DelegateTo] private IContentStreamOperations fake = null!;
        private Func<PdfStream, ValueTask> verify;

        public DoImpl(Action<PdfStream> verify) :
            this(s => { verify(s); return ValueTask.CompletedTask;})
        {
        }

        public DoImpl(Func<PdfStream, ValueTask> verify)
        {
            this.verify = verify;
        }

        public ValueTask DoAsync(PdfStream stream)
        {
            this.SetCalled();
            return verify(stream);
        }
    }
    [Fact]
    public Task ParseSimpleInlineImage() =>
        TestInput("BI/Width 12/Height 24\nID\nStreamDataEI",
            new DoImpl(async i =>
            {
                Assert.Equal(2, i.Count);
                Assert.Equal("StreamData", 
                    await (await i.StreamContentAsync()).ReadAsStringAsync() );
                
            }));

    // [Theory]
    // [InlineData("/ASCIIHexDecode", "/AHx")]
    // [InlineData("/ASCII85Decode", "/A85")]
    // [InlineData("/LZWDecode", "/LZW")]
    // [InlineData("/FlateDecode", "/FL")]
    // [InlineData("/RunLengthDecode", "/RL")]
    // [InlineData("/CCITTFaxDecode", "/CCF")]
    // [InlineData("/DCTDecode", "/DCT")]
    // public async Task ParseImageSynonyms(string preferredTerm, string synonm)
    // {
    //     Assert.True(false);
    // }

}