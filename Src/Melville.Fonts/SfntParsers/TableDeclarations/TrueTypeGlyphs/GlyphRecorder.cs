using System.Numerics;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

internal interface IRecorderAllocator
{
    CapturedPoint[] Allocate(int size);
    void Free(CapturedPoint[] data);
}

internal class GlyphRecorder(IRecorderAllocator allocator) : ITrueTypePointTarget
{
    private CapturedPoint[] data = [];
    public int Count { get; private set; } = 0;

    public void BeginGlyph(short minX, short minY, short maxX, short maxY, in Matrix3x2 transform)
    {
    }

    public void AddPoint(Vector2 p, bool onCurve, bool isContourStart, bool isContourEnd) =>
        AppendPoint(p,
            PackFlags(onCurve, isContourStart, isContourEnd));

    private static CapturedPointFlags PackFlags(
        bool onCurve, bool isContourStart, bool isContourEnd) =>
        (onCurve ? CapturedPointFlags.OnCurve : 0) |
        (isContourStart ? CapturedPointFlags.Start : 0) |
        (isContourEnd ? CapturedPointFlags.End : 0);

    public void AddPhantomPoint(Vector2 point) =>
        AppendPoint(point, CapturedPointFlags.Phantom);

    public void EndGlyph(int level)
    {
    }

    private void AppendPoint(in Vector2 point, CapturedPointFlags flags)
    {
        EnsureSpace(Count + 1);
        data[Count++] = new CapturedPoint(point, flags);
    }

    private void EnsureSpace(int size)
    {
        if (size < data.Length) return;
        var newData = allocator.Allocate(Math.Max(50, size * 2));
        data.CopyTo(newData, 0);
        allocator.Free(data);
        data = newData;
    }

    private void Reset()
    {
        Count = 0;
        allocator.Free(data);
        data = [];
    }

    public void Replay(ITrueTypePointTarget target)
    {
        foreach (var item in data.AsSpan(0, Count))
        {
            if (item.Flags.HasFlag(CapturedPointFlags.Phantom))
            {
                target.AddPhantomPoint(item.Point);
            }
            else
            {
                target.AddPoint(item.Point,
                    item.Flags.HasFlag(CapturedPointFlags.OnCurve),
                    item.Flags.HasFlag(CapturedPointFlags.Start),
                    item.Flags.HasFlag(CapturedPointFlags.End));
            }
        }
    }
}

internal readonly partial struct CapturedPoint
    {
        [FromConstructor] public Vector2 Point { get; }
        [FromConstructor] public CapturedPointFlags Flags { get; }
    }

    [Flags]
    internal enum CapturedPointFlags : byte
    {
        OnCurve = 0x01,
        Start = 0x02,
        End = 0x04,
        Phantom = 0x08,
    } 