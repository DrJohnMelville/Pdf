using System;
using System.Numerics;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Payloads;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_9CommonDataStructures;

public class S7_9_5Rectangles
{
    [Fact]
    public async Task SimpleRectangleAsync()
    {
        var rect = await PdfRect.CreateAsync(new PdfArray(
            1, 2, 3, 4));
        Assert.Equal(1, rect.Left);
        Assert.Equal(2, rect.Bottom);
        Assert.Equal(3, rect.Right);
        Assert.Equal(4, rect.Top);
    }

    [Fact]
    public void BoundedTransformTest()
    {
        var rect = new PdfRect(0, 0, 3, 4);
        var transform = Matrix3x2.CreateTranslation(-1.5f, -2.0f) * 
                        Matrix3x2.CreateRotation((float)(Math.PI/4)) * 
                        Matrix3x2.CreateTranslation(2.5f,2.5f);
        var final = rect.BoundTransformedRect(transform);
        final.Left.Should().BeApproximately(0, 0.1);
        final.Bottom.Should().BeApproximately(0, 0.1);
        final.Right.Should().BeApproximately(5, 0.1);
        final.Top.Should().BeApproximately(5, 0.1);
    }

    [Theory]
    [InlineData(0,0,1,1, 10,20,30,40)]
    public void TransformToTest(
        double x1, double y1, double x2, double y2,
        double x3, double y3, double x4, double y4)
    {
        var final = new PdfRect(x3,y3, x4, y4);
        var xForm = new PdfRect(x1, y1, x2, y2).TransformTo(
            final);
        var bl = xForm.Transform(new Vector2((float)x1, (float)y1));
        var tr = xForm.Transform(new Vector2((float)x2, (float)y2));
        bl.X.Should().BeApproximately((float)final.Left, 0.1f);
        bl.Y.Should().BeApproximately((float)final.Bottom, 0.1f);
        tr.X.Should().BeApproximately((float)final.Right, 0.1f);
        tr.Y.Should().BeApproximately((float)final.Top, 0.1f);
    }

}