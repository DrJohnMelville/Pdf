using System.Buffers;
using System.Diagnostics;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// This is a rental facility for GlyphRecorders.  This is needed too get amortized,
/// allocation free renderiing of the composite glyphs.
/// </summary>
public static class GlyphRecorderFactory
{
    private static readonly GlyphRecorder[] recorders = new GlyphRecorder[20];
    private static int count = 0;

    /// <summary>
    /// Rent a glyphrecorder from the pool.
    /// </summary>
    /// <returns>An initalized GlyphRecorder that is empty and ready to use.</returns>
    public static GlyphRecorder GetRecorder()
    {
        lock (recorders)
        {
            if (count == 0)
            {
                return new GlyphRecorder(PooledAllocator.Instance);
            }

            var ret = recorders[--count];
            recorders[count] = null!; 
            return ret;
        }
    }

    /// <summary>
    /// Return a glyphrecorder to the rental pool.  Consumer muse not access the
    /// recorder after it is returned.
    /// </summary>
    /// <param name="recorder">The recorder to return.</param>
    public static void ReturnRecorder(GlyphRecorder recorder)
    {
        lock (recorders)
        {
            if (count == recorders.Length)
            {
                return;
            }
            recorder.Reset();
            recorders[count++] = recorder;
        }
    }
}

[StaticSingleton]
internal partial class PooledAllocator : IRecorderAllocator
{
    public CapturedPoint[] Allocate(int size) => ArrayPool<CapturedPoint>.Shared.Rent(size);

    public void Free(CapturedPoint[] data) => ArrayPool<CapturedPoint>.Shared.Return(data);
}