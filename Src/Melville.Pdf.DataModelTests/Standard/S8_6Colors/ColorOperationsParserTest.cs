using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public partial class ColorOperationsParserTest: ParserTest
{
    private partial class StrokeColorMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        public void SetStrokeColor(in ReadOnlySpan<double> color)
        {
            Assert.Equal(new double[]{1,2,3}, color.ToArray());
            SetCalled();
            
        }
    }

    [Fact]
    public Task SetColorAsync() => TestInputAsync("1 2 3 SC", new StrokeColorMock());
    
    private partial class NonStrokeColorMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        public void SetNonstrokingColor(in ReadOnlySpan<double> color)
        {
            Assert.Equal(new double[]{1,2,3}, color.ToArray());
            SetCalled();
            
        }
    }

    [Fact]
    public Task SetNonstrokinColorAsync() => TestInputAsync("1 2 3 sc", new NonStrokeColorMock());

    private partial class StrokeColorExtendedMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        private PdfDirectValue? expectedName;

        public StrokeColorExtendedMock(PdfDirectValue? expectedName)
        {
            this.expectedName = expectedName;
        }

        public ValueTask SetStrokeColorExtendedAsync(PdfDirectValue? patternName, in ReadOnlySpan<double> colors)
        {
            Assert.Equal(expectedName, patternName);
            Assert.Equal(new double[]{1,2,3}, colors.ToArray());
            SetCalled();
            return ValueTask.CompletedTask;
        }
    }
    [Fact]
    public Task SetStrokingExtended1Async() => TestInputAsync("1 2 3 SCN", 
        new StrokeColorExtendedMock(null));
    [Fact]
    public Task SetStrokingExtended2Async() => TestInputAsync("1 2 3 /P1 SCN", 
        new StrokeColorExtendedMock(PdfDirectValue.CreateName("P1")));

    private partial class NonStrokeColorExtendedMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        private PdfDirectValue? expectedName;

        public NonStrokeColorExtendedMock(PdfDirectValue? expectedName)
        {
            this.expectedName = expectedName;
        }

        public ValueTask SetNonstrokingColorExtendedAsync(PdfDirectValue? patternName, in ReadOnlySpan<double> colors)
        {
            Assert.Equal(expectedName, patternName);
            Assert.Equal(new double[]{1,2,3}, colors.ToArray());
            SetCalled();
            return ValueTask.CompletedTask;
        }
    }
    [Fact]
    public Task SetNonStrokingExtended1Async() => TestInputAsync("1 2 3 scn", 
        new NonStrokeColorExtendedMock(null));
    [Fact]
    public Task SetNonStrokingExtended2Async() => TestInputAsync("1 2 3 /P1 scn", 
        new NonStrokeColorExtendedMock(PdfDirectValue.CreateName("P1")));
    
    [Fact]
    public Task SetStrokeGrayAsync() => TestInputAsync("12 G", i => i.SetStrokeGrayAsync(12));
    [Fact]
    public Task SetStrokeRGBAsync() => TestInputAsync("4 5 6 RG", i => i.SetStrokeRGBAsync(4,5, 6));
    [Fact]
    public Task SetStrokeCMYKAsync() => TestInputAsync("4 5 6 7 K", i => i.SetStrokeCMYKAsync(4,5, 6, 7));
    [Fact]
    public Task SetNonstrokingGrayAsync() => TestInputAsync("12 g", i => i.SetNonstrokingGrayAsync(12));
    [Fact]
    public Task SetNonstrokingRGBAsync() => TestInputAsync("4 5 6 rg", i => i.SetNonstrokingRgbAsync(4,5, 6));
    [Fact]
    public Task SetNonstrokingCMYKAsync() => TestInputAsync("4 5 6 7 k", i => i.SetNonstrokingCMYKAsync(4,5, 6, 7));
    [Fact]
    public Task StrokingColorSpaceAsync() => 
        TestInputAsync("/DeviceGray CS", i => i.SetStrokingColorSpaceAsync(ColorSpaceName.DeviceGray));
    [Fact]
    public Task NonstrokingColorSpaceAsync() => 
        TestInputAsync("/DeviceGray cs", i => i.SetNonstrokingColorSpaceAsync(ColorSpaceName.DeviceGray));

}