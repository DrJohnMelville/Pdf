using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.DocumentPartCaches;

internal interface IDocumentPartCache: IDisposable
{
    ValueTask<T> Get<TSource, T>(TSource source, Func<TSource, ValueTask<T>> creator) where TSource:notnull ;
}

internal class DocumentPartCache: IDocumentPartCache
{
    private readonly Dictionary<object, object> store = new();

    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
    public async ValueTask<T> Get<TSource, T>(TSource source, Func<TSource, ValueTask<T>> creator) where TSource:notnull
    {
        await semaphore.WaitAsync().CA();
        try
        {
            return store.TryGetValue(source, out var value) && value is T valAsT
                ? valAsT
                : await FindAndStoreValue(source, creator).CA();
        }
        finally
        {
            semaphore.Release();
        }
    }

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