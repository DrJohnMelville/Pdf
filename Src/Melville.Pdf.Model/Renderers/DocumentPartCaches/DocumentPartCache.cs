using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.DocumentPartCaches;

public interface IDocumentPartCache: IDisposable
{
    ValueTask<T> Get<TSource, T>(TSource source, Func<TSource, ValueTask<T>> creator) where TSource:notnull ;
}

public class DocumentPartCache: IDocumentPartCache
{
    private readonly Dictionary<object, object> store = new();


    public ValueTask<T> Get<TSource, T>(TSource source, Func<TSource, ValueTask<T>> creator) where TSource:notnull
         =>
        store.TryGetValue(source, out var value) && value is T valAsT
            ? new ValueTask<T>(valAsT)
            : FindAndStoreValue(source, creator);

    private async ValueTask<T> FindAndStoreValue<T, TSource>(
        TSource source, Func<TSource, ValueTask<T>> creator) where TSource:notnull
    {
        var value = await creator(source).CA();
        if (value is not null) store[source] = value;
        return value;
    }

    public void Dispose()
    {
        foreach (var item in store.Values.OfType<IDisposable>())
        {
            item.Dispose();
        }
        store.Clear();
    }
}