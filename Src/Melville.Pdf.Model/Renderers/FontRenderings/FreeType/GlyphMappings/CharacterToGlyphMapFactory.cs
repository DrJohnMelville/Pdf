using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

public readonly partial struct CharacterToGlyphMapFactory
{
    [FromConstructor] private readonly Face face;
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfObject? encoding;

    public async ValueTask<IMapCharacterToGlyph> Parse() =>
        (await font.SubTypeAsync().CA()).GetHashCode() switch
        {
            KnownNameKeys.Type0 => await Type0CharMapping().CA(),
            KnownNameKeys.MMType1 => throw new NotImplementedException("MultiMaster fonts not implemented."),
            KnownNameKeys.Type1 => await SingleByteNamedMapping().CA(),
            KnownNameKeys.TrueType => await ParseTrueTypeMapping((await font.FontFlagsAsync().CA())).CA(),
            _ => throw new PdfParseException("Unknown Font Type"),

        };

    // this is a variance from the spec.  If a symbolic true type font lacks a valid CMAP for mapping
    // we fall back and attempt to map the font as a roman font.
    private ValueTask<IMapCharacterToGlyph> ParseTrueTypeMapping(FontFlags fontFlags) => 
        TryMapAsSymbolicFont(fontFlags) is {} ret? new(ret): SingleByteNamedMapping();

    private IMapCharacterToGlyph? TryMapAsSymbolicFont(FontFlags fontFlags) => 
        fontFlags.HasFlag(FontFlags.Symbolic) ? TrueTypeSymbolicMapping(): null;

    private IMapCharacterToGlyph? TrueTypeSymbolicMapping()
    {
        var charmap = ValidCharMap(face.CharMapByInts(1, 0)) ?? ValidCharMap(face.CharMapByInts(3, 0));
        if (charmap is null) return null;
        var ret = new uint[256];
        foreach (var (character, glyph) in charmap.AllMappings())
        {
            ret[character & 0xFF] = glyph;
        }

        return new CharacterToGlyphArray(ret);
    }

    private CharMap? ValidCharMap(CharMap? input) => 
        input?.AllMappings().Any()??false?input:null;

    private async ValueTask<IMapCharacterToGlyph> SingleByteNamedMapping()
    {
        var array = new uint[256];
        var nameToGlyphMapper = new NameToGlyphMappingFactory(face).Create();
        await new SingleByteEncodingParser(nameToGlyphMapper, array, await BuiltInFontCharMappings().CA())
            .WriteEncodingToArray(encoding).CA();
        return new CharacterToGlyphArray(array);
    }

    private async Task<byte[][]?> BuiltInFontCharMappings()
    {
        if ((await font.SubTypeAsync().CA()) != KnownNames.Type1) return null;
        return (await font.BaseFontNameAsync().CA()).GetHashCode() switch
        {
            KnownNameKeys.Symbol => CharacterEncodings.Symbol,
            KnownNameKeys.ZapfDingbats => CharacterEncodings.ZapfDingbats,
            _=> null
        };
    }
    
    private async ValueTask<IMapCharacterToGlyph> Type0CharMapping()
    {
        var subFont = await font.Type0SubFont().CA();
        return  (await subFont.CidToGidMapStream().CA() is {} mapStream)?
            await new CMapStreamParser(
                PipeReader.Create(await mapStream.StreamContentAsync().CA())).Parse().CA():
             IdentityCharacterToGlyph.Instance;
    }
}

public readonly partial struct CMapStreamParser
{
    private readonly List<uint> dictionary = new();
    [FromConstructor] private readonly PipeReader pipe;
    
    public async ValueTask<IMapCharacterToGlyph> Parse()
    {
        return new CharacterToGlyphArray(await ReadList().CA());
    }

    private async ValueTask<IReadOnlyList<uint>> ReadList()
    {
        while (await pipe.ReadAsync().CA() is { } result &&
               (result.Buffer.Length > 1 || !result.IsCompleted))
        {
            ProcessItems(result.Buffer);
        }
        return dictionary;
    }

    private void ProcessItems(ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        while (CanReadUshort(reader)) dictionary.Add(ReadUShort(ref reader));
        pipe.AdvanceTo(reader.Position, buffer.End);
    }

    private static uint ReadUShort(ref SequenceReader<byte> reader)
    {
        Trace.Assert(reader.TryRead(out var hiByte));
        Trace.Assert(reader.TryRead(out var lowByte));
        var value = (uint)((hiByte << 8) | lowByte);
        return value;
    }

    private static bool CanReadUshort(in SequenceReader<byte> reader) => reader.Remaining >= 2;
}
