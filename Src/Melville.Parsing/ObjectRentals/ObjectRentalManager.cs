namespace Melville.Parsing.ObjectRentals;

/// <summary>
/// Simple onbject rental facility.
/// </summary>
/// <typeparam name="T">The type of the object to rent, must be a class with a default constructor</typeparam>
public class ObjectRentalMan1ager<T> where T:class, new()
{
    private int itemsAvailable = 0;
    private readonly T[] items;

    /// <summary>
    /// Create a new ObjectRentalManager
    /// </summary>
    /// <param name="rentalCapacity">Maximum number of objects to buffer</param>
    public ObjectRentalMan1ager(int rentalCapacity) => items = new T[rentalCapacity];
 
    /// <summary>
    /// Obtain a rented object.
    /// </summary>
    /// <returns>The desired object.</returns>
    public T Rent()
    {
        lock (items)
        {
            return itemsAvailable > 0 ? items[--itemsAvailable] : new T();
        }
    }

    /// <summary>
    /// Return a rented object to the rental pool.  Consumer must not touch
    /// the object after returning it.
    /// </summary>
    /// <param name="item">The object to be returned.</param>
    public void Return(T item)
    {
        lock (items)
        {
            if (itemsAvailable < items.Length) items[itemsAvailable++] = item;
        }
    }
}