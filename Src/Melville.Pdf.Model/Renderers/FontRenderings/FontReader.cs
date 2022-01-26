using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public readonly struct FontReader
{
    private readonly IDefaultFontMapper defaultMapper;

    public FontReader(IDefaultFontMapper defaultMapper)
    {
        this.defaultMapper = defaultMapper;
    }

    public async ValueTask<IRealizedFont> DictionaryToRealizedFont(
        PdfDictionary dict, IFontTarget target, double size)
    {
        return await FontMappingToRealizedFont(await DictionaryToMappingAsync(dict, target, size).ConfigureAwait(false), target, size).ConfigureAwait(false);
    }

    public async ValueTask<IRealizedFont> NameToRealizedFont(PdfName name, IFontTarget target, double size)
    {
        return await FontMappingToRealizedFont(NameToMapping(name, CharacterEncodings.Standard), target, size).ConfigureAwait(false);
    }

    private async Task<IRealizedFont> FontMappingToRealizedFont(IFontMapping mapping, IFontTarget target, double size)
    {
        return mapping.Font switch
        {
            IRealizedFont rf => rf,
            byte[] name => await FreeTypeFontFactory.SystemFont(
                name, size, target, mapping.Mapping, mapping.Bold, mapping.Oblique).ConfigureAwait(false),
            PdfStream s => await FreeTypeFontFactory.FromStream(s, size, target, mapping.Mapping).ConfigureAwait(false),
            _ => throw new NotSupportedException("Unknown font type")
        };
    }

    public async ValueTask<IFontMapping> DictionaryToMappingAsync(
        PdfDictionary font, IFontTarget target, double size)
    {
        var fontTypeKey = 
            (await font.GetOrDefaultAsync(KnownNames.Subtype, KnownNames.Type1).ConfigureAwait(false)).GetHashCode();
        
        if (fontTypeKey == KnownNameKeys.Type3)
            return await new Type3FontFactory(font, target, size).ParseAsync().ConfigureAwait(false);
        
        var encoding = await ComputeEncoding(font).ConfigureAwait(false);
        
        if ((await HasEmbeddedFontProgram(font).ConfigureAwait(false)) is {} fontAsStream)
            return new NamedDefaultMapping(fontAsStream, false, false, encoding);
        
        var baseFontName = await font.GetOrDefaultAsync(KnownNames.BaseFont, KnownNames.Helvetica).ConfigureAwait(false);
        return NameToMapping( ComputeOsFontName(fontTypeKey, baseFontName), encoding);
    }
    
    private PdfName ComputeOsFontName(int fontType, PdfName baseFontName) =>
        fontType == KnownNameKeys.MMType1?
            RemoveMultMasterSuffix(baseFontName):baseFontName;

    private async ValueTask<PdfStream?> HasEmbeddedFontProgram(PdfDictionary font) =>
        font.TryGetValue(KnownNames.FontDescriptor, out var descTask) && 
        (await descTask.ConfigureAwait(false)) is PdfDictionary descriptor &&
        await StreamFromDescriptorAsync(descriptor).ConfigureAwait(false) is { } fontAsStream?
            fontAsStream: null;

    private async ValueTask<PdfStream?> StreamFromDescriptorAsync(PdfDictionary descriptor) =>
        (descriptor.TryGetValue(KnownNames.FontFile2, out var ff2Task) ||
         descriptor.TryGetValue(KnownNames.FontFile3, out ff2Task)) 
       && (await ff2Task.ConfigureAwait(false)) is PdfStream ff2
            ? ff2
            : null;

    private PdfName RemoveMultMasterSuffix(PdfName baseFontName)
    {
        var source = baseFontName.Bytes.AsSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : NameDirectory.Get(source[..firstUnderscore]);
    }

    public IFontMapping NameToMapping(PdfName name, IByteToUnicodeMapping encoding) => 
        defaultMapper.MapDefaultFont(name, encoding);
    
    private async ValueTask<IByteToUnicodeMapping> ComputeEncoding(PdfDictionary font)
    {
        if (!font.TryGetValue(KnownNames.Encoding, out var EncodingTask))
            return CharacterEncodings.Standard;
        var encoding = await EncodingTask.ConfigureAwait(false);
        return await InterpretEncodingValue(encoding).ConfigureAwait(false);
    }

    private  ValueTask<IByteToUnicodeMapping> InterpretEncodingValue(PdfObject encoding) =>
        (encoding, encoding.GetHashCode()) switch
        {
            (_, KnownNameKeys.WinAnsiEncoding) => new(CharacterEncodings.WinAnsi),
            (_, KnownNameKeys.StandardEncoding) => new(CharacterEncodings.Standard),
            (_, KnownNameKeys.MacRomanEncoding) => new(CharacterEncodings.MacRoman),
            (_, KnownNameKeys.PdfDocEncoding) => new(CharacterEncodings.Pdf),
            (_, KnownNameKeys.MacExpertEncoding) => new(CharacterEncodings.MacExpert),
            (PdfDictionary dict, _) => ReadEncodingDictionary(dict),
            _ => throw new PdfParseException("Invalid encoding member on font.")
        };

    private async ValueTask<IByteToUnicodeMapping> ReadEncodingDictionary(PdfDictionary dict)
    {
        var baseEncoding = dict.TryGetValue(KnownNames.BaseEncoding, out var baseTask)
            ? await InterpretEncodingValue(await baseTask.ConfigureAwait(false)).ConfigureAwait(false)
            : CharacterEncodings.Standard;
        return dict.TryGetValue(KnownNames.Differences, out var arrTask) &&
               (await arrTask.ConfigureAwait(false)) is PdfArray arr?
            await CustomFontEncodingFactory.Create(baseEncoding, arr).ConfigureAwait(false): baseEncoding;
    }
}