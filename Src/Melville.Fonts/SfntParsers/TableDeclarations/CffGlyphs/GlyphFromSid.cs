using System.Buffers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct GlyphFromSid(ushort[] glyphSids): IDisposable
{
    public void Dispose()
    {
        ArrayPool<ushort>.Shared.Return(glyphSids);
    }

    public static async ValueTask <GlyphFromSid> ParseAsync(
        int length, IByteSource pipe)
    {
        var data = ArrayPool<ushort>.Shared.Rent(length);
        data.AsSpan().Clear();
        var target = new MemoryTarget(data);
        await new CharSetReader<MemoryTarget>(pipe, target).ReadCharSetAsync().CA();
        return new GlyphFromSid(data);
    }

    public int Search(ushort sid) =>
        Math.Max(glyphSids.AsSpan().IndexOf(sid), 0);
}