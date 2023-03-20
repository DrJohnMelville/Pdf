using System.Runtime.CompilerServices;

namespace Melville.Parsing.AwaitConfiguration;

/// <summary>
/// The WPF renderer must always return to the same thread, but others do not.  This class controls whether or
/// not await operations must resume on the same theread they were awaited upon
/// </summary>
public static class AwaitConfig
{
    private static bool resumeOnCalledThread = false;

    /// <summary>
    /// Set the global value whether or not to resume on the calling thread
    /// </summary>
    /// <param name="value">True if must always resume on the calling thread, false otherwsie</param>
    public static void ResumeOnCalledThread(bool value) => resumeOnCalledThread = value;

    /// <summary>
    /// Configure the parameter for the current thread resume setting
    /// </summary>
    /// <param name="vt">Task or Valuetask representing the await operation</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredValueTaskAwaitable CA(this in ValueTask vt) => vt.ConfigureAwait(resumeOnCalledThread);

    /// <summary>
    /// Configure the parameter for the current thread resume setting
    /// </summary>
    /// <param name="vt">Task or Valuetask representing the await operation</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredValueTaskAwaitable<T> CA<T>(this in ValueTask<T> vt) =>
        vt.ConfigureAwait(resumeOnCalledThread);

    /// <summary>
    /// Configure the parameter for the current thread resume setting
    /// </summary>
    /// <param name="vt">Task or Valuetask representing the await operation</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable CA(this Task vt) => vt.ConfigureAwait(resumeOnCalledThread);

    /// <summary>
    /// Configure the parameter for the current thread resume setting
    /// </summary>
    /// <param name="vt">Task or Valuetask representing the await operation</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable<T> CA<T>(this Task<T> vt) => vt.ConfigureAwait(resumeOnCalledThread);

    /// <summary>
    /// Configure the parameter for the current thread resume setting
    /// </summary>
    /// <param name="vt">Task or Valuetask representing the await operation</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredCancelableAsyncEnumerable<T> CA<T>(this IAsyncEnumerable<T> vt) =>
        vt.ConfigureAwait(resumeOnCalledThread);

    /// <summary>
    /// Configure the parameter for the current thread resume setting
    /// </summary>
    /// <param name="vt">Task or Valuetask representing the await operation</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredAsyncDisposable CA(this IAsyncDisposable vt) => vt.ConfigureAwait(resumeOnCalledThread);
}