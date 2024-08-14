using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
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
            {trace}
            """);
}

internal readonly struct RentalRecord
{
    private readonly StackTrace trace = new StackTrace(4);
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
                "An object was rented twice.", trace);
        }
    }

    private void CheckForDisposedObject(object? strongRef)
    {
        if (strongRef is null)
        {
            RentalLog.Log(
                "A rented object was garbage collected before return.", trace);
        }
    }

    public bool CheckObjectAtReturn(object returned)
    {
        var strongRef = item.Target;
        CheckForDisposedObject(strongRef);
        return ReferenceEquals(strongRef, returned);
    }
}

internal class RentalPolicyChecker
{
    private readonly List<RentalRecord> rentals = new();

    public void CheckOut(object item)
    {
        foreach (var rental in rentals) rental.CheckObjectAtRental(item);
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
}
#endif