using System.Buffers;
using System.ComponentModel.Design.Serialization;
using System.IO.Pipelines;
using System.Text;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.Fonts.SfntParsers.TableParserParts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.PostscriptDatas
{
    internal readonly struct PostscriptTableParser(IByteSource source)
    {
        public async ValueTask<PostscriptData> ParseAsync()
        {
            var result = await FieldParser.ReadFromAsync<PostscriptData>(source).CA();

            result.SetGlpyhNames(await ReadGlyphNamesAsync(source, result.Version).CA());

            return result;
        }

        private ValueTask<string[]> ReadGlyphNamesAsync(
            IByteSource source, uint version) => version switch
        {
            0x00010000 => new (DefaultNames),
            0x00020000 => ReadType20NamesAsync(source),
            0x00025000 => ReadType25NamesAsync(source),
            _ => new ValueTask<string[]>([])
        };

        private async ValueTask<string[]> ReadType20NamesAsync(IByteSource source)
        {
            var ret = await PrepareResultArrayAsync(source).CA();
            var indexes = ArrayPool<int>.Shared.Rent(ret.Length);
            indexes.AsSpan().Fill(-1);
            var result = await source.ReadAtLeastAsync(ret.Length*2).CA();
            source.AdvanceTo(Read20Differences(result.Buffer, ret, indexes));
            
            await ReadCustomNamesAsync(source, ret, indexes).CA();
            ArrayPool<int>.Shared.Return(indexes);
            return ret;
        }

        private async ValueTask ReadCustomNamesAsync(
            IByteSource source, string[] ret, int[] indexes)
        {
            var count = indexes.AsSpan().LastIndexOfAnyExcept(-1);
            for (int i = 0; i <= count; i++)
            {
                var result = await source.ReadAsync().CA();
                var strLen = result.Buffer.FirstSpan[0];
                source.AdvanceTo(result.Buffer.GetPosition(1));
                result = await source.ReadAtLeastAsync(strLen).CA();
                ret[indexes[i]] = Encoding.ASCII.GetString(result.Buffer.Slice(0, strLen));
                source.AdvanceTo(result.Buffer.GetPosition(strLen));
            }
        }

        private SequencePosition Read20Differences(
            ReadOnlySequence<byte> resultBuffer, string[] ret, int[] indexes)
        {
            var topString = -1;
            var reader = new SequenceReader<byte>(resultBuffer);
            for (int i = 0; i < ret.Length; i++)
            {
                if (!reader.TryReadBigEndianUint16(out var index)) 
                    throw new InvalidDataException("No glyph names in Post version 2.5 table");
                if (index < 258)
                    ret[i] = DefaultNames[index];
                else
                {
                    index -= 258;
                    indexes[index] = i;
                    topString = Math.Max(topString, index);
                }
            }

            return reader.Position;
        }

        private async ValueTask<string[]> ReadType25NamesAsync(IByteSource source)
        {
            var ret = await PrepareResultArrayAsync(source).CA();
            var result2 = await source.ReadAtLeastAsync(ret.Length).CA();
            source.AdvanceTo(ReadDifferences(result2.Buffer, ret));
            return ret;
        }

        private async ValueTask<string[]> PrepareResultArrayAsync(IByteSource source)
        {
            var result = await source.ReadAtLeastAsync(2).CA();
            var count = ReadCount(result.Buffer);
            source.AdvanceTo(result.Buffer.GetPosition(2));
            return new string[count];
        }

        private SequencePosition ReadDifferences(
            ReadOnlySequence<byte> resultBuffer, string[] ret)
        {
            var reader = new SequenceReader<byte>(resultBuffer);
            for (int i = 0; i < ret.Length; i++)
            {
                if (!reader.TryRead(out byte firstByte)) 
                    throw new InvalidDataException("No glyph names in Post version 2.5 table");
                ret[i] = DefaultNames[firstByte];
            }

            return reader.Position;
        }

        private int ReadCount(ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            return reader.ReadBigEndianUint16();
        }

        public static readonly string[] DefaultNames =
        [
            ".notdef",
            ".null",
            "nonmarkingreturn",
            "space",
            "exclam",
            "quotedbl",
            "numbersign",
            "dollar",
            "percent",
            "ampersand",
            "quotesingle",
            "parenleft",
            "parenright",
            "asterisk",
            "plus",
            "comma",
            "hyphen",
            "period",
            "slash", "zero", "one", "two", "three", "four", "five", "six", "seven",
            "eight", "nine",
            "colon", "semicolon",
            "less", "equal", "greater",
            "question", "at",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
            "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "bracketleft", "backslash",
            "bracketright", "asciicircum", "underscore", "grave",
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q",
            "r", "s", "t", "u", "v", "w", "x", "y", "z",
            "braceleft", "bar", "braceright", "asciitilde",
            "Adieresis", "Aring",
            "Ccedilla",
            "Eacute", "Ntilde", "Odieresis", "Udieresis",
            "aacute", "agrave", "acircumflex",
            "adieresis", "atilde", "aring",
            "ccedilla",
            "eacute", "egrave", "ecircumflex", "edieresis",
            "iacute", "igrave", "icircumflex", "idieresis",
            "ntilde",
            "oacute", "ograve", "ocircumflex", "odieresis", "otilde",
            "uacute", "ugrave", "ucircumflex", "udieresis",
            "dagger", "degree",
            "cent", "sterling", "section", "bullet", "paragraph", "germandbls",
            "registered", "copyright",
            "trademark",
            "acute", "dieresis", "notequal", "AE", "Oslash", "infinity", "plusminus",
            "lessequal", "greaterequal", "yen", "mu", "partialdiff", "summation",
            "product", "pi", "integral", "ordfeminine", "ordmasculine",
            "Omega", "ae", "oslash", "questiondown", "exclamdown", "logicalnot",
            "radical", "florin", "approxequal", "Delta",
            "guillemotleft", "guillemotright", "ellipsis", "nonbreakingspace",
            "Agrave", "Atilde", "Otilde", "OE", "oe", "endash", "emdash",
            "quotedblleft", "quotedblright", "quoteleft", "quoteright",
            "divide", "lozenge", "ydieresis", "Ydieresis", "fraction",
            "currency", "guilsinglleft", "guilsinglright", "fi", "fl",
            "daggerdbl", "periodcentered", "quotesinglbase", "quotedblbase",
            "perthousand", "Acircumflex", "Ecircumflex", "Aacute", "Edieresis",
            "Egrave", "Iacute", "Icircumflex", "Idieresis", "Igrave", "Oacute",
            "Ocircumflex", "apple", "Ograve", "Uacute", "Ucircumflex", "Ugrave",
            "dotlessi", "circumflex", "tilde", "macron", "breve", "dotaccent",
            "ring", "cedilla", "hungarumlaut", "ogonek", "caron", "Lslash",
            "lslash", "Scaron", "scaron", "Zcaron", "zcaron", "brokenbar",
            "Eth", "eth", "Yacute", "yacute", "Thorn", "thorn", "minus",
            "multiply", "onesuperior", "twosuperior", "threesuperior",
            "onehalf", "onequarter", "threequarters", "franc", "Gbreve",
            "gbreve", "Idotaccent", "Scedilla", "scedilla", "Cacute", "cacute",
            "Ccaron", "ccaron", "dcroat"
        ];
    }
}