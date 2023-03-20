namespace Melville.Parsing.AwaitConfiguration;

/// <summary>
/// Run an asyncronous operation synchronously.  This actually just pushes the task to the threadpool and then blocks until the
/// threadpool is done.  Not very efficient, but avoids most of the deadlocks that can occur with other solutions.
/// </summary>
public class RunSynchronous
{
    /// <summary>
    /// Run the given async func synchronously.
    /// </summary>
    /// <param name="func">The async operation to run synchronously</param>
    public static void Do(Func<Task> func) => Task.Run(func).GetAwaiter().GetResult();
    /// <summary>
    /// Run the given async func synchronously.
    /// </summary>
    /// <param name="func">The async operation to run synchronously</param>
    public static T Do<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();
    /// <summary>
    /// Run the given async func synchronously.
    /// </summary>
    /// <param name="func">The async operation to run synchronously</param>
    public static void Do(Func<ValueTask> func) => Do(() => func().AsTask());
    /// <summary>
    /// Run the given async func synchronously.
    /// </summary>
    /// <param name="func">The async operation to run synchronously</param>
    public static T Do<T>(Func<ValueTask<T>> func) => Do(() => func().AsTask());
}