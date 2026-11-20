using System;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

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

    [Test]
    public Task ParseInlineImageDictionaryAsync()=>
        TestInputAsync($"BI/Width 12/Height 24\nID\nxxEI",
            new DoImpl(async i =>
            {
                Assert.Equal(4, i.Count);
                Assert.Equal(KnownNames.XObject, await i[KnownNames.Type]);
                Assert.Equal(KnownNames.Image, await i[KnownNames.Subtype]);
                Assert.Equal(12, await i.GetAsync<int>(KnownNames.Width));
                Assert.Equal(24, await i.GetAsync<int>(KnownNames.Height)
                );
            }));

    [Test]
    [Arguments("StreamData", "StreamData")]
    [Arguments("StreamEI!Data", "StreamEI!Data")]
    [Arguments("StreamEIData!", "StreamEIData!")]
    [Arguments("StreamEI Data!", "StreamEI Data!")]
    [Arguments("StreamEI\rData!", "StreamEI\rData!")]
    [Arguments("StreamEI\nData!", "StreamEI\nData!")]
    [Arguments("StreamEI\tData!", "StreamEI\tData!")]
    public Task ParseSimpleInlineImageAsync(string onDisk, string parsed) =>
        TestInputAsync($"BI/Width 12/Height 24\nID\n{onDisk}EI",
            new DoImpl(async i => Assert.Equal(parsed,
                await (await i.ReaderForContentAsync()).ReadAsStringAsync())));

    [Test]
    [Arguments("ASCIIHexDecode", "/AHx")]
    [Arguments("ASCII85Decode", "/A85")]
    [Arguments("LZWDecode", "/LZW")]
    [Arguments("FlateDecode", "/Fl")]
    [Arguments("RunLengthDecode", "/RL")]
    [Arguments("CCITTFaxDecode", "/CCF")]
    [Arguments("DCTDecode", "/DCT")]
    [Arguments("DeviceGray", "/G")]
    [Arguments("DeviceRGB", "/RGB")]
    [Arguments("DeviceCMYK", "/CMYK")]
    [Arguments("Indexed", "/I")]
    public Task ParseImageSynonymsAsync(string preferredTerm, string synonym) =>
        TestInputAsync($"BI/Filter {synonym}\nID\nStreamDataEI",
            new DoImpl(async i =>
            {
                Assert.Equal(PdfDirectObject.CreateName(preferredTerm), 
                    await i[KnownNames.Filter]);

            }));

    [Test]
    [Arguments("BitsPerComponent", "BPC")]
    [Arguments("ColorSpace", "CS")]
    [Arguments("Decode", "D")]
    [Arguments("DecodeParms", "DP")]
    [Arguments("Filter", "F")]
    [Arguments("Height", "H")]
    [Arguments("ImageMask", "IM")]
    [Arguments("Interpolate", "I")]
    [Arguments("Width", "W")]
    public Task ParseImageEntrySynonymsAsync(string preferredTerm, string synonym) =>
        TestInputAsync($"BI/{synonym} 1234\nID\nStreamDataEI",
            new DoImpl(async i =>
            {
                Assert.Equal(1234,
                        (await i.GetAsync<int>(PdfDirectObject.CreateName(preferredTerm))));

            }));

    [Test]
    public Task WithArrayArgumentAsync() =>
                  TestInputAsync(
            "BI/D[/AHx/DCT]ID\nHelloEI",
            new DoImpl(async i =>
            {
                Assert.True((await i.GetAsync<PdfArray>(KnownNames.Decode)) is PdfArray);
            }));
}