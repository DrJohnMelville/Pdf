using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings.BuiltInCMaps;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

/// <summary>
/// The factory class to parse a CMAP from a PdfDirectObject
/// </summary>
public partial class CMapFactory
{
    private readonly List<ByteRange> data = new();
    /// <summary>
    /// Strategy to map names to glyph values
    /// </summary>
    [FromConstructor] private INameToGlyphMapping namer;
    /// <summary>
    /// Strategy to read characters in the underlying font format
    /// </summary>
    [FromConstructor] private IReadCharacter innerMapper;
    /// <summary>
    /// Strategy to retreive named Cmaps by name
    /// </summary>
    [FromConstructor] private IRetrieveCmapStream cMapLibrary;

    /// <summary>
    /// Construct a CMapFactory
    /// </summary>
    /// <param name="namer">Strategy to map names to glyph values</param>
    /// <param name="innerMapper">Strategy to read characters in the underlying font encoding</param>
    public CMapFactory(INameToGlyphMapping namer, IReadCharacter innerMapper): 
        this(namer, innerMapper, BuiltinCmapLibrary.Instance){}

    /// <summary>
    /// Parwse a PdfDirectObject from the encoding member of a font dictionary to a CMAP that can read the font.
    /// </summary>
    /// <param name="encoding">A name of a standard CMAP or a stream containing a CMAP</param>
    /// <returns>The parsed  CMAP or null if the CMAP cannot be parsed</returns>
    public async ValueTask<IReadCharacter?> ParseCMapAsync(PdfDirectObject encoding)
    {
        try
        {
            await ReadFromPdfValueAsync(encoding).CA();
            return new CMap(data);
        }
        catch (Exception )
        {
            return null;
        }
    }

    #region Cmap Sources
    private static readonly IPostscriptDictionary dict =
        PostscriptOperatorCollections.BaseLanguage()
            .With(CmapParserOperations.AddOperations);
    /// <summary>
    /// Create a new postscript engine that could evaluate a cmap.
    /// </summary>
    /// <returns>A new postscript engine initalized to this factory</returns>
    public PostscriptEngine CmapPostscriptEngine()
    {
        var parser = new PostscriptEngine(dict) { Tag = this }.WithImmutableStrings();
        parser.ResourceLibrary.Put("ProcSet", "CIDInit", PostscriptValueFactory.CreateDictionary());
        parser.ErrorDict.Put("undefined"u8, PostscriptValueFactory.CreateNull());
        return parser;
    }


    internal async ValueTask ReadFromPdfValueAsync(PdfDirectObject encoding)
    {
        await ReadFromCSharpStreamAsync(
            encoding.IsName ? cMapLibrary.CMapStreamFor(encoding) :
                await PdfToCSharpStreamAsync(encoding.Get<PdfStream>()).CA()).CA();
    }

    private async ValueTask<Stream> PdfToCSharpStreamAsync(PdfStream stream)
    {
        if (await stream.GetOrNullAsync(KnownNames.UseCMap).CA() is { IsNull: false } ancestor)
            await ReadFromPdfValueAsync(ancestor).CA();

        return await stream.StreamContentAsync().CA();
    }

    private async ValueTask ReadFromCSharpStreamAsync(Stream source)
    {
        try
        {
            await CmapPostscriptEngine().ExecuteAsync(source).CA();
        }
        finally
        {
            await source.DisposeAsync().CA();
        }

    }
    #endregion

    #region CMap Operators

    internal void AddCodespaces(ReadOnlySpan<PostscriptValue> values) => 
        values.ForEachGroup(AddSingleCodespace);

    private void AddSingleCodespace(PostscriptValue minValue, PostscriptValue maxValue) => 
        data.Add(new ByteRange(ToVariableBitCharacter(minValue), ToVariableBitCharacter(maxValue)));

    private VariableBitChar ToVariableBitCharacter(in PostscriptValue value) => 
        new(value.Get<StringSpanSource>().GetSpan());

    internal void AddNotDefRanges(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleNotDefRange);

    private void AddSingleNotDefRange(
        PostscriptValue min, PostscriptValue max, PostscriptValue value) => 
        AddRange(new ConstantCMapper(
            ToVariableBitCharacter(min), ToVariableBitCharacter(max), (uint)value.Get<long>()));

    internal void AddCidRanges(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleCidRange);

    private void AddSingleCidRange(
        PostscriptValue min, PostscriptValue max, PostscriptValue value) => 
        AddRange(new LinearCMapper(
            ToVariableBitCharacter(min), ToVariableBitCharacter(max), (uint)value.Get<long>()));

    internal void AddCidChars(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(AddSingleCidChar);

    private void AddSingleCidChar(PostscriptValue source, PostscriptValue dest)
    {
        var sourceVbc = ToVariableBitCharacter(source);
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

        HandleSpecViolation_NpRangeForMapper(mapper);
    }

    private void HandleSpecViolation_NpRangeForMapper(CMapMapperBase mapper)
    {
        data.Add(mapper.MinimumContainingRange());
    }

    internal void AddBaseFontRanges(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(CreateSingleBaseFontRange);

    private void CreateSingleBaseFontRange(PostscriptValue min, PostscriptValue max, PostscriptValue value)
    {
        var minCharacter = ToVariableBitCharacter(min);
        var maxCharacter = ToVariableBitCharacter(max);
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

    internal void AddBaseFontChars(ReadOnlySpan<PostscriptValue> values) =>
        values.ForEachGroup(CreateSingleBaseFontChar);

    private void CreateSingleBaseFontChar(PostscriptValue input, PostscriptValue value) => 
        CreateSingleBaseFontCharacter(ToVariableBitCharacter(input), value);

    private void CreateSingleBaseFontCharacter(VariableBitChar character, PostscriptValue value)
    {
        AddRange(
            value.IsLiteralName ?
                new ConstantCMapper(character, character, ValueForName(value)):
                new BaseFontConstantMapper(character, character, innerMapper, value));
    }

    private uint ValueForName(PostscriptValue value) => 
        namer.GetGlyphFor(value.AsPdfName());

    #endregion
}