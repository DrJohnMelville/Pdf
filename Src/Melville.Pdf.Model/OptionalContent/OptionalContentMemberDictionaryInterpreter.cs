using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.OptionalContent;

internal readonly partial struct OptionalContentMemberDictionaryInterpreter
{
    [FromConstructor] private readonly PdfDictionary dictionary;
    [FromConstructor] private readonly IOptionalContentState state;

    public async  ValueTask<bool> ParseAsync()
    {
        var veItem = await VeDictionaryAsync().CA();
        var veAsArray = veItem.TryGet(out PdfArray? arr);
        return await (veAsArray
            ? EvaluateVeAsync(arr!)
            : EvaluateUsingOcgsAsync()).CA();
    }

    private async ValueTask<bool> EvaluateUsingOcgsAsync() =>
        await EvaluateUsingPAsync(
            (await OcgsAsync().CA()).ObjectAsUnresolvedList(), 
            await PDictionaryAsync().CA()).CA();

    private ValueTask<PdfDirectObject> PDictionaryAsync() => dictionary.GetOrDefaultAsync(KnownNames.PTName, KnownNames.AnyOnTName);
    private ValueTask<PdfDirectObject> OcgsAsync() => dictionary.GetOrDefaultAsync(KnownNames.OCGsTName, (PdfDirectObject)PdfArray.Empty);
    private ValueTask<PdfDirectObject> VeDictionaryAsync() => dictionary.GetOrNullAsync(KnownNames.VETName);

    private ValueTask<bool> EvaluateUsingPAsync(IEnumerable<PdfIndirectObject> ocgs, PdfDirectObject rule)
    {
        return rule switch
        {
            _ when rule.Equals(KnownNames.AnyOnTName) => CheckAnyAsync(ocgs, true),
            _ when rule.Equals(KnownNames.AnyOffTName) => CheckAnyAsync(ocgs, false),
            _ when rule.Equals(KnownNames.AllOffTName) => CheckAllAsync(ocgs, false),
            _ => CheckAllAsync(ocgs, true) 
        };
    }

    private ValueTask<bool> CheckAllAsync(IEnumerable<PdfIndirectObject> ocgs, bool expected) => CheckAsync(ocgs, !expected, false);
    private ValueTask<bool> CheckAnyAsync(IEnumerable<PdfIndirectObject> ocgs, bool expected) => CheckAsync(ocgs, expected, true);

    private async ValueTask<bool> CheckAsync(IEnumerable<PdfIndirectObject> ocgs, bool expected, bool valueIfFound)
    {
        foreach (var ocg in ocgs)
        {
            if ( await EvaluateVeItemAsync(ocg).CA() == expected) return valueIfFound;
        }

        return !valueIfFound;
    }

    private async ValueTask<bool> EvaluateVeItemAsync(PdfIndirectObject ocg) =>
         await ((await ocg.LoadValueAsync().CA()) switch
        {
            var x when x.TryGet(out PdfDictionary? dict) => state.IsGroupVisibleAsync(dict),
            var x when x.TryGet(out PdfArray? arr) => EvaluateVeAsync(arr),
            _ => throw new PdfParseException("Invalid optional content member dictionary item")
        }).CA();

    private async ValueTask<bool> EvaluateVeAsync(PdfArray arr) =>
        (await arr[0].CA()) switch
        {
            var x when x.Equals(KnownNames.NotTName) =>
                !await EvaluateVeItemAsync(arr.RawItems[1]).CA(),
            var x when x.Equals(KnownNames.OrTName) =>
                 await CheckAnyAsync(arr.RawItems.Skip(1), true).CA(),
            _ => await CheckAllAsync(arr.RawItems.Skip(1), true).CA(),
        };

}