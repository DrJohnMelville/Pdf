using System.Buffers;
using Melville.Fonts.SfntParsers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Fonts;

public interface IGenericFont
{
    // this is an interface that handles all of the font types
}

public static class RootFontParser
{
    public static async Task<IReadOnlyList<IGenericFont>> ParseAsync(IMultiplexSource src)
    {
        var pipe = src.ReadPipeFrom(0);
        var  result = await pipe.ReadAtLeastAsync(4).CA();
        if (result.Buffer.Length < 4 ) return [];
        return await ParseFontTypeAsync(src, result.Buffer).CA();
    }

    private const uint openTypeFmt = 0x00_01_00_00;
    private const uint ottoFmt = 0x4F_54_54_4F;
    private const uint trueFmt = 0x74_72_75_65;
    private const uint typ1Fmt = 0x74_79_70_31;
    private const uint ttcfFmt = 0x74_74_63_66;

    private static ValueTask<IReadOnlyList<IGenericFont>> ParseFontTypeAsync(
        IMultiplexSource src, ReadOnlySequence<byte> resultBuffer)
    {
        var reader = new SequenceReader<byte>(resultBuffer);
        if (!(reader.TryReadBigEndianUint32(out var type)))
            return new([]);
        return type switch
        {
            openTypeFmt or ottoFmt or trueFmt or typ1Fmt => new SfntParser(src).ParseAsync(0),
            ttcfFmt => new FontCollectionParser(src).ParseAsync(),
            _ => new([])
        };
    }
}