using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Writers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_9Images;

public class ParseInlineImage:ParserTest
{  
    
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
public class WriteInllineImage:WriterTest
{
    [Fact]
    public async Task WriteSimpleImage()
    {
         await sut.DoAsync(new DictionaryBuilder()
             .WithItem(KnownNames.Width, 12)
             .WithItem(KnownNames.Height, 24)
             .AsStream("StreamData"));

         Assert.Equal("BI/Width 12/Height 24\nID\nStreamDataEI", await WrittenText());
         
    }
}