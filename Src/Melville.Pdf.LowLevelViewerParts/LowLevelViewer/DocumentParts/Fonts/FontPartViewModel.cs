﻿using Melville.Fonts;
using Melville.Fonts.SfntParsers;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevelViewerParts.FontViewers;
using Melville.Pdf.LowLevelViewerParts.FontViewers.SfntViews;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

public partial class FontPartViewModel: DocumentPart
{
    private readonly PdfDictionary fontDic;
    [AutoNotify] private IRealizedFont? font;
    [AutoNotify] private GenericFontViewModel? genericFont;
    [AutoNotify] private object? specificFont;
    
    
    public FontPartViewModel(string title, PdfDictionary fontDic, IReadOnlyList<DocumentPart> children) : base(title, children)
    {
        this.fontDic = fontDic;
    }

    public override object? DetailView
    {
        get
        {
            LoadFont();
            return this;
        }
    }

    private async void LoadFont()
    {
        try
        {
            Font = await new FontReader(WindowsDefaultFonts.Instance).DictionaryToRealizedFontAsync(fontDic);
            var generic = Font.ExtractGenericFont();
            if (generic == null) return;
            
            GenericFont = new GenericFontViewModel(generic, Font, Font.FamilyName);
            SpecificFont = generic.CreateSpecificViewModel();
        }
        catch (PdfParseException e)
        {
            SpecificFont = $"""
                Font Parsing error ({e.Message})
                This may be because CID fonts are not real fonts, and cannot be parsed.
                """;
        }
        catch (Exception)
        {
        }
   }
}