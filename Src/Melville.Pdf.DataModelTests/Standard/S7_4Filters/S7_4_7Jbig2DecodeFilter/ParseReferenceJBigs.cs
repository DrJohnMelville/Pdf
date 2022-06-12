using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class ParseReferenceJBigs
{
    private static async Task<BinaryBitmap> LoadBitmapAsync(string name)
    {
        var fact = new JbigExplicitPageReader();
        fact.RequestPage(1);
        await fact.ProcessFileBitsAsync(JBigSampleStreams.Get(name)!);
        var bitmap = fact.GetPage(1);
        return bitmap;
    }

    [Theory]
    [InlineData("042_12","042_1")]
    public async Task CompareFiles(string fileA, string fileB)
    {
        Assert.Equal((await LoadBitmapAsync(fileA)).BitmapString(), (await LoadBitmapAsync(fileB)).BitmapString());
    }
}