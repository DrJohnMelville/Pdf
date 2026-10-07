using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.StreamInterfaces;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal static class GlobalFreeTypeMutex
{
    private static readonly SemaphoreSlim freeTypeMutex = new(1);
    public static  void WaitFor() => freeTypeMutex.Wait();
    public static async Task WaitForAsync() => await freeTypeMutex.WaitAsync().CA();
    public static void Release() => freeTypeMutex.Release();
}