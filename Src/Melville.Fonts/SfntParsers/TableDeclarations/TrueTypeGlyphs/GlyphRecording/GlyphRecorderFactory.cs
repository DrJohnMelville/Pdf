using System.Buffers;
using Melville.INPC;
using Melville.Parsing.ObjectRentals;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// This is a rental facility for GlyphRecorders.  This is needed for amortized,
/// allocation free rendering of the composite glyphs.
/// </summary>
public class GlyphRecorderFactory: ObjectPoolBase<GlyphRecorder>
{
    /// <summary>
    /// Single shared instance of the factory
    /// </summary>
    public static readonly GlyphRecorderFactory Shared = new();

    /// <inheritdoc />
    protected override GlyphRecorder Create() => 
        new(PooledAllocator.Instance);
}

[StaticSingleton]
internal partial class PooledAllocator : IRecorderAllocator
{
    public CapturedPoint[] Allocate(int size) => ArrayPool<CapturedPoint>.Shared.Rent(size);

    public void Free(CapturedPoint[] data) => ArrayPool<CapturedPoint>.Shared.Return(data);
}