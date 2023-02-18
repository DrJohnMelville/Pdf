using System;
using System.Collections.Generic;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

internal readonly struct DisposeList: IDisposable
{
    private readonly List<IDisposable> itemsToDispose = new();

    public DisposeList()
    {
    }

    public T TryRegister<T>(T item) // generic so we do not box value types just to check them
    {
        if (item is IDisposable iDisposabke) itemsToDispose.Add(iDisposabke);
        return item;
    }

    public void Dispose()
    {
        foreach (var item in itemsToDispose)
        {
            item.Dispose();
        }
    }
}