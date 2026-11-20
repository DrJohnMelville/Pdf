using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public partial class ColorOperationsParserTest: ParserTest
{
    private partial class StrokeColorMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        public void SetStrokeColor(in ReadOnlySpan<double> color)
        {
            Assert.SeqEqual(new double[]{1,2,3}, color.ToArray());
            SetCalled();
            
        }
    }

    [Test]
    public Task SetColorAsync() => TestInputAsync("1 2 3 SC", new StrokeColorMock());
    
    private partial class NonStrokeColorMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        public void SetNonstrokingColor(in ReadOnlySpan<double> color)
        {
            Assert.SeqEqual(new double[]{1,2,3}, color.ToArray());
            SetCalled();
            
        }
    }

    [Test]
    public Task SetNonstrokinColorAsync() => TestInputAsync("1 2 3 sc", new NonStrokeColorMock());

    private partial class StrokeColorExtendedMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        private PdfDirectObject? expectedName;

        public StrokeColorExtendedMock(PdfDirectObject? expectedName)
        {
            this.expectedName = expectedName;
        }

        public ValueTask SetStrokeColorExtendedAsync(PdfDirectObject? patternName, in ReadOnlySpan<double> colors)
        {
            Assert.Equal(expectedName, patternName);
            Assert.SeqEqual(new double[]{1,2,3}, colors.ToArray());
            SetCalled();
            return ValueTask.CompletedTask;
        }
    }
    [Test]
    public Task SetStrokingExtended1Async() => TestInputAsync("1 2 3 SCN", 
        new StrokeColorExtendedMock(null));
    [Test]
    public Task SetStrokingExtended2Async() => TestInputAsync("1 2 3 /P1 SCN", 
        new StrokeColorExtendedMock(PdfDirectObject.CreateName("P1")));

    private partial class NonStrokeColorExtendedMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        private PdfDirectObject? expectedName;

        public NonStrokeColorExtendedMock(PdfDirectObject? expectedName)
        {
            this.expectedName = expectedName;
        }

        public ValueTask SetNonstrokingColorExtendedAsync(PdfDirectObject? patternName, in ReadOnlySpan<double> colors)
        {
            Assert.Equal(expectedName, patternName);
            Assert.SeqEqual(new double[]{1,2,3}, colors.ToArray());
            SetCalled();
            return ValueTask.CompletedTask;
        }
    }
    [Test]
    public Task SetNonStrokingExtended1Async() => TestInputAsync("1 2 3 scn", 
        new NonStrokeColorExtendedMock(null));
    [Test]
    public Task SetNonStrokingExtended2Async() => TestInputAsync("1 2 3 /P1 scn", 
        new NonStrokeColorExtendedMock(PdfDirectObject.CreateName("P1")));
    
    [Test]
    public Task SetStrokeGrayAsync() => TestInputAsync("12 G", i => i.SetStrokeGrayAsync(12));
    [Test]
    public Task SetStrokeRGBAsync() => TestInputAsync("4 5 6 RG", i => i.SetStrokeRGBAsync(4,5, 6));
    [Test]
    public Task SetStrokeCMYKAsync() => TestInputAsync("4 5 6 7 K", i => i.SetStrokeCMYKAsync(4,5, 6, 7));
    [Test]
    public Task SetNonstrokingGrayAsync() => TestInputAsync("12 g", i => i.SetNonstrokingGrayAsync(12));
    [Test]
    public Task SetNonstrokingRGBAsync() => TestInputAsync("4 5 6 rg", i => i.SetNonstrokingRgbAsync(4,5, 6));
    [Test]
    public Task SetNonstrokingCMYKAsync() => TestInputAsync("4 5 6 7 k", i => i.SetNonstrokingCMYKAsync(4,5, 6, 7));
    [Test]
    public Task StrokingColorSpaceAsync() => 
        TestInputAsync("/DeviceGray CS", i => i.SetStrokingColorSpaceAsync(ColorSpaceName.DeviceGray));
    [Test]
    public Task NonstrokingColorSpaceAsync() => 
        TestInputAsync("/DeviceGray cs", i => i.SetNonstrokingColorSpaceAsync(ColorSpaceName.DeviceGray));

}