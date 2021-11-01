using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_9CommonDataStructures;

public class S7_9_5Rectangles
{
    [Fact]
    public async Task SimpleRectangle()
    {
        var rect = await PdfRect.CreateAsync(new PdfArray(
            new PdfInteger(1), new PdfInteger(2), new PdfInteger(3), new PdfInteger(4)));
        Assert.Equal(1, rect.Left);
        Assert.Equal(2, rect.Bottom);
        Assert.Equal(3, rect.Right);
        Assert.Equal(4, rect.Top);
    }
}