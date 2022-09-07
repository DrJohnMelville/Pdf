using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model;

public readonly struct PdfReader
{
    private readonly IPasswordSource? passwordSource;
    private readonly IDefaultFontMapper fontFactory;

    public PdfReader(IPasswordSource? passwordSource = null, IDefaultFontMapper? fontFactory = null)
    {
        this.passwordSource = passwordSource;
        this.fontFactory = fontFactory ?? WindowsDefaultFonts.Instance;
    }
    public PdfReader(IDefaultFontMapper mapper): this(null, mapper){}


    public ValueTask<DocumentRenderer> ReadFrom(object input) => input switch
    {
        PdfDocument doc => ReadFrom(doc),
        PdfLowLevelDocument doc => ReadFrom(doc),
        DocumentRenderer dr => new(dr),
        _=> ReadFromLowLevelLateBound(input)
    };

    private async ValueTask<DocumentRenderer> ReadFromLowLevelLateBound(object input) => 
        await ReadFrom(await LowLevelReader().ReadFrom(input).CA()).CA();

    public async ValueTask<DocumentRenderer> ReadFromFile(string input) =>
        await ReadFrom(await LowLevelReader().ReadFromFile(input).CA()).CA();
    
    public async ValueTask<DocumentRenderer> ReadFrom(byte[] input) =>
        await ReadFrom(await LowLevelReader().ReadFrom(input).CA()).CA();
    
    public async ValueTask<DocumentRenderer> ReadFrom(Stream input) =>
        await ReadFrom(await LowLevelReader().ReadFrom(input).CA()).CA();

    private PdfLowLevelReader LowLevelReader() => new PdfLowLevelReader(passwordSource);

    public ValueTask<DocumentRenderer> ReadFrom(PdfLowLevelDocument doc) =>
        ReadFrom(new PdfDocument(doc));
    
    public ValueTask<DocumentRenderer> ReadFrom(PdfDocument doc) =>
        DocumentRendererFactory.CreateRendererAsync(doc, fontFactory);
}