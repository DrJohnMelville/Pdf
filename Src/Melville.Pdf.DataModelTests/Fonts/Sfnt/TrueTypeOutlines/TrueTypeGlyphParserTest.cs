using System.Buffers;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.Pdf.ReferenceDocuments.Utility;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.TrueTypeOutlines;

public class TrueTypeGlyphParserTest
{
    private readonly Mock<ISubGlyphRenderer> glyphSource = new();
    private readonly Mock<ITrueTypePointTarget> target = new();

    public TrueTypeGlyphParserTest()
    {
        glyphSource.Setup(i => i.RenderGlyphInFontUnitsAsync(It.IsAny<uint>(), It.IsAny<ITrueTypePointTarget>(),
            It.IsAny<Matrix3x2>())).Returns(
            (uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix) =>
            {
                target.AddPoint(new Vector2(glyph, glyph), true, true, false);
                return ValueTask.CompletedTask;
            });

    }

    private TrueTypeGlyphParser<ITrueTypePointTarget> CreateParser(string hex, Matrix3x2 matrix) =>
        new(glyphSource.Object, new ReadOnlySequence<byte>(hex.BitsFromHex()),
            target.Object, matrix, new HorizontalMetric(514, 17));

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

        target.Verify(i=>i.AddPoint(new Vector2(10, 20), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 21), true, false, true));
    }

    [Fact]
    public Task HasInstructionsToSkipOverAsync() =>
        DrawGlyphAsync("0001" + 
                       "0000 0000 0000 0000" + 
                       "0001" + 
                       "0005 0000000000" + // 5 instructions
                       "01 01" + 
                       "000A 0000" +
                       "0014 0001"
    );

    [Fact]
    public Task UseZeroChangeBitAsync() =>
        DrawGlyphAsync("0001" + 
                       "0000 0000 0000 0000" + 
                       "0001" + 
                       "0000" + 
                       "01 11" +  // use as same value for the second x 
                       "000A " + //1 x values
                       "0014 0001" // 2 yvalues
    );

    [Fact]
    public Task UseShortPositiveVectorsAsync() =>
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

        target.Verify(i=>i.AddPoint(new Vector2(30, 40), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(30, 42), true, false, true));
    }
    [Fact]
    public async Task NegativeShortVectorsAsync()
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

        target.Verify(i=>i.AddPoint(new Vector2(10, 20), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(8, 19), true, false, true));
    }

    [Fact]
    public async Task PhantomPointsAsync()
    {
        var parser = this.CreateParser("0001" + 
                                       "0008 000A 000B 000C" +  
                                       "0001" + 
                                       "0000" + 
                                       "37 07" +  // Short vectors 
                                       "0A 02" + //2 x values
                                       "14 01" // 2 y values
                                , Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(new Vector2(10, 20), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(8, 19), true, false, true));

        target.Verify(i=>i.AddPhantomPoint(new Vector2(-9,0)));
        target.Verify(i=>i.AddPhantomPoint(new Vector2(514-9,0)));
        target.Verify(i=>i.AddPhantomPoint(new Vector2(0,0x0C)));
        target.Verify(i=>i.AddPhantomPoint(new Vector2(0,0x0A)));
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

        target.Verify(i=>i.AddPoint(new Vector2(10, 20), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 21), true, false, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 22), true, false, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 25), true, false, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 29), true, false, true));
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

        target.Verify(i=>i.AddPoint(new Vector2(10, 20), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 21), true, false, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 22), true, false, true));
        target.Verify(i=>i.AddPoint(new Vector2(10, 25), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 29), true, false, true));
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

        target.Verify(i=>i.AddPoint(new Vector2(10, 20), false, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 21), true, false, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 22), true, false, true));
        target.Verify(i=>i.AddPoint(new Vector2(10, 25), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(10, 29), true, false, true));
    }

    [Fact]
    public async Task SimpleCompositeParseAsync()
    {
        var parser = this.CreateParser("FFFF" + // -1 countours = compositeGlyph
                                       "0000 0000 0000 0000" + // bounding Box
                                       "0002 0004" +  // flags and the subglyph number
                                       "05 FF"  // x and y of the4 subglyph offser
                                        ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        // simple parsing test
        glyphSource.Verify(i=>i.RenderGlyphInFontUnitsAsync(4, It.IsAny<ITrueTypePointTarget>(), 
            Matrix3x2.CreateTranslation(5,-1)));

        // verify rendering completed by verifying the phantom points
        target.Verify(i=>i.AddPhantomPoint(new Vector2(-17,0)));
        target.Verify(i=>i.AddPhantomPoint(new Vector2(497,0)));
        target.Verify(i=>i.AddPhantomPoint(new Vector2(0,0)), Times.Exactly(2));
    }

    [Fact]
    public async Task CompositeWithWordOffsetsAsync()
    {
        var parser = this.CreateParser("FFFF" + 
                                       "0000 0000 0000 0000" + 
                                       "0003 0004" +  
                                       "0005 FFFF"  // x and y of the4 subglyph offser
                                        ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        // simple parsing test
        glyphSource.Verify(i=>i.RenderGlyphInFontUnitsAsync(4, It.IsAny<ITrueTypePointTarget>(), 
            Matrix3x2.CreateTranslation(5,-1)));

        // verify rendering completed by verifying the phantom points
        target.Verify(i=>i.AddPhantomPoint(new Vector2(-17,0)));
        target.Verify(i=>i.AddPhantomPoint(new Vector2(497,0)));
        target.Verify(i=>i.AddPhantomPoint(new Vector2(0,0)), Times.Exactly(2));
    }
    [Fact]
    public async Task InnerGlyphGetsDrawnAsync()
    {
        var parser = CreateParser("FFFF" + 
                                  "0000 0000 0000 0000" + 
                                  "0002 0004" +  
                                  "05 FF"  // 
                                   ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(new Vector2(4,4), true, true, false));
    }
    [Theory]
    [InlineData("10")]
    [InlineData("00")]
    [InlineData("18")]
    public async Task SimpleScaleWithParentOffdetAsync(string scaleOffsetBytes)
    {
        var parser = this.CreateParser("FFFF" + 
                                       "0000 0000 0000 0000" + 
                                       $"{scaleOffsetBytes}0A 0004" +  
                                       "05 FF" +
                                       "8000" // scale
            ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        glyphSource.Verify(i=>i.RenderGlyphInFontUnitsAsync(4, It.IsAny<ITrueTypePointTarget>(), 
            Matrix3x2.CreateScale(-2)*Matrix3x2.CreateTranslation(5,-1)));

    }
    [Fact]
    public async Task SimpleScaleWithGlyphOffdetAsync()
    {
        var parser = this.CreateParser("FFFF" + 
                                       "0000 0000 0000 0000" + 
                                       "080A 0004" +  
                                       "05 FF" +
                                       "8000" // scale
            ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        glyphSource.Verify(i=>i.RenderGlyphInFontUnitsAsync(4, It.IsAny<ITrueTypePointTarget>(), 
            Matrix3x2.CreateTranslation(5,-1)*Matrix3x2.CreateScale(-2)));

    }
    [Fact]
    public async Task XYScaleAsync()
    {
        var parser = this.CreateParser("FFFF" + 
                                       "0000 0000 0000 0000" + 
                                       "1042 0004" +  
                                       "00 00" +
                                       "8000 7000" // scale
            ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        // simple parsing test
        glyphSource.Verify(i=>i.RenderGlyphInFontUnitsAsync(4, It.IsAny<ITrueTypePointTarget>(), 
            Matrix3x2.CreateScale(-2, 1.75f)));

    }
    [Fact]
    public async Task TwoByTwoScaleAsync()
    {
        var parser = this.CreateParser("FFFF" + 
                                       "0000 0000 0000 0000" + 
                                       "1082 0004" +  
                                       "00 00" +
                                       "8000 7000 0000 8000" // scale
            ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        // simple parsing test
        glyphSource.Verify(i=>i.RenderGlyphInFontUnitsAsync(4, It.IsAny<ITrueTypePointTarget>(), 
            new Matrix3x2(-2, 1.75f, 0, -2, 0, 0)));

    }
    [Fact]
    public async Task Render2GlyphsAsync()
    {
        var parser = CreateParser("FFFF" + 
                                  "0000 0000 0000 0000" + 
                                  "0022 0004" +  
                                  "05 FF" + 
                                  "0002 0004" + // two identical points except repeat flag. 
                                  "05 FF"   
            ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(new Vector2(4,4), true, true, false), Times.Exactly(2));
    }
    [Fact]
    public async Task SkipSomeInstructionsAsync()
    {
        var parser = CreateParser("FFFF" + 
                                  "0000 0000 0000 0000" + 
                                  "0122 0004" +  
                                  "05 FF" + 
                                  "0008 00000000 00000000" + // instructions to skip
                                  "0002 0004" + 
                                  "05 FF"   
            ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(new Vector2(4,4), true, true, false), Times.Exactly(2));
    }

    [Fact]
    public async Task Render2PointAlignedGlyphsAsync()
    {
        var parser = CreateParser("FFFF" + 
                                  "0000 0000 0000 0000" + 
                                  "0022 0004" +  
                                  "05 FF" + 
                                  "0000 0344" + // two different points get unified.
                                  "00 00"   
            ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(new Vector2(4,4), true, true, false), Times.Exactly(2));
    }
    [Fact]
    public async Task Render2PointAlignedGlyphsLongFormAsync()
    {
        var parser = CreateParser("FFFF" + 
                                  "0000 0000 0000 0000" + 
                                  "0022 0004" +  
                                  "05 FF" + 
                                  "0001 0344" + // two different points get unified.
                                  "0000 0000"   
            ,Matrix3x2.Identity);
        await parser.DrawGlyphAsync();

        target.Verify(i=>i.AddPoint(new Vector2(4,4), true, true, false), Times.Exactly(2));
    }
}