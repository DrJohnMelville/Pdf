﻿using System.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Melville.Parsing.ObjectRentals;

#if DEBUG

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
            throw new ObjectRentalException(
                "An object was rented twice.", trace);
        }
    }

    private void CheckForDisposedObject(object? strongRef)
    {
        if (strongRef is null)
        {
            throw new ObjectRentalException(
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
            throw new ObjectRentalException(
                "An object was returned that was not rented. (or was returned twice.)");
        if (index + 1 < rentals.Count)
        {
            rentals[index] = rentals[^1];
            rentals.RemoveAt(rentals.Count - 1);
        }
    }
}

internal class ObjectRentalException: Exception
{
    public StackTrace? RentalTrace { get; }

    public ObjectRentalException(string message) : base(message)
    {
    }

    public ObjectRentalException(string message, StackTrace origin) : base(
        $"""
        {message}
        
        Stack trace at rental:
        {origin}
        """)
    {
        RentalTrace = origin;
    }
}

#endif