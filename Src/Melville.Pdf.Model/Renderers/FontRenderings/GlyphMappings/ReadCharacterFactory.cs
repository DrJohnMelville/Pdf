using System;
using System.Buffers;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings.BuiltInCMaps;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;
using SingleByteCharacters = Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders.SingleByteCharacters;
using TwoByteCharacters = Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders.TwoByteCharacters;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal readonly partial struct ReadCharacterFactory
{
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfEncoding encoding;
    [FromConstructor] private readonly INameToGlyphMapping nameMapper;

    public  ValueTask<IReadCharacter> CreateAsync() =>
        KnownNames.Type0.Equals(font.SubType()) ?
            ParseType0FontEncodingAsync(): 
            new(SingleByteCharacters.Instance);

    private async ValueTask<IReadCharacter> ParseType0FontEncodingAsync()
    {
#warning This is  not clean code -- clean it up once it works.
        var outerFontCmap = encoding.IsIdentityCdiEncoding()?
            TwoByteCharacters.Instance:
            (await ReadCmap(encoding.LowLevel, HasNoBaseFont.Instance).CA())??SingleByteCharacters.Instance;
        //Ordering of Identity means no mapping
        var inner = await font.Type0SubFontAsync().CA();
        var sysInfo = await inner.CidSystemInfoAsync().CA();
        if (sysInfo is null) return outerFontCmap;
        var ordering = await sysInfo.GetOrDefaultAsync(KnownNames.Ordering, KnownNames.Identity).CA();
        var registry = await sysInfo.GetOrDefaultAsync(KnownNames.Registry, KnownNames.Identity).CA();
        if (registry.Equals(KnownNames.Identity) || ordering.Equals(KnownNames.Identity))
            return outerFontCmap;
        
        var innerName = PdfDirectObject.CreateName($"{registry}-{ordering}-UCS2");
        var innercmap = await ReadCmap(innerName, outerFontCmap).CA();

        return  innercmap??outerFontCmap;
    }

    private ValueTask<IReadCharacter?> ReadCmap(PdfDirectObject encoding, IReadCharacter baseMapper) =>
        new CMapFactory(nameMapper,baseMapper, BuiltinCmapLibrary.Instance)
            .ParseCMapAsync(encoding);
}


[StaticSingleton]
internal partial class HasNoBaseFont: IReadCharacter
{
    public Memory<uint> GetCharacters(
        in ReadOnlyMemory<byte> input, in Memory<uint> scratchBuffer, out int bytesConsumed) => 
        throw new PdfParseException("Builtin CMAPS should not rely on a base font.");
}