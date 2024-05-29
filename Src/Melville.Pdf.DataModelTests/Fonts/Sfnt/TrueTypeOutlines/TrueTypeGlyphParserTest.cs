using System.Buffers;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.Pdf.ReferenceDocuments.Utility;
using Melville.SharpFont.PostScript.Internal;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.TrueTypeOutlines;

public class TrueTypeGlyphParserTest
{
    private readonly Mock<IGlyphSource> glyphSource = new();
    private readonly Mock<ITrueTypePointTarget> target = new();

    private TrueTypeGlyphParser CreateParser(string hex, Matrix3x2 matrix) =>
        new(glyphSource.Object, new ReadOnlySequence<byte>(hex.BitsFromHex()),
            target.Object, matrix,0);

    [Theory]
    [InlineData("0001" + // 1 contour
                "0000 0000 0000 0000" +  // bounding box
                "0001" + // Second point is the end of a contour
                "0005 0000000000" + // 5 instructions
                "01 01" +  // two flags values on the point, long vectors 
                "000A 0000" + //2 x values
                "0014 0001")] // 2 y values
    public async Task DrawGlyphAsync(string source)
    {
        var parser = this.CreateParser(source, Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(10, 20, true, true, false));
        target.Verify(i=>i.AddPoint(10, 21, true, false, true));
    }

    [Fact]
    public Task HasInstructionsToSkipOver() =>
        DrawGlyphAsync("0001" + 
                       "0000 0000 0000 0000" + 
                       "0001" + 
                       "0005 0000000000" + // 5 instructions
                       "01 01" + 
                       "000A 0000" +
                       "0014 0001"
    );

    [Fact]
    public Task UseZeroChangeBit() =>
        DrawGlyphAsync("0001" + 
                       "0000 0000 0000 0000" + 
                       "0001" + 
                       "0000" + 
                       "01 11" +  // use as same value for the second x 
                       "000A " + //1 x values
                       "0014 0001" // 2 yvalues
    );

    [Fact]
    public Task UseShortPositiveVectors() =>
        DrawGlyphAsync("0001" + 
                       "0000 0000 0000 0000" +  
                       "0001" + 
                       "0000" + 
                       "37 37" +  // Short vectors 
                       "0A 00" + //2 x values
                       "14 01" // 2 y values
    );

    [Fact]
    public async Task UseTheMatrixAsync()
    {
        var parser = this.CreateParser("0001" + 
                                       "0000 0000 0000 0000" +
                                       "0001" +
                                       "0005 0000000000" +                                        "01 01" +  // two flags values on the point, long vectors 
                                       "000A 0000" + 
                                       "0014 0001", Matrix3x2.CreateScale(3,2));
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(30, 40, true, true, false));
        target.Verify(i=>i.AddPoint(30, 42, true, false, true));
    }
    [Fact]
    public async Task NegativeShortVectors()
    {
        var parser = this.CreateParser("0001" + 
                                       "0000 0000 0000 0000" +  
                                       "0001" + 
                                       "0000" + 
                                       "37 07" +  // Short vectors 
                                       "0A 02" + //2 x values
                                       "14 01" // 2 y values
                                , Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(10, 20, true, true, false));
        target.Verify(i=>i.AddPoint(8, 19, true, false, true));
    }

    [Fact]
    public async Task ShowBoundingBoxAsync()
    {
        var mat = Matrix3x2.CreateScale(3,2);
        var parser = this.CreateParser("0001" + 
                                       "0001 0002 0003 0004" +
                                       "0001" +
                                       "0005 0000000000" +                                        "01 01" +  // two flags values on the point, long vectors 
                                       "000A 0000" + 
                                       "0014 0001", mat);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.BeginGlyph(0, 1, 2, 3, 4, mat));
        target.Verify(i=>i.EndGlyph(0));
    }

    [Fact]
    public async Task RepeatAFlagAsync()
    {
        var parser = this.CreateParser("0001" +
                                       "0000 0000 0000 0000" +  
                                       "0004" + // fourth point is the end of a contour
                                       "0005 0000000000" +
                                       "01 09 03" +  // two flags values plus 3 repeats
                                       "000A 0000 0000 0000 0000" + //5 x values
                                       "0014 0001 0001 0003 0004", Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(10, 20, true, true, false));
        target.Verify(i=>i.AddPoint(10, 21, true, false, false));
        target.Verify(i=>i.AddPoint(10, 22, true, false, false));
        target.Verify(i=>i.AddPoint(10, 25, true, false, false));
        target.Verify(i=>i.AddPoint(10, 29, true, false, true));
    }
    
    [Fact]
    public async Task TwoContoursAsync()
    {
        var parser = this.CreateParser("0002" + // 2 contour
                                       "0000 0000 0000 0000" +  
                                       "0002 0004" + // 2 endpoints
                                       "0005 0000000000" + // 5 instructions
                                       "01 09 03" +  
                                       "000A 0000 0000 0000 0000" + 
                                       "0014 0001 0001 0003 0004", Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(10, 20, true, true, false));
        target.Verify(i=>i.AddPoint(10, 21, true, false, false));
        target.Verify(i=>i.AddPoint(10, 22, true, false, true));
        target.Verify(i=>i.AddPoint(10, 25, true, true, false));
        target.Verify(i=>i.AddPoint(10, 29, true, false, true));
    }

    [Fact]
    public async Task FirstPointNotOnCurveAsync()
    {
        var parser = this.CreateParser("0002" + // 2 contour
                                       "0000 0000 0000 0000" +  
                                       "0002 0004" + // 2 endpoints
                                       "0000" + 
                                       "00 09 03" +  // 2 points plus 3 repeats 
                                       "000A 0000 0000 0000 0000" +
                                       "0014 0001 0001 0003 0004", Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(10, 20, false, true, false));
        target.Verify(i=>i.AddPoint(10, 21, true, false, false));
        target.Verify(i=>i.AddPoint(10, 22, true, false, true));
        target.Verify(i=>i.AddPoint(10, 25, true, true, false));
        target.Verify(i=>i.AddPoint(10, 29, true, false, true));
    }

}