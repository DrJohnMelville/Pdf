using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

public interface IRetrieveCmapStream
{
    public Stream CMapStreamFor(PdfDirectObject name);
}

internal partial class CMapFactory
{
    private readonly List<ByteRange> data = new();
    [FromConstructor] private IGlyphNameMap namer;
    [FromConstructor] private IReadCharacter innerMapper;
    [FromConstructor] private IRetrieveCmapStream cMapLibrary;

    public async ValueTask<IReadCharacter> ParseCMapAsync(PdfEncoding encoding)
    {
        await ReadFromPdfValue(encoding.LowLevel).CA();
        return CreateCMap();
    }

    #region Cmap Sources
    private static readonly IPostscriptDictionary dict =
        PostscriptOperatorCollections.BaseLanguage().With(CmapParserOperations.AddOperations);

    public async ValueTask ReadFromPdfValue(PdfDirectObject encoding)
    {
        await ReadFromCSharpStreamAsync(
            encoding.IsName ? cMapLibrary.CMapStreamFor(encoding) :
                await PdfToCSharpStreamAsync(encoding.Get<PdfStream>()).CA()).CA();
    }

    private ValueTask<Stream> PdfToCSharpStreamAsync(PdfStream stream)
    {
        return stream.StreamContentAsync();
    }

    public  async ValueTask ReadFromCSharpStreamAsync(Stream source)
    {
        var parser = new PostscriptEngine(dict) { Tag = this };
        parser.ResourceLibrary.Put("ProcSet", "CIDInit", PostscriptValueFactory.CreateDictionary());
        parser.ErrorDict.Put("undefined"u8, PostscriptValueFactory.CreateNull());
        await parser.ExecuteAsync(source).CA();

    }
    #endregion

    #region CMap Operators

    public CMap CreateCMap() => new CMap(data);

    public void AddCodespaces(ReadOnlySpan<PostscriptValue> values) => 
        values.ForEachGroup(AddSingleCodespace);

    private void AddSingleCodespace(PostscriptValue minValue, PostscriptValue maxValue) => 
        data.Add(new ByteRange(ToVbc(minValue), ToVbc(maxValue)));

    private VariableBitChar ToVbc(in PostscriptValue value) => 
        new(value.Get<StringSpanSource>().GetSpan());

    public void AddNotDefRanges(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleNotDefRange);

    private void AddSingleNotDefRange(
        PostscriptValue min, PostscriptValue max, PostscriptValue value) => 
        AddRange(new ConstantCMapper(ToVbc(min), ToVbc(max), (uint)value.Get<long>()));

    public void AddCidRanges(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleCidRange);

    private void AddSingleCidRange(
        PostscriptValue min, PostscriptValue max, PostscriptValue value) => 
        AddRange(new LinearCMapper(ToVbc(min), ToVbc(max), (uint)value.Get<long>()));

    public void AddCidChars(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleCidChar);

    private void AddSingleCidChar(PostscriptValue source, PostscriptValue dest)
    {
        var sourceVbc = ToVbc(source);
        AddRange(new ConstantCMapper(sourceVbc, sourceVbc, (uint)dest.Get<long>()));
    }


    private void AddRange(CMapMapperBase mapper)
    {
        foreach (var range in data)
        {
            if (range.Contains(mapper))
            {
                range.AddMapper(mapper);
                return;
            }
        }
    }

    public void AddBaseFontRanges(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(CreateSingleBaseFontRange);

    private void CreateSingleBaseFontRange(PostscriptValue min, PostscriptValue max, PostscriptValue value)
    {
        var minCharacter = ToVbc(min);
        var maxCharacter = ToVbc(max);
        if (value.IsString)
            AddRange(new BaseFontLinearMapper(minCharacter, maxCharacter, innerMapper, value));
        else
            ProcessBaseFontArray(minCharacter, maxCharacter, value.Get<IPostscriptArray>());
    }

    private void ProcessBaseFontArray(
        VariableBitChar minCharacter, VariableBitChar maxCharacter, IPostscriptArray value)
    {
        var length = Math.Min(value.Length, SizeOfRange(minCharacter, maxCharacter));
        for (int i = 0; i < length; i++)
        {
            CreateSingleBaseFontCharacter(minCharacter + i, value.Get(i));
        }
    }

    private static int SizeOfRange(VariableBitChar minCharacter, VariableBitChar maxCharacter) => 
        1+ (int)(maxCharacter - minCharacter);

    public void AddBaseFontChars(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(CreateSingleBaseFontChar);

    private void CreateSingleBaseFontChar(PostscriptValue input, PostscriptValue value) => 
        CreateSingleBaseFontCharacter(ToVbc(input), value);

    private void CreateSingleBaseFontCharacter(VariableBitChar character, PostscriptValue value)
    {
        AddRange(
            value.IsLiteralName ?
                new ConstantCMapper(character, character, ValueForName(value)):
                new BaseFontConstantMapper(character, character, innerMapper, value));
    }

    private uint ValueForName(PostscriptValue value) => 
        namer.TryMap(value.AsPdfName(), out var ret) ? (uint)ret : 0;

    #endregion
}