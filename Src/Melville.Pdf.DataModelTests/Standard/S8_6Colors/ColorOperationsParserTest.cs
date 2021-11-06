using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
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
    public Task SetColor() => TestInput("1 2 3 SC", new StrokeColorMock());
    
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
    public Task SetNonstrokinColor() => TestInput("1 2 3 sc", new NonStrokeColorMock());

    private partial class StrokeColorExtendedMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        private PdfName? expectedName;

        public StrokeColorExtendedMock(PdfName? expectedName)
        {
            this.expectedName = expectedName;
        }

        public void SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
        {
            Assert.Equal(expectedName, patternName);
            Assert.Equal(new double[]{1,2,3}, colors.ToArray());
            SetCalled();
        }
    }
    [Fact]
    public Task SetStrokingExtended1() => TestInput("1 2 3 SCN", 
        new StrokeColorExtendedMock(null));
    [Fact]
    public Task SetStrokingExtended2() => TestInput("1 2 3 /P1 SCN", 
        new StrokeColorExtendedMock(NameDirectory.Get("P1")));

    private partial class NonStrokeColorExtendedMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        private PdfName? expectedName;

        public NonStrokeColorExtendedMock(PdfName? expectedName)
        {
            this.expectedName = expectedName;
        }

        public void SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
        {
            Assert.Equal(expectedName, patternName);
            Assert.Equal(new double[]{1,2,3}, colors.ToArray());
            SetCalled();
        }
    }
    [Fact]
    public Task SetNonStrokingExtended1() => TestInput("1 2 3 scn", 
        new NonStrokeColorExtendedMock(null));
    [Fact]
    public Task SetNonStrokingExtended2() => TestInput("1 2 3 /P1 scn", 
        new NonStrokeColorExtendedMock(NameDirectory.Get("P1")));
    
    [Fact]
    public Task SetStrokeGray() => TestInput("12 G", i => i.SetStrokeGray(12));
    [Fact]
    public Task SetStrokeRGB() => TestInput("4 5 6 RG", i => i.SetStrokeRGB(4,5, 6));
    [Fact]
    public Task SetStrokeCMYK() => TestInput("4 5 6 7 K", i => i.SetStrokeCMYK(4,5, 6, 7));
    [Fact]
    public Task SetNonstrokingGray() => TestInput("12 g", i => i.SetNonstrokingGray(12));
    [Fact]
    public Task SetNonstrokingRGB() => TestInput("4 5 6 rg", i => i.SetNonstrokingRGB(4,5, 6));
    [Fact]
    public Task SetNonstrokingCMYK() => TestInput("4 5 6 7 k", i => i.SetNonstrokingCMYK(4,5, 6, 7));
    [Fact]
    public Task StrokingColorSpace() => 
        TestInput("/DeviceGray CS", i => i.SetStrokingColorSpace(ColorSpaceName.DeviceGray));
    [Fact]
    public Task NonstrokingColorSpace() => 
        TestInput("/DeviceGray cs", i => i.SetNonstrokingColorSpace(ColorSpaceName.DeviceGray));

}