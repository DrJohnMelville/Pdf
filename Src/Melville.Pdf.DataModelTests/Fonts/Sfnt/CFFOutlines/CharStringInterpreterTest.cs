using System;
using System.Buffers;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.ReferenceDocuments.Utility;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.CFFOutlines;

public partial class MockSpanFilter : ICffGlyphTarget
{
    [FromConstructor][DelegateTo] private readonly ICffGlyphTarget target;

    public void Operator(CharStringOperators opCode, Span<DictValue> stack)
    {

    }

}

public class CharStringInterpreterTest
{
    private readonly Mock<ICffGlyphTarget> target = new();
    private readonly Mock<IGlyphSubroutineExecutor> globalSubrs = new();
    private readonly Mock<IGlyphSubroutineExecutor> localSubrs = new();

    private IFontDictExecutorSelector CreateSel(Mock<IGlyphSubroutineExecutor> ex)
    {
        var ret = new Mock<IFontDictExecutorSelector>();
        ret.Setup(i => i.GetExecutor(It.IsAny<uint>())).Returns(ex.Object);
        return ret.Object;
    }

    private async Task<CffGlyphSource> CreateAsync(string hex)
    {
        var bytes = hex.BitsFromHex();
        byte[] buffer = [00, 01, 01, 01, (byte)(1+bytes.Length),
            ..bytes];
        var source = MultiplexSourceFactory.Create(buffer);
        var index = await new CFFIndexParser(source, 
                new ByteSource(source.ReadPipeFrom(0)))
            .ParseCff1Async();
        return new CffGlyphSource(index, 
            CreateSel(globalSubrs), CreateSel(localSubrs), Matrix3x2.Identity,[]);
    }

    private Task ExecuteInstructionAsync(string code) =>
        ExecuteInstructionAsync(code, Matrix3x2.Identity);
    private async Task ExecuteInstructionAsync(string code, Matrix3x2 mat)
    {
        var sut = await CreateAsync(code);
        await sut.RenderGlyphAsync(0, new MockSpanFilter(target.Object), mat);
    }

    [Fact]
    public async Task RMoveToAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 15");
        target.Verify(i=>i.MoveTo(new Vector2(1, 2)));
    }
    
    [Fact]
    public async Task WithMatrixAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 15", Matrix3x2.CreateScale(5,10));
        target.Verify(i=>i.MoveTo(new Vector2(5, 20)));
    }
    [Fact]
    public async Task RMoveToWithWidthAsync()
    {
        await ExecuteInstructionAsync("1C00FF 1C0001 1C0002 15");
        target.Verify(i=> i.RelativeCharWidth(255));
        target.Verify(i=>i.MoveTo(new Vector2(1, 2)));
    }
    [Fact]
    public async Task HMoveToAsync()
    {
        await ExecuteInstructionAsync("1C0002 16");
        target.Verify(i=>i.MoveTo(new Vector2(2, 0)));
    }
    [Fact]
    public async Task HMoveToWithWidthAsync()
    {
        await ExecuteInstructionAsync("1C00ff 1C0002 16");
        target.Verify(i=>i.RelativeCharWidth(255));
        target.Verify(i=>i.MoveTo(new Vector2(2, 0)));
    }
    [Fact]
    public async Task VMoveToAsync()
    {
        await ExecuteInstructionAsync("1C0002 04");
        target.Verify(i=>i.MoveTo(new Vector2(0, 2)));
    }
    [Fact]
    public async Task VMoveToWithWidthAsync()
    {
        await ExecuteInstructionAsync("1C00ff 1C0002 04");
        target.Verify(i=>i.RelativeCharWidth(255));
        target.Verify(i=>i.MoveTo(new Vector2(0, 2)));
    }

    [Fact]
    public async Task SingleRLineToAsync()
    {
        await ExecuteInstructionAsync("1c0001 1c0002 05");
        target.Verify(i=>i.LineTo(new Vector2(1, 2)));
    }
    [Fact]
    public async Task MultiRLineToAsync()
    {
        await ExecuteInstructionAsync("1c0001 1c0002 1c0003 1c0004 05");
        target.Verify(i=>i.LineTo(new Vector2(1, 2)));
        target.Verify(i=>i.LineTo(new Vector2(4, 6)));
    }

    [Fact]
    public async Task SingleHLineToAsync()
    {
        await ExecuteInstructionAsync("1c0005 06");
        target.Verify(i=>i.LineTo(new Vector2(5,0)));
    }

    [Fact]
    public async Task MultiHLineToOddAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004 1c0005 06");
        target.Verify(i=>i.LineTo(new Vector2(1,0)));
        target.Verify(i=>i.LineTo(new Vector2(1,2)));
        target.Verify(i=>i.LineTo(new Vector2(4,2)));
        target.Verify(i=>i.LineTo(new Vector2(4,6)));
        target.Verify(i=>i.LineTo(new Vector2(9,6)));
    }
    [Fact]
    public async Task MultiHLineToEvenAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004 06");
        target.Verify(i=>i.LineTo(new Vector2(1,0)));
        target.Verify(i=>i.LineTo(new Vector2(1,2)));
        target.Verify(i=>i.LineTo(new Vector2(4,2)));
        target.Verify(i=>i.LineTo(new Vector2(4,6)));
    }
    [Fact]
    public async Task MultiVLineToOddAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004 1c0005 07");
        target.Verify(i=>i.LineTo(new Vector2(0, 1)));
        target.Verify(i=>i.LineTo(new Vector2(2, 1)));
        target.Verify(i=>i.LineTo(new Vector2(2, 4)));
        target.Verify(i=>i.LineTo(new Vector2(6, 4)));
        target.Verify(i=>i.LineTo(new Vector2(6, 9)));
    }
    [Fact]
    public async Task MultiVLineToEvenAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004 07");
        target.Verify(i=>i.LineTo(new Vector2(0, 1)));
        target.Verify(i=>i.LineTo(new Vector2(2, 1)));
        target.Verify(i=>i.LineTo(new Vector2(2, 4)));
        target.Verify(i=>i.LineTo(new Vector2(6, 4)));
    }
    [Fact]
    public async Task MultiRRCurveToAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004 1c0005 1c0006" +
                                      "1C0007 1c0008 1c0009 1C000A 1c000b 1c000c 08");
        target.Verify(i=>i.CurveTo(
            new Vector2(1,2), new Vector2(4,6), new Vector2(9,12)));
        target.Verify(i=>i.CurveTo(
            new Vector2(16,20), new Vector2(25,30), new Vector2(36,42)));
    }
    [Fact]
    public async Task MultiHHCurveToEvenAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004" +
                                      "1C0007 1c0008 1c0009 1C000A 1B");
        target.Verify(i=>i.CurveTo(
            new Vector2(1,0), new Vector2(3,3), new Vector2(7, 3)));
        target.Verify(i=>i.CurveTo(
            new Vector2(14,3), new Vector2(22,12), new Vector2(32,12)));
    }
    [Fact]
    public async Task MultiHHCurveToOddAsync()
    {
        await ExecuteInstructionAsync("1C000B 1C0001 1C0002 1c0003 1c0004" +
                                      "1C0007 1c0008 1c0009 1C000A 1B");
        target.Verify(i=>i.CurveTo(
            new Vector2(1,11), new Vector2(3,14), new Vector2(7, 14)));
        target.Verify(i=>i.CurveTo(
            new Vector2(14,14), new Vector2(22,23), new Vector2(32,23)));
    }
    [Fact]
    public async Task MultiHVCurveToMod4Async()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004" +
                                      "1C0007 1c0008 1c0009 1C000A 1F");
        target.Verify(i=>i.CurveTo(
            new Vector2(1,0), new Vector2(3,3), new Vector2(3,7)));
        target.Verify(i=>i.CurveTo(
            new Vector2(3,14), new Vector2(11, 23), new Vector2(21,23)));
    }
    [Fact]
    public async Task MultiHVCurveToWithTrailerAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004" +
                                      "1C0007 1c0008 1c0009 1C000A 1C000B 1F");
        target.Verify(i=>i.CurveTo(
            new Vector2(1,0), new Vector2(3,3), new Vector2(3,7)));
        target.Verify(i=>i.CurveTo(
            new Vector2(3,14), new Vector2(11, 23), new Vector2(21,34)));
    }
    [Fact]
    public async Task MultiRRCurveLineAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004 1c0005 1c0006" +
                                      "1C0007 1c0008 1c0009 1C000A 1c000b 1c000c " +
                                      "1c000d 1c000e 18");
        target.Verify(i=>i.CurveTo(
            new Vector2(1,2), new Vector2(4,6), new Vector2(9,12)));
        target.Verify(i=>i.CurveTo(
            new Vector2(16,20), new Vector2(25,30), new Vector2(36,42)));
        target.Verify(i=>i.LineTo(new Vector2(49, 56)));
    }

    [Fact]
    public async Task RLineCurveTestAsync()
    {
        await ExecuteInstructionAsync("1c0001 1c0002 1c0003 1c0004 " +
                                      "1c0005 1c0006 1c0007 1c0008 1c0009 1c000A 19");
        target.Verify(i=>i.LineTo(new Vector2(1, 2)));
        target.Verify(i=>i.LineTo(new Vector2(4, 6)));
        target.Verify(i=>i.CurveTo(
            new Vector2(9,12), new Vector2(16,20), new Vector2(25,30)));
    }
    [Fact]
    public async Task MultiVHCurveToMod4Async()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004" +
                                      "1C0007 1c0008 1c0009 1C000A 1E");
        target.Verify(i=>i.CurveTo(
            new Vector2(0,1), new Vector2(2,4), new Vector2(6,4)));
        target.Verify(i=>i.CurveTo(
            new Vector2(13,4), new Vector2(21,13), new Vector2(21,23)));
    }

    [Fact]
    public async Task MultiVVCurveToEvenAsync()
    {
        await ExecuteInstructionAsync("1C0001 1C0002 1c0003 1c0004" +
                                      "1C0007 1c0008 1c0009 1C000A 1A");
        target.Verify(i=>i.CurveTo(
            new Vector2(0, 1), new Vector2(2,4), new Vector2(2, 8)));
        target.Verify(i=>i.CurveTo(
            new Vector2(2,15), new Vector2(10,24), new Vector2(10,34)));
    }
    [Fact]
    public async Task MultiVVCurveToOddAsync()
    {   
        await ExecuteInstructionAsync("1C000B 1C0001 1C0002 1c0003 1c0004" +
                                      "1C0007 1c0008 1c0009 1C000A 1A");
        target.Verify(i=>i.CurveTo(
            new Vector2(11,1), new Vector2(13,4), new Vector2(13, 8)));
        target.Verify(i=>i.CurveTo(
            new Vector2(13,15), new Vector2(21,24), new Vector2(21,34)));
    }

    [Fact]
    public async Task TestSimpleFlexToCurvesAsync()
    {
        await ExecuteInstructionAsync("1C0001 1c0000    1c0000 1c0002    1c0002 1c0000" +
                                      "1c0001 1c0000    1c0000 1cFFFE    1c0001 1c0000" +
                                      "1c0032 0c23");
        target.Verify(i=>i.CurveTo(new Vector2(1,0), new Vector2(1,2), new Vector2(3,2)));
        target.Verify(i=>i.CurveTo(new Vector2(4,2), new Vector2(4,0),new Vector2(5,0)));
    }
    [Fact]
    public async Task TestSimpleFlexToLineAsync()
    {
        await ExecuteInstructionAsync("1C0001 1c0000    1c0000 1c0002    1c0002 1c0000" +
                                      "1c0001 1c0000    1c0000 1cFFFE    1c0001 1c0000" +
                                      "1c0032 0c23", Matrix3x2.CreateScale(0.1f));
        target.Verify(i=>i.LineTo(new Vector2(0.5f, 0)));
    }
    [Fact]
    public async Task TestHFlexToCurvesAsync()
    {
        await ExecuteInstructionAsync("1C0001  1c0000 1c0002    1c0002 " +
                                      "1c0001 1c0000 1c0001 " +
                                      "1c0032 0c22");
        target.Verify(i=>i.CurveTo(new Vector2(1,0), new Vector2(1,2), new Vector2(3,2)));
        target.Verify(i=>i.CurveTo(new Vector2(4,2), new Vector2(4,0),new Vector2(5,0)));
    }
    [Fact]
    public async Task TestHFlex1ToCurvesAsync()
    {
        await ExecuteInstructionAsync("1C0001 1c0000  1c0000 1c0002    1c0002 " +
                                      "1c0001 1c0000 1c0001 " +
                                      "1c0032 0c24");
        target.Verify(i=>i.CurveTo(new Vector2(1,0), new Vector2(1,2), new Vector2(3,2)));
        target.Verify(i=>i.CurveTo(new Vector2(4,2), new Vector2(4,0),new Vector2(5,0)));
    }
    [Fact]
    public async Task TestFlex1ToCurvesAsync()
    {
        await ExecuteInstructionAsync("1C0001 1c0000  1c0000 1c0002    1c0002  1c0000" +
                                      "1c0001 1c0000 1c0000 1cFFFE 1c0001 " +
                                      "1c0032 0c25");
        target.Verify(i=>i.CurveTo(new Vector2(1,0), new Vector2(1,2), new Vector2(3,2)));
        target.Verify(i=>i.CurveTo(new Vector2(4,2), new Vector2(4,0),new Vector2(5,0)));
    }

    [Fact]
    public async Task TestEndCharOperatorAsync()
    {
        await ExecuteInstructionAsync("0E");
        target.Verify(i=>i.EndGlyph());
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task TestMultipleEndCharOperatorAsync()
    {
        await ExecuteInstructionAsync("0E0E");
        target.Verify(i=>i.EndGlyph(),Times.Once);
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task TestEndCharOperatorWithWidthAsync()
    {
        await ExecuteInstructionAsync("1c0005 0E");
        target.Verify(i=>i.RelativeCharWidth(5));
        target.Verify(i=>i.EndGlyph());
        target.VerifyNoOtherCalls();
    }
    
    [Theory]
    [InlineData("1c0001 1c0002 01 0E")]
    [InlineData("1c0001 1c0002 03 0E")]
    [InlineData("1c0001 1c0002 12 0E")]
    [InlineData("1c0001 1c0002 17 0E")]
    [InlineData("1c0001 1c0002 17 13FF 0E")]
    [InlineData("1c0001 1c0002 17 1c0003 1c0004 1c0005 1c0006 1c0007 1c0008 1c0009 1c000A 1c000B 1c000C 1c000D 1c000E 1c000F 1c0010 1c0011 1c0012 13FFFE 0E")]
    [InlineData("1c0001 1c0002 17 14FF 0E")]
    [InlineData("1c0001 1c0002 17 1c0003 1c0004 1c0005 1c0006 1c0007 1c0008 1c0009 1c000A 1c000B 1c000C 1c000D 1c000E 1c000F 1c0010 1c0011 1c0012 14FFFE 0E")]
    public async Task IgnoreHintTestAsync(string code)
    {
        await ExecuteInstructionAsync(code);
        target.Verify(i=>i.EndGlyph());
        target.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("1c0005 1c0001 1c0002 01 0e")]
    [InlineData("1c0005 1c0001 1c0002 03 0e")]
    [InlineData("1c0005 1c0001 1c0002 12 0e")]
    [InlineData("1c0005 1c0001 1c0002 17 0e")]
    [InlineData("1c0005 13 0E")]
    public async Task HintWithWidthTestAsync(string code)
    {
        await ExecuteInstructionAsync(code);
        target.Verify(i=>i.RelativeCharWidth(5));
        target.Verify(i=>i.EndGlyph());
        target.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("1c0004 0c09 0E", 4f)]
    [InlineData("1cFFFE 0c09 0E", 2f)]
    [InlineData("1c0001 1c0002 0c0A 0E", 3f)]
    [InlineData("1c0009 1c0006 0c0B 0E", 3f)]
    [InlineData("1c0008 1c0004 0c0C 0E", 2f)]
    [InlineData("1c0008 1c0004 0c18 0E", 32f)]
    [InlineData("1c0008 0c0E 0E", -8f)]
    [InlineData("1c0009 0c1A 0E", 3f)]
    [InlineData("1c0001 1c0002 0c12 0E", 1f)]
    //and
    [InlineData("1c0000 1c0000 0c03 0E", 0f)]
    [InlineData("1c0005 1c0000 0c03 0E", 0f)]
    [InlineData("1c0000 1c0006 0c03 0E", 0f)]
    [InlineData("1c0004 1c0006 0c03 0E", 1f)]
    //or
    [InlineData("1c0000 1c0000 0c04 0E", 0f)]
    [InlineData("1c0005 1c0000 0c04 0E", 1f)]
    [InlineData("1c0000 1c0006 0c04 0E", 1f)]
    [InlineData("1c0004 1c0006 0c04 0E", 1f)]
    //not
    [InlineData("1c0000 0c05 0E", 1f)]
    [InlineData("1c000A 0c05 0E", 0f)]
    //Equal
    [InlineData("1c0000 1c0000 0c0F 0E", 1f)]
    [InlineData("1c000A 1c0000 0c0F 0E", 0f)]
    //If
    [InlineData("1c0001 1c0002 1c0003 1c0004 0c16 0E", 1f)]
    [InlineData("1c0001 1c0002 1c0004 1c0004 0c16 0E", 1f)]
    [InlineData("1c0001 1c0002 1c0005 1c0004 0c16 0E", 2f)]
    public async Task MathOperationsAsync(string code, float topValue)
    {
        await ExecuteInstructionAsync(code);
        target.Verify(i=>i.RelativeCharWidth(topValue));
        target.Verify(i=>i.EndGlyph());
        target.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("1c0001 1c0002 0c1c 15", 2f, 1f)]
    [InlineData("1c0001 1c0002 1c0002 1c0003 0c1E 15", 2f, 1f)]
    [InlineData("1c0001 1c0002 1c0002 1cFFFF 0c1E 15", 2f, 1f)]
    [InlineData("1c0001 0c1B 15", 1f, 1f)]
    [InlineData("1c0007 1c0005 0C14 1C0006 1C0005 0C15 15", 6f, 7f)]
    public async Task MathOperationsDoubleRetAsync(string code, float bottom, float topValue)
    {
        await ExecuteInstructionAsync(code);
        target.Verify(i=>i.MoveTo(new Vector2(bottom, topValue)));
        target.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task TestRandomAsync()
    {
        await ExecuteInstructionAsync("0c17 0e");
        target.Verify(i=>i.RelativeCharWidth(It.IsAny<float>()));
        target.Verify(i=>i.EndGlyph());
    }
    [Fact]
    public async Task CallSubroutineTestAsync()
    {
        await ExecuteInstructionAsync("1c0002 0A");
        localSubrs.Verify(i=>i.CallAsync(2,
            It.IsAny<Func<ReadOnlySequence<byte>, ValueTask>>()));
    }
    [Fact]
    public async Task CallGlobalSubroutineTestAsync()
    {
        await ExecuteInstructionAsync("1c0002 1D");
        globalSubrs.Verify(i=>i.CallAsync(2,
            It.IsAny<Func<ReadOnlySequence<byte>, ValueTask>>()));
    }
}