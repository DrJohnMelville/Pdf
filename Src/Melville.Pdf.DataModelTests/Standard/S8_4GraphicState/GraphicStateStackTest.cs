using Melville.Pdf.Model.Renderers.GraphicsStates;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateStackTest
{
    private readonly GraphicsStateStack stack = new GraphicsStateStack();
    
    [Fact]
    public void PushAndPop()
    {
        Assert.Equal(1.0, stack.Current().LineWidth);
        stack.SetLineWidth(2.0);
        Assert.Equal(2.0, stack.Current().LineWidth);
        stack.SaveGraphicsState();
        Assert.Equal(2.0, stack.Current().LineWidth);
        stack.SetLineWidth(5.0);
        Assert.Equal(5.0, stack.Current().LineWidth);
        stack.RestoreGraphicsState();
        Assert.Equal(2.0, stack.Current().LineWidth);
    }
    
}