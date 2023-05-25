using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Pdf.ImageExtractor;
using Melville.Pdf.ImageExtractor.ImageCollapsing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Payloads;
using Xunit;

namespace Melville.Pdf.DataModelTests.ImageExtractors;

public class ImageCombinationTest
{
    [Fact]
    public ValueTask TestBitmapFunctionAsync()
    {
        var bitmap = new TestBitmap(2, 3, 7, 8, 9, 10, 20);
        Assert.Equal(2, bitmap.Width);
        Assert.Equal(3, bitmap.Height);
        Assert.False(bitmap.DeclaredWithInterpolation);
        Assert.Equal(1, bitmap.Page);
        Assert.Equal(new Vector2(7, 8), bitmap.PositionTopLeft);
        Assert.Equal(new Vector2(9, 8), bitmap.PositionTopRight);
        Assert.Equal(new Vector2(9, 10), bitmap.PositionBottomRight);
        Assert.Equal(new Vector2(7, 10), bitmap.PositionBottomLeft);
        return bitmap.VerifyUniformAsync(2, 3, 20, 21, 22, 23, 24, 25, 26);
    }

    [Fact]
    public Task StitchVerticalAsync()
    {
        var source = new List<IExtractedBitmap>()
        {
            new TestBitmap(2,2, 0,2,10,1, 1),
            new TestBitmap(2,3, 0,1,10,0, 50)
        };
        source.CollapseAdjacentImages();
        Assert.Single(source);
        var final = source.First();
        return final.VerifyUniformAsync(2, 5,
            1, 2,
            3, 4,
            50, 51,
            52, 53,
            54, 55
            ).AsTask();

    }
    [Fact]
    public Task StitchVertical3Async()
    {
        var source = new List<IExtractedBitmap>()
        {
            new TestBitmap(2,2, 0,2,10, 1, 1),
            new TestBitmap(2,3, 0,1,10, 0, 50),
            new TestBitmap(2,1, 0,0,10,-1, 70)
        };
        source.CollapseAdjacentImages();
        Assert.Single(source);
        var final = source.First();
        Assert.Equal(3, ((VerticalBitmapStrip)final).Count);
        return final.VerifyUniformAsync(2, 6,
            1, 2,
            3, 4,
            50, 51,
            52, 53,
            54, 55,
            70, 71
            ).AsTask();
    }
    [Fact]
    public Task StitchVerticalInvertedOrderAsync()
    {
        var source = new List<IExtractedBitmap>()
        {
            new TestBitmap(2,1, 0,300,10,-310, 70),
            new TestBitmap(2,2, 0,320,10, 310, 1),
            new TestBitmap(2,3, 0,310,10, 300, 50),
        };
        source.CollapseAdjacentImages();
        Assert.Single(source);
        var final = source.First();
        Assert.Equal(3, ((VerticalBitmapStrip)final).Count);
        return final.VerifyUniformAsync(2, 6,
            1, 2,
            3, 4,
            50, 51,
            52, 53,
            54, 55,
            70, 71
        ).AsTask();
    }
    [Fact]
    public void DoNotStitchDifferentPixelWidth()
    {
        var source = new List<IExtractedBitmap>()
        {
            new TestBitmap(2,2, 0,2,10,1.2f, 1),
            new TestBitmap(4,3, 0,1,10,0, 50)
        };
        source.CollapseAdjacentImages();
        Assert.Equal(2, source.Count);
    }
    [Fact]
    public void DoNotStitchGreaterThan1Apart()
    {
        var source = new List<IExtractedBitmap>()
        {
            new TestBitmap(2,2, 0,2,10,1.6f, 1),
            new TestBitmap(4,3, 0.4f,1,10,0, 50)
        };
        source.CollapseAdjacentImages();
        Assert.Equal(2, source.Count);
    }

    [Fact]
    public Task StitchHorizontalAsync()
    {
        var source = new List<IExtractedBitmap>()
        {
            new TestBitmap(2,2, 0,2,2,0, 1),
            new TestBitmap(3,2, 2,2,5,0, 51)
        };
        source.CollapseAdjacentImages();
        Assert.Single(source);
        var final = source.First();
        return final.VerifyUniformAsync(5,2,
            1, 2, 51, 52, 53,
            3, 4, 54, 55, 56
        ).AsTask();

    }

}