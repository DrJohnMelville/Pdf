using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Melville.INPC;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Melville.Parsing.ObjectRentals;

#if DEBUG

public static class RentalLog
{
    private static UdpClient? client = null;
    private static UdpClient Client
    {
        get
        {
            client ??= new UdpClient();
            return client;
        }
    }

    public static string WriteLine(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        Client.Send(bytes, bytes.Length, "127.0.0.1", 15321);
        return str;
    }

    public static void Log(string message, StackTrace trace) =>
        WriteLine($"""
            {message}
            {DumpTrace(trace)}
            """);

    private static string DumpTrace(StackTrace trace)
    {
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
}

internal readonly struct RentalRecord
{
    public StackTrace Trace { get; } = new StackTrace(4);
    private readonly WeakReference item;

    public RentalRecord(object item)
    {
        this.item = new WeakReference(item);
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
internal partial class RentalPolicyChecker
{
    private readonly List<RentalRecord> rentals = new();

    public static IDisposable RentalScope() => new RentalScopeImplementation(Instance);

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
            rentals.RemoveAt(rentals.Count - 1);
        }
    }


    private class RentalScopeImplementation : IDisposable
    {
        private readonly RentalPolicyChecker parent;

        public RentalScopeImplementation(RentalPolicyChecker parent)
        {
            this.parent = parent;
            if (parent.rentals.Any())
                RentalLog.WriteLine("Rental Manager had rentals at the time scope was initialized");
        }

        public void Dispose()
        {
            foreach (var rental in parent.rentals)
            {
                RentalLog.Log($"Object not returned at end of rental scope.", rental.Trace);
            }
        }
    }
}
#endif