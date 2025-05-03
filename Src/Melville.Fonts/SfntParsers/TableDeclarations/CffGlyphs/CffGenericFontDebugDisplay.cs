using System.Buffers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ParserMapping;

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
            0 => BuiltinCharSetAsync("IsoAdobe", bookMark),
            1 => BuiltinCharSetAsync("Expert", bookMark),
            2 => BuiltinCharSetAsync("ExpertSubset", bookMark),
            _ => CustomCharsetAsync(bookMark)
        };
    }

    private ValueTask BuiltinCharSetAsync(string charset, ParseMapBookmark bookmark)
    {
        bookmark.LogParsePosition($"Built-in CharSet {charset}");
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

#else
        public ValueTask TryAddToParseMapAsync() => ValueTask.CompletedTask;
#endif
}
