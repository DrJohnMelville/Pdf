using System.Runtime.CompilerServices;

namespace Melville.Parsing.AwaitConfiguration;

public static class AwaitConfig
{
    private static bool resumeOnCalledThread = false;

    public static void ResumeOnCalledThread(bool value) => resumeOnCalledThread = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredValueTaskAwaitable CA(this in ValueTask vt) => vt.ConfigureAwait(resumeOnCalledThread);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredValueTaskAwaitable<T> CA<T>(this in ValueTask<T> vt) => vt.ConfigureAwait(resumeOnCalledThread);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable CA(this Task vt) => vt.ConfigureAwait(resumeOnCalledThread);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable<T> CA<T>(this Task<T> vt) => vt.ConfigureAwait(resumeOnCalledThread);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredCancelableAsyncEnumerable<T> CA<T>(this IAsyncEnumerable<T> vt) =>
        vt.ConfigureAwait(resumeOnCalledThread);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredAsyncDisposable CA(this IAsyncDisposable vt) => vt.ConfigureAwait(resumeOnCalledThread);
}