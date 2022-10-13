using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.CountingReaders;

public static class AdvanceToLocalPositionImpl
{
    public static ValueTask AdvanceToLocalPositionAsync(
        this IByteSource pipe, long targetPosition) =>
        (targetPosition - pipe.Position) switch
        {
            < 0 => throw new InvalidOperationException("Cannot rewind a pipe reader"),
            0 => new ValueTask(),
            var delta => TryAdvanceFast(pipe, delta)
        };
    
    private static ValueTask TryAdvanceFast(IByteSource pipe, long delta)
    {
        if (!pipe.TryRead(out var rr) || rr.Buffer.Length < delta) 
            return SlowAdvanceToPositionAsync(pipe, delta);
        pipe.AdvanceTo(rr.Buffer.GetPosition(delta));
        return new ValueTask();

    }

    private static async ValueTask SlowAdvanceToPositionAsync(IByteSource pipe, long delta)
    {
        while (true)
        {
            var ret = await pipe.ReadAsync().CA();
            if (ret.Buffer.Length > delta)
            {
                pipe.AdvanceTo(ret.Buffer.GetPosition(delta));
                return;
            }

            if (ret.IsCompleted) return;
            pipe.AdvanceTo(ret.Buffer.Start, ret.Buffer.End);
        }
    }
}