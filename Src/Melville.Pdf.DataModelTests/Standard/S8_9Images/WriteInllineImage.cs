using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_9Images;

public class WriteInllineImage:WriterTest
{
    [Fact]
    public async Task WriteSimpleImageAsync()
    {
         await sut.DoAsync(new ValueDictionaryBuilder()
             .WithItem(KnownNames.WidthTName, 12)
             .WithItem(KnownNames.HeightTName, 24)
             .AsStream("StreamData"));

         Assert.Equal("BI/Width 12/Height 24\nID\nStreamDataEI", await WrittenTextAsync());
         
    }
}