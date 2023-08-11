using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.FuzzTest;

public static class Program
{
    public static async Task Main(string[] cmdLineArgs)
    {
        if (cmdLineArgs.Length < 1)
        {
            Console.WriteLine("Must Pass a root path to find PDF files");
        }

        var logger = new ExceptionLogger();
        var pdfs = GatherPdfs(cmdLineArgs[0]);
        foreach (var pdf in pdfs)
        {
            try
            {
                await ParseFile.DoAsync(pdf, logger).AsTask().TimeoutAfterAsync(30_000);
            }
            catch (Exception)
            {
            }
        }
        Console.WriteLine();
        Console.WriteLine("Writing file");
        await using var output = File.Create(@"C:\Users\jmelv\Documents\Scratch\PDF torture test\Errors.xlsx");
        logger.WriteTo(output);
        Console.WriteLine("Done");
    }
    private static IEnumerable<string> GatherPdfs(string cmdLineArg)
    {
        foreach (var file in Directory.EnumerateFiles(cmdLineArg, "*.pdf",
                     new EnumerationOptions(){ IgnoreInaccessible = true, RecurseSubdirectories = true, ReturnSpecialDirectories = false}))
        {
            yield return file;
        }
    }
}

public struct VoidTypeStruct
{
}

public static class TestTimeouts
{
        public static Task TimeoutAfterAsync(this Task task, int millisecondsTimeout)
    {
        // Short-circuit #1: infinite timeout or task already completed
        if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
        {
            // Either the task has already completed or timeout will never occur.
            // No proxy necessary.
            return task;
        }

        // tcs.Task will be returned as a proxy to the caller
        TaskCompletionSource<VoidTypeStruct> tcs = 
            new TaskCompletionSource<VoidTypeStruct>();

        // Short-circuit #2: zero timeout
        if (millisecondsTimeout == 0)
        {
            // We've already timed out.
            tcs.SetException(new TimeoutException());
            return tcs.Task;
        }

        // Set up a timer to complete after the specified timeout period
        Timer timer = new Timer(state =>
        {
            if (state is null) return;
            // Recover your state information
            var myTcs = (TaskCompletionSource<VoidTypeStruct>)state;

            // Fault our proxy with a TimeoutException
            myTcs.TrySetException(new TimeoutException()); 
        }, tcs, millisecondsTimeout, Timeout.Infinite);

        // Wire up the logic for what happens when source task completes
        task.ContinueWith((antecedent, state) =>
            {
                if (state is null) return;
                // Recover our state data
                var tuple = 
                    (Tuple<Timer, TaskCompletionSource<VoidTypeStruct>>)state;

                // Cancel the Timer
                tuple.Item1.Dispose();

                // Marshal results to proxy
                MarshalTaskResults(antecedent, tuple.Item2);
            }, 
            Tuple.Create(timer, tcs),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        return tcs.Task;
    }
    internal static void MarshalTaskResults<TResult>(
        Task source, TaskCompletionSource<TResult> proxy)
    {
        switch (source.Status)
        {
            case TaskStatus.Faulted:
                proxy.TrySetException(source.Exception!);
                break;
            case TaskStatus.Canceled:
                proxy.TrySetCanceled();
                break;
            case TaskStatus.RanToCompletion:
                Task<TResult>? castedSource = source as Task<TResult>;
                proxy.TrySetResult(
                    castedSource is null ? default(TResult)! : // source is a Task
                        castedSource.Result); // source is a Task<TResult>
                break;
        }
    }

}