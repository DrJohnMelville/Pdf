﻿using Melville.Fonts.Type1TextParsers.EexecDecoding;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Fonts.Type1TextParsers;

/// <summary>
/// This is an internal class that parses type 1 ascii fonts.  it is public so the
/// comparing reader can get the postscript interpreter for the debugger
/// </summary>
/// <param name="source"></param>
public readonly struct Type1Parser(IMultiplexSource source)
{
    private readonly EexecDecryptingByteSource eexecDecryptingSource = new(source);

    internal async ValueTask<IReadOnlyList<IGenericFont>> ParseAsync()
    {
        try
        {
            var parser = CreateEngine();
            using var tokens = new Tokenizer(eexecDecryptingSource);
            await parser.ExecuteAsync(tokens).CA();
            return ExtractFont(parser);
        }
        finally
        {
            eexecDecryptingSource.Dispose();
            source.Dispose();
        }
    }

    private static IReadOnlyList<IGenericFont> ExtractFont(PostscriptEngine parser) => 
        parser.Tag as IReadOnlyList<IGenericFont> ?? [];

    /// <summary>
    /// Create a postscriptengine with extra functions to parse the font.
    /// </summary>
    /// <returns>A new postscript engine ready to read this font</returns>
    public PostscriptEngine CreateEngine() =>
        new(PostscriptOperatorCollections.BaseLanguage()
            .With(Type1FontPostscriptOperations.AddOperations)
            .With(AddEexec));

    private void AddEexec(IPostscriptDictionary obj)
    {
        obj.Put("eexec", PostscriptValueFactory.Create(
            (IExternalFunction)eexecDecryptingSource));

        obj.Put("FontDirectory", PostscriptValueFactory.CreateDictionary());
        
        obj.Put("StandardEncoding", PostscriptValueFactory.CreateArray([
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("space"u8),
            MakeName("exclam"u8),
            MakeName("quotedbl"u8),
            MakeName("numbersign"u8),
            MakeName("dollar"u8),
            MakeName("percent"u8),
            MakeName("ampersand"u8),
            MakeName("quoteright"u8),
            MakeName("parenleft"u8),
            MakeName("parenright"u8),
            MakeName("asterisk"u8),
            MakeName("plus"u8),
            MakeName("comma"u8),
            MakeName("hyphen"u8),
            MakeName("period"u8),
            MakeName("slash"u8),
            MakeName("zero"u8),
            MakeName("one"u8),
            MakeName("two"u8),
            MakeName("three"u8),
            MakeName("four"u8),
            MakeName("five"u8),
            MakeName("six"u8),
            MakeName("seven"u8),
            MakeName("eight"u8),
            MakeName("nine"u8),
            MakeName("colon"u8),
            MakeName("semicolon"u8),
            MakeName("less"u8),
            MakeName("equal"u8),
            MakeName("greater"u8),
            MakeName("question"u8),
            MakeName("at"u8),
            MakeName("A"u8),
            MakeName("B"u8),
            MakeName("C"u8),
            MakeName("D"u8),
            MakeName("E"u8),
            MakeName("F"u8),
            MakeName("G"u8),
            MakeName("H"u8),
            MakeName("I"u8),
            MakeName("J"u8),
            MakeName("K"u8),
            MakeName("L"u8),
            MakeName("M"u8),
            MakeName("N"u8),
            MakeName("O"u8),
            MakeName("P"u8),
            MakeName("Q"u8),
            MakeName("R"u8),
            MakeName("S"u8),
            MakeName("T"u8),
            MakeName("U"u8),
            MakeName("V"u8),
            MakeName("W"u8),
            MakeName("X"u8),
            MakeName("Y"u8),
            MakeName("Z"u8),
            MakeName("bracketleft"u8),
            MakeName("backslash"u8),
            MakeName("bracketright"u8),
            MakeName("asciicircum"u8),
            MakeName("underscore"u8),
            MakeName("quoteleft"u8),
            MakeName("a"u8),
            MakeName("b"u8),
            MakeName("c"u8),
            MakeName("d"u8),
            MakeName("e"u8),
            MakeName("f"u8),
            MakeName("g"u8),
            MakeName("h"u8),
            MakeName("i"u8),
            MakeName("j"u8),
            MakeName("k"u8),
            MakeName("l"u8),
            MakeName("m"u8),
            MakeName("n"u8),
            MakeName("o"u8),
            MakeName("p"u8),
            MakeName("q"u8),
            MakeName("r"u8),
            MakeName("s"u8),
            MakeName("t"u8),
            MakeName("u"u8),
            MakeName("v"u8),
            MakeName("w"u8),
            MakeName("x"u8),
            MakeName("y"u8),
            MakeName("z"u8),
            MakeName("braceleft"u8),
            MakeName("bar"u8),
            MakeName("braceright"u8),
            MakeName("asciitilde"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("exclamdown"u8),
            MakeName("cent"u8),
            MakeName("sterling"u8),
            MakeName("fraction"u8),
            MakeName("yen"u8),
            MakeName("florin"u8),
            MakeName("section"u8),
            MakeName("currency"u8),
            MakeName("quotesingle"u8),
            MakeName("quotedblleft"u8),
            MakeName("guillemotleft"u8),
            MakeName("guilsinglleft"u8),
            MakeName("guilsinglright"u8),
            MakeName("fi"u8),
            MakeName("fl"u8),
            MakeName("notdef"u8),
            MakeName("endash"u8),
            MakeName("dagger"u8),
            MakeName("daggerdbl"u8),
            MakeName("periodcentered"u8),
            MakeName("notdef"u8),
            MakeName("paragraph"u8),
            MakeName("bullet"u8),
            MakeName("quotesinglbase"u8),
            MakeName("quotedblbase"u8),
            MakeName("quotedblright"u8),
            MakeName("guillemotright"u8),
            MakeName("ellipsis"u8),
            MakeName("perthousand"u8),
            MakeName("notdef"u8),
            MakeName("questiondown"u8),
            MakeName("notdef"u8),
            MakeName("grave"u8),
            MakeName("acute"u8),
            MakeName("circumflex"u8),
            MakeName("tilde"u8),
            MakeName("macron"u8),
            MakeName("breve"u8),
            MakeName("dotaccent"u8),
            MakeName("dieresis"u8),
            MakeName("notdef"u8),
            MakeName("ring"u8),
            MakeName("cedilla"u8),
            MakeName("notdef"u8),
            MakeName("hungarumlaut"u8),
            MakeName("ogonek"u8),
            MakeName("caron"u8),
            MakeName("emdash"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("AE"u8),
            MakeName("notdef"u8),
            MakeName("ordfeminine"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("Lslash"u8),
            MakeName("Oslash"u8),
            MakeName("OE"u8),
            MakeName("ordmasculine"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("ae"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("dotlessi"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("lslash"u8),
            MakeName("oslash"u8),
            MakeName("oe"u8),
            MakeName("germandbls"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
            MakeName("notdef"u8),
        ]));
    }

    private static PostscriptValue MakeName(ReadOnlySpan<byte> name) =>
        PostscriptValueFactory.CreateString(name, StringKind.LiteralName);
}