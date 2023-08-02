using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_9Images;

public class WriteInllineImage:WriterTest
{
    [Fact]
    public async Task WriteSimpleImageAsync()
    {
         await sut.DoAsync(new DictionaryBuilder()
             .WithItem(KnownNames.Width, 12)
             .WithItem(KnownNames.Height, 24)
             .AsStream("StreamData"));

         Assert.Equal("BI/Width 12/Height 24\nID\nStreamDataEI", await WrittenTextAsync());
         
    }
}