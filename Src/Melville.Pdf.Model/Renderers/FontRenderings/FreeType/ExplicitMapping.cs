using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using SharpFont.PostScript;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class ExplicitMapping: IGlyphMapping
{
    private readonly IGlyphMapping inner;
    private IReadOnlyList<uint> mappings;

    public ExplicitMapping(IGlyphMapping inner, IReadOnlyList<uint> mappings)
    {
        this.inner = inner;
        this.mappings = mappings;
    }

    public (uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input)
    {
        var (raw, len) = inner.SelectGlyph(input);
        return (raw < mappings.Count ? mappings[(int)raw]: raw, len);
    }
}

public readonly struct ExplicitMappingFactory
{
    private readonly List<uint> dictionary = new();
    private readonly PipeReader pipe;
    private ExplicitMappingFactory(PipeReader pipe)
    {
        this.pipe = pipe;
    }

    public static async ValueTask<IGlyphMapping> Parse(IGlyphMapping inner, PdfStream source) =>
        new ExplicitMapping(inner, 
            await new ExplicitMappingFactory(
                PipeReader.Create(await source.StreamContentAsync().CA())).Parse().CA());

    private async ValueTask<IReadOnlyList<uint>> Parse()
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
       #warning assert success here because we checked ahead of time if we had enough room.
        reader.TryRead(out var hiByte);
        reader.TryRead(out var lowByte);
        var value = (uint)((hiByte << 8) | lowByte);
        return value;
    }

    private static bool CanReadUshort(in SequenceReader<byte> reader) => reader.Remaining >= 2;
}
