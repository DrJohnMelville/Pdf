﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

internal static class StitchedFunctionParser
{
    public static async ValueTask<PdfFunction> ParseAsync(PdfDictionary source)
    {
        var domain = await source.ReadIntervalsAsync(KnownNames.Domain).CA();
        var encode = await source.ReadIntervalsAsync(KnownNames.Encode).CA();
        var bounds = await (await source.GetAsync<PdfArray>(KnownNames.Bounds).CA()).CastAsync<double>().CA();
        var functionDecls =
            await (await source.GetAsync<PdfArray>(KnownNames.Functions).CA()).CastAsync<PdfDictionary>().CA();
        var functions = await CreateFunctionSegmentsAsync(functionDecls, domain[0], bounds, encode).CA();

        var range = await source.ReadOptionalRangesAsync(functions[0].NumberOfOutputs).CA();
        return new StitchedFunction(domain, range, functions);
    }
        
    private static async Task<StitchedFunctionSegment[]> CreateFunctionSegmentsAsync(
        IReadOnlyList<PdfDictionary> functionDecls, ClosedInterval domain, IReadOnlyList<double> bounds, 
        ClosedInterval[] encode)
    {
        var functions = new StitchedFunctionSegment[functionDecls.Count];
        for (int i = 0; i < functionDecls.Count; i++)
        {
            functions[i] = new StitchedFunctionSegment(
                SegmentDomain(bounds, i, domain),
                encode[i],
                await functionDecls[i].CreateFunctionAsync().CA());
        }

        return functions;
    }

    private static ClosedInterval SegmentDomain(IReadOnlyList<double> bounds, int i, ClosedInterval domain) =>
        new(
            i == 0 ? domain.MinValue : bounds[i-1],
            i >= bounds.Count ? domain.MaxValue : bounds[i]);
}