﻿using System.Numerics;
using System.Runtime.InteropServices;

namespace Melville.Parsing.ObjectRentals;

/// <summary>
/// This interface is used to clear an object that is being returned to an ObjectPoolBase.
/// </summary>
public interface IClearable
{
    /// <summary>
    /// Clear the object so it can be returned to the object pool
    /// </summary>
    void Clear();
}

/// <summary>
/// This class is a base class for object pools.
/// </summary>
/// <typeparam name="T">The type of object to be pooled</typeparam>
public abstract class ObjectPoolBase<T> where T : class
{
    private const int bufferLength = 20;
    private VectorBuffer buffer = new();
    private int nextSlot;

    #warning use new lock object in .net 9.0
    private object mutex = new();
    
    /// <summary>
    /// Get a rented object from the pool.
    /// </summary>
    /// <returns>A new object of the type of the pol</returns>
    public T Rent()
    {
        lock (mutex)
        {
            return nextSlot == 0 ? Create() : ClearAndReturnSlot();
        }
    }

    /// <summary>
    /// Return an object to the rental pool.  Caller must not reference the object
    /// after returning it.
    /// </summary>
    /// <param name="item">The object to return to the pool.</param>
    public void Return(T item)
    {
        lock(mutex)
        {
            if (nextSlot >= bufferLength)
            {
                (item as IDisposable)?.Dispose();
                return;
            }

            (item as IClearable)?.Clear();
            buffer[nextSlot++] = item;
        }
    }

    private T ClearAndReturnSlot()
    {
        nextSlot--;
        var ret = buffer[nextSlot];
        if (ret is null)
            throw new InvalidOperationException(
                "Internal object pool error -- pool object should not be null");
        buffer[nextSlot] = null;
        return ret;
    }


    [System.Runtime.CompilerServices.InlineArray(bufferLength)]
    private struct VectorBuffer
    {
        private T? _element0;
    }

    /// <summary>
    /// Create a new object of the pooled object type.
    /// </summary>
    /// <returns>A new object of the pooled type</returns>
    protected abstract T Create();
}

/// <summary>
/// This is a concrete object pool class for objects with a default constructor
/// </summary>
/// <typeparam name="T">The type to be pooled.</typeparam>
public class ObjectPool<T> : ObjectPoolBase<T> where T : class, new()
{
    /// <summary>
    /// Singleton object pool for the given type.
    /// </summary>
    public static ObjectPoolBase<T> Shared = new ObjectPool<T>();

    /// <inheritdoc />
    protected override T Create() => new T();
}
