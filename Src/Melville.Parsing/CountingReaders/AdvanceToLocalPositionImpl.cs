using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.CountingReaders;

/// <summary>
/// Implements an extension method to advance an IByteSource  to a new forward position
/// </summary>
public static class AdvanceToLocalPositionImpl
{
    /// <summary>
    /// Advance the byte source to the position indicated.
    /// </summary>
    /// <param name="pipe">The byte source to advance</param>
    /// <param name="targetPosition">The desired position</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static ValueTask AdvanceToLocalPositionAsync(
        this IByteSource pipe, long targetPosition) =>
        (targetPosition - pipe.Position) switch
        {
            < 0 => throw new InvalidOperationException("Cannot rewind a pipe reader"),
            0 => new ValueTask(),
            var delta => TryAdvanceFastAsync(pipe, delta)
        };
    
    private static ValueTask TryAdvanceFastAsync(IByteSource pipe, long delta)
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