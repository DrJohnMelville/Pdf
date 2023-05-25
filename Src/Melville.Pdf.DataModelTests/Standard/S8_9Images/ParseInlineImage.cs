using System;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_9Images;

public partial class ParseInlineImage : ParserTest
{
    private partial class DoImpl : MockBase, IContentStreamOperations
    {
        [DelegateTo] private IContentStreamOperations fake = null!;
        private Func<PdfStream, ValueTask> verify;

        public DoImpl(Action<PdfStream> verify) :
            this(s =>
            {
                verify(s);
                return ValueTask.CompletedTask;
            })
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
    public Task ParseInlineImageDictionaryAsync()=>
        TestInputAsync($"BI/Width 12/Height 24\nID\nxxEI",
            new DoImpl(async i =>
            {
                Assert.Equal(4, i.Count);
                Assert.Equal(KnownNames.XObject, (await i.GetAsync<PdfName>(KnownNames.Type)));
                Assert.Equal(KnownNames.Image, (await i.GetAsync<PdfName>(KnownNames.Subtype)));
                Assert.Equal(12, (await i.GetAsync<PdfNumber>(KnownNames.Width)).IntValue);
                Assert.Equal(24, (await i.GetAsync<PdfNumber>(KnownNames.Height)).IntValue);
            }));

    [Theory]
    [InlineData("StreamData", "StreamData")]
    [InlineData("StreamEI!Data", "StreamEI!Data")]
    [InlineData("StreamEIData!", "StreamEIData!")]
    [InlineData("StreamEI Data!", "StreamEI Data!")]
    [InlineData("StreamEI\rData!", "StreamEI\rData!")]
    [InlineData("StreamEI\nData!", "StreamEI\nData!")]
    [InlineData("StreamEI\tData!", "StreamEI\tData!")]
    public Task ParseSimpleInlineImageAsync(string onDisk, string parsed) =>
        TestInputAsync($"BI/Width 12/Height 24\nID\n{onDisk}EI",
            new DoImpl(async i => Assert.Equal(parsed,
                await (await i.StreamContentAsync()).ReadAsStringAsync())));

    [Theory]
    [InlineData("ASCIIHexDecode", "/AHx")]
    [InlineData("ASCII85Decode", "/A85")]
    [InlineData("LZWDecode", "/LZW")]
    [InlineData("FlateDecode", "/Fl")]
    [InlineData("RunLengthDecode", "/RL")]
    [InlineData("CCITTFaxDecode", "/CCF")]
    [InlineData("DCTDecode", "/DCT")]
    [InlineData("DeviceGray", "/G")]
    [InlineData("DeviceRGB", "/RGB")]
    [InlineData("DeviceCMYK", "/CMYK")]
    [InlineData("Indexed", "/I")]
    public Task ParseImageSynonymsAsync(string preferredTerm, string synonym) =>
        TestInputAsync($"BI/Filter {synonym}\nID\nStreamDataEI",
            new DoImpl(async i =>
            {
                Assert.Equal(NameDirectory.Get(preferredTerm), 
                    await i[KnownNames.Filter]);

            }));

    [Theory]
    [InlineData("BitsPerComponent", "BPC")]
    [InlineData("ColorSpace", "CS")]
    [InlineData("Decode", "D")]
    [InlineData("DecodeParms", "DP")]
    [InlineData("Filter", "F")]
    [InlineData("Height", "H")]
    [InlineData("ImageMask", "IM")]
    [InlineData("Interpolate", "I")]
    [InlineData("Width", "W")]
    public Task ParseImageEntrySynonymsAsync(string preferredTerm, string synonym) =>
        TestInputAsync($"BI/{synonym} 1234\nID\nStreamDataEI",
            new DoImpl(async i =>
            {
                Assert.Equal(1234,
                        (await i.GetAsync<PdfNumber>(NameDirectory.Get(preferredTerm))).
                        IntValue);

            }));
}