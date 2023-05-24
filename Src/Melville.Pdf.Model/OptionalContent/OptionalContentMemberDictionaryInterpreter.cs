using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.OptionalContent;

internal readonly struct OptionalContentMemberDictionaryInterpreter
{
    private readonly PdfDictionary dictionary;
    private readonly IOptionalContentState state;

    public OptionalContentMemberDictionaryInterpreter(PdfDictionary dictionary, IOptionalContentState state)
    {
        this.dictionary = dictionary;
        this.state = state;
    }

    public async  ValueTask<bool> ParseAsync() =>
        await (await VeDictionaryAsync().CA() is PdfArray arr ? 
            EvaluateVeAsync(arr) : 
            EvaluateUsingOcgsAsync()).CA();

    private async ValueTask<bool> EvaluateUsingOcgsAsync() =>
        await EvaluateUsingPAsync(
            (await OcgsAsync().CA()).ObjectAsUnresolvedList(), 
            await PDictionaryAsync().CA()).CA();

    private ValueTask<PdfName> PDictionaryAsync() => dictionary.GetOrDefaultAsync(KnownNames.P, KnownNames.AnyOn);
    private ValueTask<PdfArray> OcgsAsync() => dictionary.GetOrDefaultAsync(KnownNames.OCGs, PdfArray.Empty);
    private ValueTask<PdfObject> VeDictionaryAsync() => dictionary.GetOrNullAsync(KnownNames.VE);

    private ValueTask<bool> EvaluateUsingPAsync(IEnumerable<PdfObject> ocgs, PdfName rule) =>
        rule.GetHashCode() switch
        {
            KnownNameKeys.AnyOn => CheckAnyAsync(ocgs, true),
            KnownNameKeys.AnyOff => CheckAnyAsync(ocgs, false),
            KnownNameKeys.AllOff => CheckAllAsync(ocgs, false),
            _ => CheckAllAsync(ocgs, true)

        };

    private ValueTask<bool> CheckAllAsync(IEnumerable<PdfObject> ocgs, bool expected) => CheckAsync(ocgs, !expected, false);
    private ValueTask<bool> CheckAnyAsync(IEnumerable<PdfObject> ocgs, bool expected) => CheckAsync(ocgs, expected, true);

    private async ValueTask<bool> CheckAsync(IEnumerable<PdfObject> ocgs, bool expected, bool valueIfFound)
    {
        foreach (var ocg in ocgs)
        {
            if ( await EvaluateVeItemAsync(ocg).CA() == expected) return valueIfFound;
        }

        return !valueIfFound;
    }

    private async ValueTask<bool> EvaluateVeItemAsync(PdfObject ocg) =>
        await (await ocg.DirectValueAsync().CA() switch
        {
            PdfDictionary dict => state.IsGroupVisibleAsync(dict),
            PdfArray arr => EvaluateVeAsync(arr),
            _ => throw new PdfParseException("Invalid optional content member dictionary item")
        }).CA();

    private async ValueTask<bool> EvaluateVeAsync(PdfArray arr) =>
        (await arr[0].CA()).GetHashCode() switch
        {
            KnownNameKeys.Not => !await EvaluateVeItemAsync(arr.RawItems[1]).CA(),
            KnownNameKeys.Or => await CheckAnyAsync(arr.RawItems.Skip(1), true).CA(),
            _ => await CheckAllAsync(arr.RawItems.Skip(1), true).CA(),
        };

}