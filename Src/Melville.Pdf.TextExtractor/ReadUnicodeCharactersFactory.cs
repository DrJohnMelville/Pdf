﻿using System.Reflection.Metadata;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

namespace Melville.Pdf.TextExtractor;


internal readonly partial struct ReadUnicodeCharactersFactory
{
    [FromConstructor] private readonly IRealizedFont baseFont;
    [FromConstructor] private readonly PdfDictionary fontDictionary;

    public async ValueTask<IReadCharacter> ParseTextMappingAsync()
    {
        return (await fontDictionary.GetOrNullAsync(KnownNames.ToUnicode)) is {IsNull:false } cmap?
            await ParseCmapAsync(cmap, baseFont).CA():baseFont.ReadCharacter;
    }

    private async ValueTask<IReadCharacter> ParseCmapAsync(PdfDirectObject cmap, IRealizedFont realizedFont) => 
        (await new CMapFactory(GlyphNameToUnicodeMap.AdobeGlyphList, TwoByteCharacters.Instance)
            .ParseCMapAsync(cmap)) ?? SingleByteCharacters.Instance;
}