using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

public readonly partial struct CMapStreamParser
{
    private readonly List<uint> dictionary = new();
    [FromConstructor] private readonly PipeReader pipe;
    
    public async ValueTask<IMapCharacterToGlyph> Parse()
    {
        return new FontRenderings.GlyphMappings.CharacterToGlyphArray(await ReadList().CA());
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
