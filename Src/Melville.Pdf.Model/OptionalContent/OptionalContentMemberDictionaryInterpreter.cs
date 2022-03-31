using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.OptionalContent;

public readonly struct OptionalContentMemberDictionaryInterpreter
{
    private readonly PdfDictionary dictionary;
    private readonly IOptionalContentState state;

    public OptionalContentMemberDictionaryInterpreter(PdfDictionary dictionary, IOptionalContentState state)
    {
        this.dictionary = dictionary;
        this.state = state;
    }

    public async  ValueTask<bool> Parse() =>
        await (await VeDictionary().CA() is PdfArray arr ? 
            EvaluateVe(arr) : 
            EvaluateUsingOcgs()).CA();

    private async ValueTask<bool> EvaluateUsingOcgs() =>
        await EvaluateUsingP(
            (await Ocgs().CA()).AsList(), 
            await PDictionary().CA()).CA();

    private ValueTask<PdfName> PDictionary() => dictionary.GetOrDefaultAsync(KnownNames.P, KnownNames.AnyOn);
    private ValueTask<PdfArray> Ocgs() => dictionary.GetOrDefaultAsync(KnownNames.OCGs, PdfArray.Empty);
    private ValueTask<PdfObject> VeDictionary() => dictionary.GetOrNullAsync(KnownNames.VE);

    private ValueTask<bool> EvaluateUsingP(IEnumerable<PdfObject> ocgs, PdfName rule) =>
        rule.GetHashCode() switch
        {
            KnownNameKeys.AnyOn => CheckAny(ocgs, true),
            KnownNameKeys.AnyOff => CheckAny(ocgs, false),
            KnownNameKeys.AllOff => CheckAll(ocgs, false),
            _ => CheckAll(ocgs, true)

        };

    private ValueTask<bool> CheckAll(IEnumerable<PdfObject> ocgs, bool expected) => Check(ocgs, !expected, false);
    private ValueTask<bool> CheckAny(IEnumerable<PdfObject> ocgs, bool expected) => Check(ocgs, expected, true);

    private async ValueTask<bool> Check(IEnumerable<PdfObject> ocgs, bool expected, bool valueIfFound)
    {
        foreach (var ocg in ocgs)
        {
            if ( await EvaluateVeItem(ocg).CA() == expected) return valueIfFound;
        }

        return !valueIfFound;
    }

    private async ValueTask<bool> EvaluateVeItem(PdfObject ocg) =>
        await (await ocg.DirectValueAsync().CA() switch
        {
            PdfDictionary dict => state.IsGroupVisible(dict),
            PdfArray arr => EvaluateVe(arr),
            _ => throw new PdfParseException("Invalid optional content member dictionary item")
        }).CA();

    private async ValueTask<bool> EvaluateVe(PdfArray arr) =>
        (await arr[0].CA()).GetHashCode() switch
        {
            KnownNameKeys.Not => !await EvaluateVeItem(arr.RawItems[1]).CA(),
            KnownNameKeys.Or => await CheckAny(arr.RawItems.Skip(1), true).CA(),
            _ => await CheckAll(arr.RawItems.Skip(1), true).CA(),
        };

}