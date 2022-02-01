namespace Melville.Parsing.AwaitConfiguration;

public class RunSynchronous
{
    public static void Do(Func<Task> func) => Task.Run(func).GetAwaiter().GetResult();
    public static T Do<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();
    public static void Do(Func<ValueTask> func) => Do(() => func().AsTask());
    public static T Do<T>(Func<ValueTask<T>> func) => Do(() => func().AsTask());
}