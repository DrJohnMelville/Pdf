using System.Buffers;

using Melville.Parsing.CountingReaders;
using Melville.Parsing.IStreamAdaptors;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct GlyphFromSid(ushort[] glyphSids): IDisposable
{
    public void Dispose()
    {
        ArrayPool<ushort>.Shared.Return(glyphSids);
    }

    public static async ValueTask <GlyphFromSid> ParseAsync(
        int length, ParsingBuffer pipe)
    {
        using var data = new PooledBuffer<ushort>(length);
        data.AsSpan().Clear();
        var target = new MemoryTarget(data.AsMemory());
        await new CharSetReader<MemoryTarget>(pipe, target).ReadCharSetAsync().CA();
        return new GlyphFromSid(data);
    }

    public int Search(ushort sid) =>
        Math.Max(glyphSids.AsSpan().IndexOf(sid), 0);
}