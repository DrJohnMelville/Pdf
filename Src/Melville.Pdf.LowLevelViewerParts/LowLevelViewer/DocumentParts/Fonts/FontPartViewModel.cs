using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Fonts;

public partial class FontPartViewModel: DocumentPart
{
    private readonly PdfDictionary fontDic;
    [AutoNotify] private IRealizedFont? font;
    
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
            Font = await new FontReader(WindowsDefaultFonts.Instance).DictionaryToRealizedFont(fontDic);
        }
        catch (Exception)
        {
        }
   }
}