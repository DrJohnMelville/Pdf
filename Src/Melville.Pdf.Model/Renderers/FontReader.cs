using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.FontMappings;

namespace Melville.Pdf.Model.Renderers;

public readonly struct FontReader
{
    private readonly IDefaultFontMapper defaultMapper;

    public FontReader(IDefaultFontMapper defaultMapper)
    {
        this.defaultMapper = defaultMapper;
    }

    public async ValueTask<IFontMapping> DictionaryToMappingAsync(PdfDictionary font)
    {
        return NameToMapping(await font.GetAsync<PdfName>(KnownNames.BaseFont));
    }

    public IFontMapping NameToMapping(PdfName name) => defaultMapper.MapDefaultFont(name);
}