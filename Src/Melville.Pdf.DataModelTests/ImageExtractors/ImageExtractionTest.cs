using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.ImageExtractor;
using Melville.Pdf.ReferenceDocuments.Graphics.Images;
using Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Xunit;

namespace Melville.Pdf.DataModelTests.ImageExtractors;

#warning -- come back to here when stream and reader are rentable.
public class ImageExtractionTest//: RentalPolicyTestBase
{
    private static async Task<IList<IExtractedBitmap>> ImagesFromAsync(CreatePdfParser docGenerator)
    {
        using var document = await docGenerator.AsDocumentRendererAsync();

        var images = await document.ImagesFromAsync(1);
        return images;
    }

    [Fact]
    public async Task ReadImageFromFileAsync()
    {
        var images = await ImagesFromAsync(new ExplicitColorMask());
        Assert.Single(images);
        var image = images[0];
        Assert.Equal(1, image.Page);
        Assert.Equal(256, image.Height);
        Assert.Equal(256, image.Width);
        Assert.Equal(new Vector2(20, 176), image.PositionBottomLeft);
        Assert.Equal(new Vector2(270, 176), image.PositionBottomRight);
        Assert.Equal(new Vector2(20, 26), image.PositionTopLeft);
        Assert.Equal(new Vector2(270, 26), image.PositionTopRight);
    }
    [Fact]
    public async Task ReadImageFromFile2Async()
    {
        var images = await ImagesFromAsync(new JBigSampleBitStream1());
        Assert.Single(images);
        var image = images[0];
        Assert.Equal(1, image.Page);
        Assert.Equal(56, image.Height);
        Assert.Equal(64, image.Width);
        Assert.Equal(new Vector2(20, 176), image.PositionBottomLeft);
        Assert.Equal(new Vector2(270, 176), image.PositionBottomRight);
        Assert.Equal(new Vector2(20, 26), image.PositionTopLeft);
        Assert.Equal(new Vector2(270, 26), image.PositionTopRight);
    }


}