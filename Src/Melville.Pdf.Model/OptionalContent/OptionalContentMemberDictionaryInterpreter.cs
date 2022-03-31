using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

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

    public async  ValueTask<bool> Parse()
    {
        var ocgs = (await dictionary.GetOrDefaultAsync(KnownNames.OCGs, PdfArray.Empty).CA()).AsList();
        return await EvaluateUsingP(ocgs, await dictionary.GetOrDefaultAsync(KnownNames.P, KnownNames.AnyOn).CA());
    }

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
            if (await state.IsGroupVisible(
                    await ocg.DirectValueAsync().CA() as PdfDictionary).CA() == expected) return valueIfFound;
        }

        return !valueIfFound;
    }
}