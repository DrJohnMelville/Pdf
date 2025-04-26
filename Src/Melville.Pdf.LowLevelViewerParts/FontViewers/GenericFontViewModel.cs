using System.Text;
using System.Windows;
using System.Windows.Media;
using Melville.Fonts;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.ReferenceDocuments.Utility;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class GenericFontViewModel
{
    [FromConstructor] public IGenericFont Font { get; }
    [FromConstructor] private IRealizedFont? realizedFont;
    [FromConstructor] public string Title { get; }
  
    [AutoNotify] private string fontName = "";
    [AutoNotify] private object? glyphViewModel = null;
    [AutoNotify] private object? cmapViewModel = null;
    [AutoNotify] private object? glyphNames = null;

    partial void OnConstructed()
    {
        LoadCmap();
    }
    public async void LoadCmap()
    {
        fontName = await Font.FontFamilyNameAsync();
        GlyphViewModel = new MultiGlyphViewModel((await Font.GetGlyphSourceAsync()));
        CmapViewModel = (await Font.GetCmapSourceAsync()).PrintCMap();
        GlyphNames = new MultiStringViewModel(LoadGlyphsAsync, "Glyph Names");
    }

    private async ValueTask<IReadOnlyList<string>> LoadGlyphsAsync() =>
        (await Font.GlyphNamesAsync())
        .Select((item,i)=> $"0x{i:X} => {item} (0x{MapNameToUnicode(item):X4})")
        .ToArray();
    
    private static uint MapNameToUnicode(string item) => GlyphNameToUnicodeMap.AdobeGlyphList.GetGlyphFor(PdfDirectObject.CreateName(item));

    private string textString = "";
    private string textHexString = "";
    [AutoNotify] public partial bool WideCharacters { get; set; }
    private string CharToHex(char c) => WideCharacters ? $"{(int)c:X4}" : $"{(int)c:X2}";
    [AutoNotify] public partial string Characters { get; private set; } //# = "";
    [AutoNotify] public partial string Glyphs { get; private set; } //#= "";

    public string TextString
    {
        get => textString;
        set => UpdateBothStrings(value, 
            string.Join(" ", value.Select(CharToHex)));
    }

    public string TextHexString
    {
        get => textHexString;
        set => UpdateBothStrings((WideCharacters?Encoding.BigEndianUnicode:Encoding.UTF8).GetString(value.BitsFromHex()), value);
    }

    private void UpdateBothStrings(string regString, string hexString)
    {
        textString = regString;
        textHexString = hexString;
        ((IExternalNotifyPropertyChanged)this).OnPropertyChanged(nameof(TextString));
        ((IExternalNotifyPropertyChanged)this).OnPropertyChanged(nameof(TextHexString));
        Render();
    }

    private void Render()
    {
        if (realizedFont is null) return;
        var text = new ReadOnlyMemory<byte>(TextHexString.BitsFromHex());
        StringBuilder chars = new();
        StringBuilder glyphs = new();
        var buffer = new uint[10];
        while (text.Length > 0)
        {
            var readChars = realizedFont.ReadCharacter.GetCharacters(ref text, buffer);
            foreach (var character in readChars.Span)
            {
                chars.Append($"[{character}, 0x{character:X4}] ");
                var glyph = realizedFont.MapCharacterToGlyph.GetGlyph(character);
                glyphs.Append($"[{glyph}, 0x{glyph:X4}] ");
            }
        }
        Characters = chars.ToString();
        Glyphs = glyphs.ToString();
    }
}