using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class GlobalFreeTypeMutex
{
    private static readonly SemaphoreSlim freeTypeMutex = new(1);
    
    public static  void WaitFor()
    {
        freeTypeMutex.Wait();
        RecordLocation();
    }

    private static void RecordLocation()
    {
        // Debug.WriteLine("Got Mutex At:");
        // Debug.WriteLine(Environment.StackTrace);
    }

    public static async Task WaitForAsync()
    {
        await freeTypeMutex.WaitAsync().CA();
        RecordLocation();        
    }

    public static void Release() => freeTypeMutex.Release();
}