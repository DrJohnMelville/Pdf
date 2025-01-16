using System.Runtime.InteropServices;

namespace Melville.Fonts.SfntParsers;

internal readonly struct TableCache
{
    private readonly Dictionary<uint, object> items = new();
    private readonly Lock mutex = new();

    public TableCache(){}

    public T GetTable<T>(uint tag, Func<T> loader) where T : class
    {
        lock (mutex)
        {
            ref var bucket =
                ref CollectionsMarshal.GetValueRefOrAddDefault(items, tag, out var exists);
            if (!exists) bucket = loader();
            return (T)bucket!;
        }
    }
}