using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using Melville.INPC;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Melville.Parsing.ObjectRentals;

#if DEBUG

public static class RentalLog
{
    private static UdpClient? client = null;
    private static Action<string> output = i =>
    {
        var bytes = Encoding.UTF8.GetBytes(i);
        Client.Send(bytes, bytes.Length, "127.0.0.1", 15321);
    };
    private static UdpClient Client
    {
        get
        {
            client ??= new UdpClient();
            return client;
        }
    }

    public static void WriteLine(string str) => output(str);

    public static void Log(string message, StackTrace? trace) =>
        WriteLine($"""
            {message}
            {DumpTrace(trace)}
            """);

    private static string DumpTrace(StackTrace? trace)
    {
        if (trace is null) return "Rental tracing is disabled.";
        var frames = trace.GetFrames().Select(PrintFrame).ToArray();
        var last = 0;
        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i].Contains("Melville")) last = i;
        }

        var ret = new StringBuilder();
        for (int i = 0; i <= last; i++)
        {
            ret.AppendLine(frames[i]);
        }

        return ret.ToString();
    }

    private static string PrintFrame(StackFrame i)
    {
        var method = i.GetMethod();
        if (method is null) return "No Method";
        return $"{method.DeclaringType}.{method.Name}";
    }

    public static void SetTarget(Action<string>? newOutput)
    {
        if (newOutput is null) return;
        output = newOutput;

    }
}

internal readonly struct RentalRecord
{
    public StackTrace? Trace { get; } = null;// new StackTrace(4);
    private readonly WeakReference item;
    public string TypeName { get; }

    public RentalRecord(object item)
    {
        this.item = new WeakReference(item);
        TypeName = item.GetType().ToString();
    }

    public void CheckObjectAtRental(object newlyRented)
    {
        var strongRef = item.Target;
        CheckForDisposedObject(strongRef);

        if (object.ReferenceEquals(newlyRented, strongRef))
        {
            RentalLog.Log(
                "An object was rented twice.", Trace);
        }
    }

    private void CheckForDisposedObject(object? strongRef)
    {
        if (strongRef is null)
        {
            RentalLog.Log(
                "A rented object was garbage collected before return.", Trace);
        }
    }

    public bool CheckObjectAtReturn(object returned)
    {
        var strongRef = item.Target;
        CheckForDisposedObject(strongRef);
        return ReferenceEquals(strongRef, returned);
    }
}

[StaticSingleton]
public partial class RentalPolicyChecker
{
    private readonly List<RentalRecord> rentals = new();

    public static IDisposable RentalScope(Action<string>? target) => 
        new RentalScopeImplementation(Instance, target);

    public void CheckOut(object item)
    {
        foreach (var rental in rentals.ToArray()) rental.CheckObjectAtRental(item);
        rentals.Add(new RentalRecord(item));
    }

    public void CheckIn(object item)
    {
        var index = -1;
        for (int i = 0; i < rentals.Count; i++)
        {
            if (rentals[i].CheckObjectAtReturn(item))
            {
                index = i;
            }
        }

        if (index == -1)
            RentalLog.WriteLine(
                $"An object was returned that was not rented. (or was returned twice.): {item}");
        if (index + 1 < rentals.Count)
        {
            rentals[index] = rentals[^1];
        }
        rentals.RemoveAt(rentals.Count - 1);
    }


    private class RentalScopeImplementation : IDisposable
    {
        private readonly RentalPolicyChecker parent;

        public RentalScopeImplementation(RentalPolicyChecker parent, 
            Action<string>? output)
        {
            RentalLog.SetTarget(output);
            this.parent = parent;
            if (parent.rentals.Any())
                RentalLog.WriteLine("Rental Manager had rentals at the time scope was initialized");
        }

        public void Dispose()
        {
            if (parent.rentals.Count == 0) return;
            foreach (var rental in parent.rentals.ToArray())
            {
                RentalLog.Log($"Object not returned at end of rental scope.: {rental.TypeName}",
                    rental.Trace);
            }
        }
    }
}

[Obsolete("Should be used for test only")]
public class RentalPolicyTestBase : IDisposable
{
    private IDisposable ctx = RentalPolicyChecker.RentalScope(null);
    public virtual void Dispose() => ctx.Dispose();
}
#else 
public static class RentalPolicyChecker
{
    public static IDisposable RentalScope() => new EmptyScope();
    private class EmptyScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
#endif