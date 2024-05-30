using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.INPC;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.TrueTypeOutlines;

public class GlyphRecorderTest
{
    private readonly GlyphRecorder sut = new(FakeRecorderAllocator.Instance);
    public readonly Mock<ITrueTypePointTarget> target = new();
    
    [Theory]
    [InlineData(1, 2, true, true, true)]
    [InlineData(31, 12, false, true, true)]
    [InlineData(31, 12, true, false, true)]
    [InlineData(31, 12, true, true, false)]
    public void ReplayOnePoint(float x, float y, bool onCurve, bool start, bool end)
    {
        sut.AddPoint(new Vector2(x, y), onCurve, start, end);
        sut.Replay(target.Object);
        target.Verify(i=>i.AddPoint(new Vector2(x, y), onCurve, start, end));
    }

    [Fact]
    public void Replay2Points()
    {
        sut.AddPoint(new Vector2(1,2), true, true, false);
        sut.AddPoint(new Vector2(10,20), true, false, true);

        sut.Replay(target.Object);
        target.Verify(i=>i.AddPoint(new Vector2(1,2), true, true, false));
        target.Verify(i=>i.AddPoint(new Vector2(10,20), true, false, true));
    }

    [Fact]
    public void Replay150Points ()
    {
        for (int i = 0; i < 150; i++)
        {
            sut.AddPoint(new Vector2(i, i), true, true, true);
        }
        sut.Replay(target.Object);
        for (int i = 0; i < 150; i++)
        {
            target.Verify(j=>j.AddPoint(new Vector2(i, i), true, true, true));
        }
    }
}

[StaticSingleton]
internal partial class FakeRecorderAllocator: IRecorderAllocator
{
    public CapturedPoint[] Allocate(int size) => new CapturedPoint[size];
    public void Free(CapturedPoint[] data) { }
}