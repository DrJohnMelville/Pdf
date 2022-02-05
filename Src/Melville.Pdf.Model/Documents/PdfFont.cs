﻿using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

[Flags]
public enum FontFlags: int
{
    None =  0,
    FixedPitch = 1,
    Serif = 1 << 1,
    Symbolic = 1 << 2,
    Script = 1 << 3,
    NonSymbolic = 1 << 5,
    Italic = 1 <<6,
    AllCap = 1 << 16,
    SmallCap = 1 << 17,
    ForceBold = 1<<18
}

public readonly struct PdfFont
{
    public PdfFont(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }

    public readonly PdfDictionary LowLevel { get; }

    public ValueTask<PdfName> SubTypeAsync() => 
        LowLevel.GetOrDefaultAsync(KnownNames.Subtype, KnownNames.Type1);
    public ValueTask<PdfObject?> EncodingAsync() => LowLevel.GetOrNullAsync<PdfObject>(KnownNames.Encoding);

    public ValueTask<PdfDictionary?> DescriptorAsync() =>
        LowLevel.GetOrNullAsync<PdfDictionary>(KnownNames.FontDescriptor);

    public async ValueTask<FontFlags> FontFlagsAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            ? (FontFlags)(await descriptor.GetOrDefaultAsync(KnownNames.Flags, 0).CA())
            : FontFlags.None;

    public async ValueTask<PdfStream?> EmbeddedStreamAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            && (descriptor.TryGetValue(KnownNames.FontFile2, out var retTask) ||
              descriptor.TryGetValue(KnownNames.FontFile3, out retTask) )? 
                await retTask.CA() as PdfStream : null;

    public ValueTask<PdfName> BaseFontNameAsync() =>
        LowLevel.GetOrDefaultAsync(KnownNames.BaseFont, KnownNames.Helvetica);

    public async ValueTask<PdfName> OsFontNameAsync() =>
        ComputeOsFontName((await SubTypeAsync().CA()).GetHashCode(), await BaseFontNameAsync().CA());
    
    private PdfName ComputeOsFontName(int fontType, PdfName baseFontName) =>
        fontType == KnownNameKeys.MMType1?
            RemoveMultMasterSuffix(baseFontName):baseFontName;
    
    private PdfName RemoveMultMasterSuffix(PdfName baseFontName)
    {
        var source = baseFontName.Bytes.AsSpan();
        var firstUnderscore = source.IndexOf((byte)'_');
        return firstUnderscore < 0 ? baseFontName : NameDirectory.Get(source[..firstUnderscore]);
    }
}
