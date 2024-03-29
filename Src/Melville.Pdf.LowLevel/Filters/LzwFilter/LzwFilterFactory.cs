﻿using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

internal static class LzwFilterFactory
{
    public static async ValueTask<IStreamFilterDefinition> DecoderAsync(PdfDirectObject p) => 
        new LzwDecodeFilter(await p.EarlySwitchLengthAsync().CA());

    public static async ValueTask<IStreamFilterDefinition> EncoderAsync(PdfDirectObject p) => 
        new LzwEncodeFilter(await p.EarlySwitchLengthAsync().CA());
}