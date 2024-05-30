using System.Buffers;
using Melville.INPC;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

public static class GlyphRecorderFactory
{
    private static readonly GlyphRecorder[] recorders = new GlyphRecorder[20];
    private static int count = 0;

    public static GlyphRecorder GetRecorder()
    {
        lock (recorders)
        {
            if (count == 0)
            {
                return new GlyphRecorder(PooledAllocator.Instance);
            }
            return recorders[--count];
        }
    }

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