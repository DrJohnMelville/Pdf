using System.Buffers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ParserMapping;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal partial class CffGenericFont
{
#if DEBUG
    public ValueTask TryAddToParseMapAsync()
    {
        if (!source.IsLoggingParseMap()) return ValueTask.CompletedTask;
        return AddToParseMapAsync();
    }

    private async ValueTask AddToParseMapAsync()
    {
        await ParsePrivateDictAsync().CA();
        await ParseCharSetAsync().CA();
        await ParseEncodingAsync().CA();
    }

    private async Task ParsePrivateDictAsync()
    {
        var bookmark = source.CreateParseMapBookmark((int)privateOffset);
        if (privateOffset is 0)
        {
            bookmark.LogParsePosition("No Private Dictionary");
            return;
        }

        bookmark.IndentParseMap("Private Dict");
        bookmark.JumpToParseMap(0);
        var privateDictPipe = await PrivateDictBytes().CA();
        var dictParser = new DictParser<CffDictionaryDefinition>(
            new SequenceReader<byte>(privateDictPipe.Buffer.Slice(0, privateSize)),
            bookmark, stackalloc DictValue[1]);
        while (dictParser.ReadNextInstruction(0) is not 0xFF)
        {
            // do nothing
        }
        bookmark.OutdentParseMap();
    }

    private ValueTask ParseCharSetAsync()
    {
        var bookMark = source.CreateParseMapBookmark((int)charSetOffset);
        return charSetOffset switch
        {
            0 => LogBuiltin("Built-in CharSet IsoAdobe", bookMark),
            1 => LogBuiltin("Built-in CharSet Expert", bookMark),
            2 => LogBuiltin("Built-in CharSet ExpertSubset", bookMark),
            _ => CustomCharsetAsync(bookMark)
        };
    }

    private ValueTask LogBuiltin(string charset, ParseMapBookmark bookmark)
    {
        bookmark.LogParsePosition(charset);
        return ValueTask.CompletedTask;
    }

    private async ValueTask CustomCharsetAsync(ParseMapBookmark? bookMark)
    {
        bookMark.IndentParseMap("Custom Char Map");
        bookMark.JumpToParseMap(0);
        using var charsetPipe = source.ReadPipeFrom(charSetOffset, charSetOffset);
        source.AddParseMapAlias(charsetPipe);
        await new CharSetReader<FakeCharsetTarget>(charsetPipe, new FakeCharsetTarget(charsetPipe, 
            charStringIndex.Length)).ReadCharSetAsync().CA();
        bookMark.OutdentParseMap();
    }

    internal readonly struct FakeCharsetTarget(IByteSource charsetPipe, uint length) : ICharSetTarget
    {
        public long Count => length;
        public ValueTask SetGlyphNameAsync(int index, ushort SID)
        {
            charsetPipe.LogParsePosition($"{SID} => {index}");
            return ValueTask.CompletedTask;
        }
    }

    private ValueTask ParseEncodingAsync()
    {
        var bookmark = source.CreateParseMapBookmark((int)encodingOffset);
        bookmark.JumpToParseMap(0);
        return encodingOffset switch
        {
            0 => LogBuiltin("Built-in Encoding Standard", bookmark),
            1 => LogBuiltin("Built-in Encoding Expert", bookmark),
            _ => CustomEncodingAsync(bookmark)
        };
    }

    private async ValueTask CustomEncodingAsync(ParseMapBookmark? bookmark)
    {
        bookmark.IndentParseMap("Custom Encoding");
        bookmark.JumpToParseMap(0);
            ((await ReadCmapAsync(bookmark).CA()) as IDisposable )?.Dispose();
        bookmark.OutdentParseMap();
    }


#else
        public ValueTask TryAddToParseMapAsync() => ValueTask.CompletedTask;
#endif
}
