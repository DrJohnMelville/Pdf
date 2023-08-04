using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

internal static class FunctionParsingMethods
{
    public static async ValueTask<ClosedInterval[]> 
        ReadIntervalsAsync(this PdfDictionary source, PdfDirectObject name)
    {
        var array = await source.GetAsync<PdfArray>(name).CA();
        var length = array.Count / 2;
        var ret = new ClosedInterval[length];
        for (int i = 0; i < length; i++)
        {
            ret[i] = new ClosedInterval(
                await array.GetAsync<double>(2 * i).CA(),
                await array.GetAsync<double>((2 * i) + 1).CA());
        }

        return ret;
    }
    public static async ValueTask<ClosedInterval[]> ReadOptionalRangesAsync(
        this PdfDictionary source, int numberOfOutputs) =>
        source.ContainsKey(KnownNames.Range)
            ? await source.ReadIntervalsAsync(KnownNames.Range).CA()
            : Enumerable.Repeat(ClosedInterval.NoRestriction, numberOfOutputs).ToArray();
    public static async ValueTask<IReadOnlyList<double>> ReadArrayWithDefaultAsync(
        this PdfDictionary source, PdfDirectObject name, int defaultValue) =>
        source.ContainsKey(name)
            ? await (await source.GetAsync<PdfArray>(name).CA()).CastAsync<double>().CA()
            : new double[]{defaultValue};

}