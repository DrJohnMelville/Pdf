using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts.Fonts;

public partial class FontPart: DocumentPart
{
    private readonly PdfDictionary fontDic;
    [AutoNotify] private IRealizedFont font;
    
    public FontPart(string title, PdfDictionary fontDic, IReadOnlyList<DocumentPart> children) : base(title, children)
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

    private void LoadFont()
    {
    }
}