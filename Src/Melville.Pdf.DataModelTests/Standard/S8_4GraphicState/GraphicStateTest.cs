using System;
using System.Numerics;
using Melville.Hacks.Reflection;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateTest
{
    private readonly GraphicsState sut = new();
    private readonly GraphicsState sut2 = new();
    
    private void PropTest<T>(string name, T defaultValue, T newValue, Action<IStateChangingOperations> act)
    {
        // set value
        Assert.Equal(defaultValue, (T)sut.GetProperty(name)!);
        act(sut);
        Assert.Equal(newValue, (T)sut.GetProperty(name)!);

        // copy Value
        Assert.Equal(defaultValue, (T)sut2.GetProperty(name)!);
        sut2.CopyFrom(sut);
        Assert.Equal(newValue, (T)sut2.GetProperty(name)!);
    }

    [Fact] public void LineWidthTest() => PropTest(nameof(sut.LineWidth), 1.0, 5.0, i => i.SetLineWidth(5));
    [Fact] public void MiterLimitTest() => PropTest(nameof(sut.MiterLimit), 10.0, 5.0, i => i.SetMiterLimit(5));
    [Fact] public void LineJoinTest() => PropTest(nameof(sut.LineJoinStyle), 
        LineJoinStyle.Miter, LineJoinStyle.Bevel, i=>i.SetLineJoinStyle(LineJoinStyle.Bevel));
    [Fact] public void LineCapTest() => PropTest(nameof(sut.LineCap), LineCap.Square, LineCap.Round,
        i=>i.SetLineCap(LineCap.Round));

    [Fact]
    public void ModifyTransformMatrix()
    {
        Assert.Equal(new Vector2(0,0), sut.ApplyCurrentTransform(new Vector2(0,0)));
        sut.ModifyTransformMatrix(Matrix3x2.CreateTranslation(-1,-2));
        Assert.Equal(new Vector2(0,0), sut.ApplyCurrentTransform(new Vector2(1,2)));
        sut.ModifyTransformMatrix(Matrix3x2.CreateScale(0.1f, 0.01f));
        Assert.Equal(new Vector2(0,0), sut.ApplyCurrentTransform(new Vector2(10, 200)));
    }
} 