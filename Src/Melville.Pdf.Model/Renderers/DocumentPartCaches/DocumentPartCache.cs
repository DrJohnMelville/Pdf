using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.DocumentPartCaches;

public interface IDocumentPartCache
{
    ValueTask<T> Get<TSource, T>(TSource source, Func<TSource, ValueTask<T>> creator) where TSource:class;
}

public class DocumentPartCache: IDocumentPartCache
{
    private readonly Dictionary<object, object> store = new();


    public ValueTask<T> Get<TSource, T>(TSource source, Func<TSource, ValueTask<T>> creator)
        where TSource: class =>
        store.TryGetValue(source, out var value) && value is T valAsT
            ? new ValueTask<T>(valAsT)
            : FindAndStoreValue(source, creator);

    private async ValueTask<T> FindAndStoreValue<T, TSource>(
        TSource source, Func<TSource, ValueTask<T>> creator) where TSource:class
    {
        var value = await creator(source).CA();
        if (value is not null) store[source] = value;
        return value;
    }
}