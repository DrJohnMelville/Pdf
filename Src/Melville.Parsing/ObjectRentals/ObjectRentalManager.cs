using System.Buffers;

namespace Melville.Parsing.ObjectRentals;

public class ObjectRentalManager<T> where T:class, new()
{
    private int itemsAvailable = 0;
    private readonly T[] items;

    public ObjectRentalManager(int rentalCapacity) => items = new T[rentalCapacity];
 
    public T Rent()
    {
        lock (items)
        {
            return itemsAvailable > 0 ? items[--itemsAvailable] : new T();
        }
    }

    public void Return(T item)
    {
        lock (items)
        {
            if (itemsAvailable < items.Length) items[itemsAvailable++] = item;
        }
    }
}