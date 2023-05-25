using System.Net.Mime;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_9Images;

public class ImageMaskingTest
{
    private static async Task<byte[]> MaskedBitmapAsync(
        int maskWidth, int maskHeight, byte[] maskBytes,
        int imageWidth, int imageHeight, byte[] imageBytes)
    {
        var maskImage = GrayImageBuilder(maskWidth, maskHeight)
            .AsStream(maskBytes);
        var image = GrayImageBuilder(imageWidth, imageHeight)
            .WithItem(KnownNames.SMask, maskImage)
            .AsStream(imageBytes);

        var result = await (await image.WrapForRenderingAsync(DeviceColor.Black))
            .AsByteArrayAsync();
        return result;
    }

    private static DictionaryBuilder GrayImageBuilder(int width, int height)
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Image)
            .WithItem(KnownNames.Width, width)
            .WithItem(KnownNames.Height, height)
            .WithItem(KnownNames.ColorSpace, KnownNames.DeviceGray)
            .WithItem(KnownNames.BitsPerComponent, 8);
    }


    [Fact]
    public async Task SoftMaskEqualDimensionsAsync()
    {
        var result = await MaskedBitmapAsync(3, 3, new byte[]
        {
            0, 255, 0,
            255, 0, 255,
            0, 255, 128
        }, 3, 3, new byte[]
        {
            10, 20, 30,
            40, 50, 60,
            70, 80, 90
        });

        Assert.Equal(new byte[]
        {
            0, 0, 0, 0, 20, 20, 20, 255, 0, 0, 0, 0,
            40, 40, 40, 255, 0, 0, 0, 0, 60, 60, 60, 255,
            0, 0, 0, 0, 80, 80, 80, 255, 44, 44, 44, 128,
        }, result);
    }
    [Fact]
    public async Task SoftMaskBigImageAsync()
    {
        var result = await MaskedBitmapAsync(3, 3, new byte[]
        {
            0, 255, 0,
            255, 0, 255,
            0, 255, 128
        }, 6, 6, new byte[]
        {
            10,10, 20, 20, 30, 30,
            10,10, 20, 20, 30, 30,
            40,40, 50,50, 60,60,
            40,40, 50,50, 60,60,
            70,70, 80,80, 90,90,
            70,70, 80,80, 90,90,
        });

        Assert.Equal(new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 20, 20, 20, 255,20, 20, 20, 255, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 20, 20, 20, 255,20, 20, 20, 255, 0, 0, 0, 0, 0, 0, 0, 0,
            40, 40, 40, 255,40, 40, 40, 255, 0, 0, 0, 0, 0, 0, 0, 0, 60, 60, 60, 255, 60, 60, 60, 255,
            40, 40, 40, 255,40, 40, 40, 255, 0, 0, 0, 0, 0, 0, 0, 0, 60, 60, 60, 255, 60, 60, 60, 255,
            0, 0, 0, 0,0, 0, 0, 0, 80, 80, 80, 255, 80, 80, 80, 255, 44, 44, 44, 128,44, 44, 44, 128,
            0, 0, 0, 0,0, 0, 0, 0, 80, 80, 80, 255, 80, 80, 80, 255, 44, 44, 44, 128,44, 44, 44, 128,
        }, result);
    }
    [Fact]
    public async Task SoftMaskBigMaskAsync()
    {
        var result = await MaskedBitmapAsync(6,6, new byte[]
        {
            0, 0, 255, 255, 0, 0,
            0, 0, 255, 255, 0, 0,
            255, 255, 0, 0, 255, 255,
            255, 255, 0, 0, 255, 255,
            0, 0, 255, 255, 128, 128,
            0, 0, 255, 255, 128, 128
        }, 3, 3, new byte[]
        {
            10, 20, 30,
            40, 50, 60,
            70, 80, 90
        });

        Assert.Equal(new byte[]
        {
            0, 0, 0, 0, 20, 20, 20, 255, 0, 0, 0, 0,
            40, 40, 40, 255, 0, 0, 0, 0, 60, 60, 60, 255,
            0, 0, 0, 0, 80, 80, 80, 255, 44, 44, 44, 128,
        }, result);
    }
}