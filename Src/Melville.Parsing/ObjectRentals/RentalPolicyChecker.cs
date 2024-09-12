using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using Melville.INPC;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Melville.Parsing.ObjectRentals;

#if DEBUG


internal static class RentalLog
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

    public static void Log(string message, string trace) =>
        WriteLine($"""
            {message}
            {trace}
            """);


    public static void SetTarget(Action<string>? newOutput)
    {
        if (newOutput is null) return;
        output = newOutput;

    }
}

internal readonly struct RentalRecord
{
    #if true
    public string Trace { get; } = "Stack tracing disabled";
    #else
    public string Trace { get; } = new StackTrace(4).Clip();
    #endif
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

/// <summary>
/// This is a debug-only class for checking that rental rules are observed.
/// </summary>
[StaticSingleton]
public partial class RentalPolicyChecker
{
    private readonly List<RentalRecord> rentals = new();

    /// <summary>
    /// This is a scope used to make a single unit test enforce the pooling rules.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static IDisposable RentalScope(Action<string>? target) => 
        new RentalScopeImplementation(Instance, target);

    internal void CheckOut(object item)
    {
        foreach (var rental in rentals.ToArray()) rental.CheckObjectAtRental(item);
        rentals.Add(new RentalRecord(item));
    }

    internal void CheckIn(object item)
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
            throw new InvalidOperationException(
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

        /// <inheritdoc />
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
internal class RentalPolicyTestBase : IDisposable
{
    private IDisposable ctx = RentalPolicyChecker.RentalScope(null);
    public virtual void Dispose() => ctx.Dispose();
}
#else 
/// <summary>
/// This checks to make sure the ObjectPool is being used correctly.
/// </summary>
public static class RentalPolicyChecker
{
    /// <summary>
    /// Returns a scope within which all rentals and returns should occurr
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static IDisposable RentalScope(Action<string>? s = null) => new EmptyScope();
    private class EmptyScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
#endif