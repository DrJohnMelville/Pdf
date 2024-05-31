using System.Collections;
using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// This class can record and replay the points from a glyph.  When paired with GlyphRecorderFactory
/// it can do so with amortized zero allocations.
/// </summary>
/// <param name="allocator">An interface to get buffers from the allocator</param>
public class GlyphRecorder :
    ITrueTypePointTarget, IReadOnlyList<CapturedPoint>
{
    private readonly IRecorderAllocator allocator;

    internal GlyphRecorder(IRecorderAllocator allocator) => this.allocator = allocator;

    private CapturedPoint[] data = [];


    #region Recording

    /// <inheritdoc />
    public void AddPoint(Vector2 p, bool onCurve, bool isContourStart, bool isContourEnd) =>
        AppendPoint(p,
            PackFlags(onCurve, isContourStart, isContourEnd));

    private static CapturedPointFlags PackFlags(
        bool onCurve, bool isContourStart, bool isContourEnd) =>
        (onCurve ? CapturedPointFlags.OnCurve : 0) |
        (isContourStart ? CapturedPointFlags.Start : 0) |
        (isContourEnd ? CapturedPointFlags.End : 0);

    /// <inheritdoc />
    public void AddPhantomPoint(Vector2 point) =>
        AppendPoint(point, CapturedPointFlags.Phantom);

    private void AppendPoint(in Vector2 point, CapturedPointFlags flags)
    {
        EnsureSpace(Count + 1);
        data[Count++] = new CapturedPoint(point, flags);
    }

    #endregion

    #region Buffer management

    private void EnsureSpace(int size)
    {
        if (size < data.Length) return;
        var newData = allocator.Allocate(Math.Max(50, size * 2));
        data.AsSpan().CopyTo(newData);
        allocator.Free(data);
        data = newData;
    }

    internal void Reset()
    {
        Count = 0;
        allocator.Free(data);
        data = [];
    }

    #endregion

    /// <summary>
    /// Replay the point sequence into a new point target.
    /// </summary>
    /// <param name="target">The target to play the points into</param>
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

    /// <summary>
    /// Replay the point sequence into a new point target.
    /// </summary>
    /// <param name="target">The target to play the points into</param>
    /// <param name="transform">A transform to apply to each point during playback</param>
    public void Replay(ITrueTypePointTarget target, Matrix3x2 transform)
    {
        foreach (var item in data.AsSpan(0, Count))
        {
            if (item.Flags.HasFlag(CapturedPointFlags.Phantom))
            {
                target.AddPhantomPoint(Vector2.Transform(item.Point, transform));
            }
            else
            {
                target.AddPoint(Vector2.Transform(item.Point, transform),
                    item.Flags.HasFlag(CapturedPointFlags.OnCurve),
                    item.Flags.HasFlag(CapturedPointFlags.Start),
                    item.Flags.HasFlag(CapturedPointFlags.End));
            }
        }
    }

    #region IReadOnlyList members

    /// <inheritdoc />
    public int Count { get; private set; } = 0;

    /// <inheritdoc />
    public IEnumerator<CapturedPoint> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return data[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public CapturedPoint this[int index] => 
        ((uint)index) < Count ? data[index]: 
            throw new ArgumentOutOfRangeException("Index out of used range");

    #endregion
}