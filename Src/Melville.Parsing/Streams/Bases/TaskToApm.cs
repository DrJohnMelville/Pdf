namespace Melville.Parsing.Streams.Bases;

/// <summary>
/// Convert task to APM model
/// </summary>
public static class TaskToApm
{
    /// <summary>
    /// Convert a task to an APM model 
    /// </summary>
    /// <typeparam name="T">The type of the Task</typeparam>
    /// <param name="task">The task to be replaced.</param>
    /// <param name="callback">The callback to call when the task is completed</param>
    /// <param name="state">The state parameter to pass to the callback.</param>
    /// <returns>An IAsyncResult representing th task.</returns>
    public static IAsyncResult AsApm<T>(this Task<T> task,
        AsyncCallback? callback,
        object? state)
    {
        var tcs = new TaskCompletionSource<T>(state);
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
                tcs.TrySetException(t.Exception?.InnerExceptions ?? 
                                    Array.Empty<Exception>() as IReadOnlyCollection<Exception>);
            else if (t.IsCanceled)
                tcs.TrySetCanceled();
            else
                tcs.TrySetResult(t.Result);

            if (callback != null)
                callback(tcs.Task);
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>
    /// Convert a task to an APM model 
    /// </summary>
    /// <param name="task">The task to be replaced.</param>
    /// <param name="callback">The callback to call when the task is completed</param>
    /// <param name="state">The state parameter to pass to the callback.</param>
    /// <returns>An IAsyncResult representing th task.</returns>
    public static IAsyncResult AsApm(this Task task,
        AsyncCallback? callback,
        object? state)
    {
        var tcs = new TaskCompletionSource(state);
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
                tcs.TrySetException(t.Exception?.InnerExceptions ?? 
                                    Array.Empty<Exception>() as IReadOnlyCollection<Exception>);
            else if (t.IsCanceled)
                tcs.TrySetCanceled();
            else
                tcs.TrySetResult();

            if (callback != null)
                callback(tcs.Task);
        }, TaskScheduler.Default);
        return tcs.Task;
    }
    
}