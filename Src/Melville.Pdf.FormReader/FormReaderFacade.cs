using Melville.Pdf.FormReader.AcroForms;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.FormReader;

public static class FormReaderFacade
{
    public static ValueTask<IPdfForm> ReadFormAsync(
        string path, IPasswordSource? passwordSource = null) =>
        ReadFormAsync(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), passwordSource);

    public static async ValueTask<IPdfForm> ReadFormAsync(
        object argument, IPasswordSource? passwordSource = null) =>
        await ReadFormAsync(argument as PdfLowLevelDocument ??
                            await new PdfLowLevelReader(passwordSource).ReadFromAsync(argument));

    public static async ValueTask<IPdfForm> ReadFormAsync(PdfLowLevelDocument doc)
    {
        var root = await doc.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);
        var acroForm = await root.GetAsync<PdfDictionary>(KnownNames.AcroForm);
        var fields =await acroForm.GetAsync<PdfArray>(KnownNames.Fields);
        return new AcroPdfForm(doc, await AcroFieldFactory.ParseFieldsAsync(fields.RawItems));
    }
}