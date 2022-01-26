using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

public static class FunctionParsingMethods
{
    public static async ValueTask<ClosedInterval[]> 
        ReadIntervals(this PdfDictionary source, PdfName name)
    {
        var array = await source.GetAsync<PdfArray>(name).ConfigureAwait(false);
        var length = array.Count / 2;
        var ret = new ClosedInterval[length];
        for (int i = 0; i < length; i++)
        {
            ret[i] = new ClosedInterval(
                (await array.GetAsync<PdfNumber>(2 * i).ConfigureAwait(false)).DoubleValue,
                (await array.GetAsync<PdfNumber>((2 * i) + 1).ConfigureAwait(false)).DoubleValue);
        }

        return ret;
    }
    public static async ValueTask<ClosedInterval[]> ReadOptionalRanges(
        this PdfDictionary source, int numberOfOutputs) =>
        source.ContainsKey(KnownNames.Range)
            ? await source.ReadIntervals(KnownNames.Range).ConfigureAwait(false)
            : Enumerable.Repeat(ClosedInterval.NoRestriction, numberOfOutputs).ToArray();
    public static async ValueTask<double[]> ReadArrayWithDefault(
        this PdfDictionary source, PdfName name, int defaultValue) =>
        source.ContainsKey(name)
            ? await (await source.GetAsync<PdfArray>(name).ConfigureAwait(false)).AsDoublesAsync().ConfigureAwait(false)
            : new double[]{defaultValue};

}