using Melville.Hacks.Reflection;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.FormReader.AcroForms;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.FormReader;

/// <summary>
///  This facade is the gateway to the form reader.  A PdfDocument can be opened and
/// exposed as a form view.
/// </summary>
public static class FormReaderFacade
{
    /// <summary>
    /// Ope a file from a path as a PDF form document
    /// </summary>
    /// <param name="path">The path to a file containing the code</param>
    /// <param name="passwordSource">An interface to request passwords from the user.</param>
    /// <returns>An IPdfForm representation of the document</returns>
    public static ValueTask<IPdfForm> ReadFormFromFileAsync(
        string path, IPasswordSource? passwordSource = null) =>
        ReadFormAsync(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), passwordSource);

    /// <summary>
    /// Open any object that can represent a Pdf LowLevelDocument as a pdf form
    /// </summary>
    /// <param name="argument">Source for the low level document.</param>
    /// <param name="passwordSource">An interface to request passwords from the user.</param>
    /// <returns>An IPdfForm representation of the document</returns>
    public static async ValueTask<IPdfForm> ReadFormAsync(
        object argument, IPasswordSource? passwordSource = null) =>
        await ReadFormAsync(argument as PdfLowLevelDocument ??
                            await new PdfLowLevelReader(passwordSource).ReadFromAsync(argument));

    /// <summary>
    /// Form representation of a PdfLowLevelDocument
    /// </summary>
    /// <param name="doc">The low level document containing a pdf form.</param>
    /// <returns>An IPdfForm representation of the document</returns>
    public static async ValueTask<IPdfForm> ReadFormAsync(PdfLowLevelDocument doc)
    {
        var root = await doc.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root).CA();
        var acroForm = await root.GetAsync<PdfDictionary>(KnownNames.AcroForm).CA();

        var builder = new AcroFieldFactory();
        await builder.ParseAcroFieldsAsync(
            await acroForm.GetOrDefaultAsync(KnownNames.Fields, PdfArray.Empty).CA()).CA();
        await builder.ParseXfaFieldsAsync(
            await acroForm.GetOrDefaultAsync(KnownNames.XFA, PdfArray.Empty).CA()).CA();
        return builder.Build(doc, await GetFormAppearanceDefaultAsync(acroForm).CA());
    }

    private static ValueTask<PdfDirectObject> GetFormAppearanceDefaultAsync(PdfDictionary acroForm) =>
        acroForm.GetOrDefaultAsync(KnownNames.DA, PdfDirectObject.CreateString(""u8));
}