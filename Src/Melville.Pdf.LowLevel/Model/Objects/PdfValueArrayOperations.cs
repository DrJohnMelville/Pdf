using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class PdfValueArrayOperations
{
    public static async ValueTask<T> GetAsync<T>(this PdfArray array, int index) =>
        (await array[index].CA()).Get<T>();

    public static async ValueTask<PdfDirectObject[]> AsDirectValues(this PdfArray array)
    {
        var ret = new PdfDirectObject[array.Count];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = await array[i].CA();
        }

        return ret;
    }

}