using System.Buffers;
using Melville.INPC;
using Melville.Parsing.ObjectRentals;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// This is a rental facility for GlyphRecorders.  This is needed too get amortized,
/// allocation free renderiing of the composite glyphs.
/// </summary>
public class GlyphRecorderFactory: ObjectPoolBase<GlyphRecorder>
{
    public static readonly GlyphRecorderFactory Shared = new();

    protected override GlyphRecorder Create() => 
        new(PooledAllocator.Instance);
}

[StaticSingleton]
internal partial class PooledAllocator : IRecorderAllocator
{
    public CapturedPoint[] Allocate(int size) => ArrayPool<CapturedPoint>.Shared.Rent(size);

    public void Free(CapturedPoint[] data) => ArrayPool<CapturedPoint>.Shared.Return(data);
}