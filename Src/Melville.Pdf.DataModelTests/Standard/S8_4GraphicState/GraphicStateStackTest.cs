using Melville.Pdf.Model.Renderers.GraphicsStates;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateStackTest
{
    private readonly GraphicsStateStack<TestGraphicsState> stack = new();
    
    [Fact]
    public void PushAndPop()
    {
        Assert.Equal(1.0, stack.StronglyTypedCurrentState().LineWidth);
        stack.StronglyTypedCurrentState().SetLineWidth(2.0);
        Assert.Equal(2.0, stack.StronglyTypedCurrentState().LineWidth);
        stack.SaveGraphicsState();
        Assert.Equal(2.0, stack.StronglyTypedCurrentState().LineWidth);
        stack.StronglyTypedCurrentState().SetLineWidth(5.0);
        Assert.Equal(5.0, stack.StronglyTypedCurrentState().LineWidth);
        stack.RestoreGraphicsState();
        Assert.Equal(2.0, stack.StronglyTypedCurrentState().LineWidth);
    }
    
}