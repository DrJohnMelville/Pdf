using System.Buffers;
using System.ComponentModel.Design.Serialization;
using System.IO.Pipelines;
using System.Text;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.Fonts.SfntParsers.TableParserParts;

using Melville.Parsing.CountingReaders;
using Melville.Parsing.IStreamAdaptors;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers.TableDeclarations.PostscriptDatas
{
    internal readonly struct PostscriptTableParser(ParsingBuffer source)
    {
        public async ValueTask<PostscriptData> ParseAsync()
        {
            var result = await FieldParser.ReadFromAsync<PostscriptData>(source).CA();

            result.SetGlpyhNames(await ReadGlyphNamesAsync(source, result.Version).CA());

            return result;
        }

        private ValueTask<string[]> ReadGlyphNamesAsync(
            ParsingBuffer source, uint version) => version switch
        {
            0x00010000 => new (DefaultNames),
            0x00020000 => ReadType20NamesAsync(source),
            0x00025000 => ReadType25NamesAsync(source),
            _ => new ValueTask<string[]>([])
        };

        private async ValueTask<string[]> ReadType20NamesAsync(ParsingBuffer source)
        {
            var ret = await PrepareResultArrayAsync(source).CA();
            using var indexes = new PooledBuffer<int>(ret.Length);
            indexes.AsSpan().Fill(-1);

            var bytes = await source.TakeBytesAsync(ret.Length * 2).CA();
            Read20Differences(new SpanParsingBuffer(bytes.Span), ret, indexes);
                
            await ReadCustomNamesAsync(source, ret, indexes).CA();
            return ret;
        }
        private async ValueTask ReadCustomNamesAsync(
            ParsingBuffer source, string[] ret, int[] indexes)
        {
            var count = indexes.AsSpan().LastIndexOfAnyExcept(-1);

            for (int i = 0; i <= count; i++)
            {
                var len = await source.GetUInt8Async();
                var stringBytes = await source.TakeBytesAsync(len);
                ret[indexes[i]] = Encoding.ASCII.GetString(stringBytes.Span);
            }
        }
        private void Read20Differences(
            SpanParsingBuffer reader, string[] ret, int[] indexes)
        {
            var topString = -1;
            for (int i = 0; i < ret.Length; i++)
            {
                var index = reader.GetUInt16BigEndian();
                if (index < 258)
                {
                    ret[i] = DefaultNames[index];
                }
                else
                {
                    index -= 258;
                    indexes[index] = i;
                    topString = Math.Max(topString, index);
                }
            }

        }

        private async ValueTask<string[]> ReadType25NamesAsync(ParsingBuffer source)
        {
            var ret = await PrepareResultArrayAsync(source).CA();
            var results = await source.TakeBytesAsync(ret.Length);
            ReadDifferences(new SpanParsingBuffer(results.Span), ret);
            return ret;
        }

        private async ValueTask<string[]> PrepareResultArrayAsync(ParsingBuffer source)
        {
            var count = await source.GetUInt16BigEndianAsync().CA();
            return new string[count];
        }

        private void ReadDifferences( SpanParsingBuffer reader, string[] ret)
        {
            
            for (int i = 0; i < ret.Length; i++)
            {
                var firstByte = reader.GetUInt8();
                ret[i] = DefaultNames[firstByte];
            }

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