using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model;

/// <summary>
/// Factory class that constructs a DocumentRenderer from a variety of legal source types
/// </summary>
public readonly partial struct PdfReader
{
    /// <summary>
    /// IPasswordSource to query when attempting to open a password protected
    /// PDF file.  If this parameter is null, attempts to open a password protected PDF throw an exception
    /// </summary>
    [FromConstructor]
    private readonly IPasswordSource? passwordSource;
    /// <summary>
    /// IDefaultFontMapper instance that maps font names to font declarations for
    /// fonts not embedded in the PDF file.  If null, defaults to WindowsDefaultFonts that looks for
    /// fonts in the %windir%/Fonts directory.
    /// </summary>
    [FromConstructor]
    private readonly IDefaultFontMapper fontFactory;

    /// <summary>
    /// Construct a PdfReader that will read unencrypted files with the default
    /// font mappings.
    /// </summary>
    public PdfReader() : this(WindowsDefaultFonts.Instance) { }

    /// <summary>
    /// Construct a PDF reader that will read encrypted files with default font mapping.
    /// </summary>
    /// <param name="passwordSource">IPasswordSource to query when attempting to open a password protected
    /// PDF file.  If this parameter is null, attempts to open a password protected PDF throw an exception</param>
    public PdfReader(IPasswordSource? passwordSource): 
        this(passwordSource, WindowsDefaultFonts.Instance) {}

    /// <summary>
    /// Construct a PDF reader that will throw when attempting to open a password protected PDF.
    /// </summary>
    /// <param name="mapper">IDefaultFontMapper instance that maps font names to font declarations for
    /// fonts not embedded in the PDF file.  If null, defaults to WindowsDefaultFonts that looks for
    /// fonts in the %windir%/Fonts directory.</param>
    public PdfReader(IDefaultFontMapper mapper): this(null, mapper){}
    
    /// <summary>
    /// Read a pdf file from a variety of object types.
    ///
    /// This typeless method supports the PdfViewer control, which can accept multiple types via its Source property.
    /// In code, call the type-specific overloads when possible.
    /// </summary>
    /// <param name="input">A filename, byte array, stream, PdfLowLevelDocument, PdfDocument, or DocumentRenderer to read from</param>
    /// <returns>A DocumentRenderer that can render pages from the given source.</returns>
    public ValueTask<DocumentRenderer> ReadFrom(object input) =>
        input switch
        {
            PdfDocument doc => ReadFrom(doc),
            PdfLowLevelDocument doc => ReadFrom(doc),
            DocumentRenderer dr => new(dr),
            _ => ReadFromLowLevelLateBound(input)
        };

    private async ValueTask<DocumentRenderer> ReadFromLowLevelLateBound(object input) => 
        await ReadFrom(await LowLevelReader().ReadFromAsync(input).CA()).CA();

    /// <summary>
    /// Read a pdf file into a DocumentRenderer
    /// </summary>
    /// <param name="input">File name of the PDF file.</param>
    /// <returns>A DocumentRenderer that can render pages from the given source.</returns>
    public async ValueTask<DocumentRenderer> ReadFromFile(string input) =>
        await ReadFrom(await LowLevelReader().ReadFromFileAsync(input).CA()).CA();
    
    /// <summary>
    /// Read a pdf file into a DocumentRenderer
    /// </summary>
    /// <param name="input">A byte array containing the PDF data.</param>
    /// <returns>A DocumentRenderer that can render pages from the given source.</returns>
    public async ValueTask<DocumentRenderer> ReadFrom(byte[] input) =>
        await ReadFrom(await LowLevelReader().ReadFromAsync(input).CA()).CA();
    
    /// <summary>
    /// Read a pdf file into a DocumentRenderer
    /// </summary>
    /// <param name="input">A stream containing the PDF data.  The stream must support reading and seeking.</param>
    /// <returns>A DocumentRenderer that can render pages from the given source.</returns>
    public async ValueTask<DocumentRenderer> ReadFrom(Stream input) =>
        await ReadFrom(await LowLevelReader().ReadFromAsync(input).CA()).CA();

    private PdfLowLevelReader LowLevelReader() => new(passwordSource);

    /// <summary>
    /// Read a pdf file into a DocumentRenderer
    /// </summary>
    /// <param name="doc">A PdfLowLevelDocument representing the PDF data to display.</param>
    /// <returns>A DocumentRenderer that can render pages from the given source.</returns>
    public ValueTask<DocumentRenderer> ReadFrom(PdfLowLevelDocument doc) =>
        ReadFrom(new PdfDocument(doc));
    
    
    /// <summary>
    /// Read a pdf file into a DocumentRenderer
    /// </summary>
    /// <param name="doc">A PdfDocument representing the PDF data to display.</param>
    /// <returns>A DocumentRenderer that can render pages from the given source.</returns>
    public ValueTask<DocumentRenderer> ReadFrom(PdfDocument doc) =>
        DocumentRendererFactory.CreateRendererAsync(doc, fontFactory);
}