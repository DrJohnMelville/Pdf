using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using BenchmarkDotNet.Attributes;
using JetBrains.Profiler.Api;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf.Rendering;

namespace Performance.Playground.Rendering
{
    // Async to non async by Stephen Toub
//https://blogs.msdn.microsoft.com/pfxteam/2012/02/02/await-synchronizationcontext-and-console-apps-part-3/
public static class AsyncPump
{
  /// <summary>Runs the specified asynchronous method.</summary>
  /// <param name="asyncMethod">The asynchronous method to execute.</param>
  public static void Run(Action asyncMethod)
  {
    if (asyncMethod == null) throw new ArgumentNullException("asyncMethod");

    var prevCtx = SynchronizationContext.Current;
    try
    {
      // Establish the new context
      var syncCtx = new SingleThreadSynchronizationContext(true);
      SynchronizationContext.SetSynchronizationContext(syncCtx);

      // Invoke the function
      syncCtx.OperationStarted();
      asyncMethod();
      syncCtx.OperationCompleted();

      // Pump continuations and propagate any exceptions
      syncCtx.RunOnCurrentThread();
    }
    finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
  }

  /// <summary>Runs the specified asynchronous method.</summary>
  /// <param name="asyncMethod">The asynchronous method to execute.</param>
  public static void Run(Func<Task> asyncMethod)
  {
    if (asyncMethod == null) throw new ArgumentNullException("asyncMethod");

    var prevCtx = SynchronizationContext.Current;
    try
    {
      // Establish the new context
      var syncCtx = new SingleThreadSynchronizationContext(false);
      SynchronizationContext.SetSynchronizationContext(syncCtx);

      // Invoke the function and alert the context to when it completes
      var t = asyncMethod();
      if (t == null) throw new InvalidOperationException("No task provided.");
      t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

      // Pump continuations and propagate any exceptions
      syncCtx.RunOnCurrentThread();
      t.GetAwaiter().GetResult();
    }
    finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
  }

  /// <summary>Runs the specified asynchronous method.</summary>
  /// <param name="asyncMethod">The asynchronous method to execute.</param>
  public static T Run<T>(Func<Task<T>> asyncMethod)
  {
    if (asyncMethod == null) throw new ArgumentNullException("asyncMethod");

    var prevCtx = SynchronizationContext.Current;
    try
    {
      // Establish the new context
      var syncCtx = new SingleThreadSynchronizationContext(false);
      SynchronizationContext.SetSynchronizationContext(syncCtx);

      // Invoke the function and alert the context to when it completes
      var t = asyncMethod();
      if (t == null) throw new InvalidOperationException("No task provided.");
      t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

      // Pump continuations and propagate any exceptions
      syncCtx.RunOnCurrentThread();
      return t.GetAwaiter().GetResult();
    }
    finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
  }

  /// <summary>Provides a SynchronizationContext that's single-threaded.</summary>
  private sealed class SingleThreadSynchronizationContext : SynchronizationContext
  {
    /// <summary>The queue of work items.</summary>
    private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object?>> m_queue =
      new BlockingCollection<KeyValuePair<SendOrPostCallback, object?>>();
    /// <summary>The processing thread.</summary>
    private readonly Thread m_thread = Thread.CurrentThread;
    /// <summary>The number of outstanding operations.</summary>
    private int m_operationCount = 0;
    /// <summary>Whether to track operations m_operationCount.</summary>
    private readonly bool m_trackOperations;

    /// <summary>Initializes the context.</summary>
    /// <param name="trackOperations">Whether to track operation count.</param>
    internal SingleThreadSynchronizationContext(bool trackOperations)
    {
      m_trackOperations = trackOperations;
    }

    /// <summary>Dispatches an asynchronous message to the synchronization context.</summary>
    /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
    /// <param name="state">The object passed to the delegate.</param>
    public override void Post(SendOrPostCallback d, object? state)
    {
      if (d == null) throw new ArgumentNullException("d");
      m_queue.Add(new KeyValuePair<SendOrPostCallback, object?>(d, state));
    }

    /// <summary>Not supported.</summary>
    public override void Send(SendOrPostCallback d, object? state)
    {
      throw new NotSupportedException("Synchronously sending is not supported.");
    }

    /// <summary>Runs an loop to process all queued work items.</summary>
    public void RunOnCurrentThread()
    {
      foreach (var workItem in m_queue.GetConsumingEnumerable())
        workItem.Key(workItem.Value);
    }

    /// <summary>Notifies the context that no more work will arrive.</summary>
    public void Complete() { m_queue.CompleteAdding(); }

    /// <summary>Invoked when an async operation is started.</summary>
    public override void OperationStarted()
    {
      if (m_trackOperations)
        Interlocked.Increment(ref m_operationCount);
    }

    /// <summary>Invoked when an async operation is completed.</summary>
    public override void OperationCompleted()
    {
      if (m_trackOperations &&
          Interlocked.Decrement(ref m_operationCount) == 0)
        Complete();
    }
  }
}
    [MemoryDiagnoser()]
    public class FontRenderingPerf
    {
        [Benchmark]
        public async Task RenderSkia()
        {
            AwaitConfig.ResumeOnCalledThread(false);
            var dr = await LoadDocument();
            MeasureProfiler.StartCollectingData(); 
            await RenderWithSkia.ToSurface(dr, 1); 
            MeasureProfiler.StopCollectingData();
        }
        [Benchmark]
        public void RenderWpf()
        {
          AsyncPump.Run(async () =>
          {
            AwaitConfig.ResumeOnCalledThread(true);
            var dr = await LoadDocument();
            MeasureProfiler.StartCollectingData();
            await new RenderToDrawingGroup(dr, 1).RenderToDrawingImage();
            MeasureProfiler.StopCollectingData();
          });
        }

        private static async Task<DocumentRenderer> LoadDocument()
        {
            var file = File.Open(@"C:\Users\jmelv\Documents\Scratch\PDF torture test\Adalms colposcopy for sexual assault 2001.PDF", FileMode.Open);
            var dr = await DocumentRendererFactory.CreateRendererAsync(
                await PdfDocument.ReadAsync(file), new WindowsDefaultFonts());
            return dr;
        }
    }
}