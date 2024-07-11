using System.Numerics;
using System.Runtime.InteropServices;

namespace Melville.Parsing.ObjectRentals;

public interface IClearable
{
    void Clear();
}

public abstract class ObjectPoolBase<T> where T : class
{
#warning -- ?locking
    private const int bufferLength = 20;
    private VectorBuffer buffer = new();
    private int nextSlot;

    #warning use new lock object in .net 9.0
    private object mutex = new ();
    
    public T Rent()
    {
        lock (mutex)
        {
            return nextSlot == 0 ? Create() : ClearAndReturnSlot();
        }
    }

    public void Return(T item)
    {
        lock (mutex)
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
        buffer[nextSlot] = null;
        return ret;
    }


    [System.Runtime.CompilerServices.InlineArray(bufferLength)]
    public struct VectorBuffer
    {
        private T? _element0;
    }

    protected abstract T Create();
}

public class ObjectPool<T> : ObjectPoolBase<T> where T : class, new()
{
    public static ObjectPoolBase<T> Shared = new ObjectPool<T>();
    protected override T Create() => new T();
}

