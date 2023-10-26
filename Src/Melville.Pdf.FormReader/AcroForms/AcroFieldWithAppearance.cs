using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.FormReader.AcroForms;

[FromConstructor]
internal partial class AcroFieldWithAppearance: AcroFormField
{
    protected async ValueTask ReplaceTextAppearanceAsync(ICanReplaceObjects target, PdfDirectObject formAppearanceString)
    {
        if (!sourceDictionary.TryGetValue(KnownNames.AP, out var dictTask)) return;
        if ((await dictTask).TryGet(out PdfDictionary? ap) &&
            ap.RawItems.TryGetValue(KnownNames.N, out var apStreamRef))
            await ReplaceAppearanceStreamAsync(apStreamRef,
                await sourceDictionary.GetOrDefaultAsync(KnownNames.DA,
                    formAppearanceString), target);
    }

    private async ValueTask ReplaceAppearanceStreamAsync(
        PdfIndirectObject apStreamRef, PdfDirectObject appearanceString,
        ICanReplaceObjects target)
    {
        var textValue = Value.DecodedString();

        var template = await apStreamRef.LoadValueAsync();
        if (!template.TryGet(out PdfStream? templateStream)) return;
        var builder = new DictionaryBuilder(templateStream.RawItems);
        var original = new MemoryStream(); 
        await (await templateStream.StreamContentAsync().CA()).CopyToAsync(original).CA();

        var stm = new MultiBufferStream();
        WriteNewAppearnce(appearanceString, stm, textValue, AsSpan(original));
        target.ReplaceReferenceObject(apStreamRef, builder.AsStream(stm));
    }

    private static Span<byte> AsSpan(MemoryStream original) => 
        original.GetBuffer().AsSpan(0,(int)original.Length);

    private static void WriteNewAppearnce(
        PdfDirectObject appearanceString, MultiBufferStream stm, string textValue,
        in ReadOnlySpan<byte> priorText)
    {

        stm.Write(PrefixFrom(priorText));
        stm.Write("/Tx BMC\nq\nBT\n2 2.106 Td\n"u8);
        stm.Write(appearanceString.Get<StringSpanSource>().GetSpan());
        stm.Write(" "u8);

        WriteEncodedStringValue(stm, textValue);

        stm.Write(" Tj ET Q EMC"u8);
        stm.Write(SuffixFrom(priorText));
    }

    private static void WriteEncodedStringValue(MultiBufferStream stm, string textValue)
    {
        Span<byte> translatedSpan = stackalloc byte[textValue.Length];
        PdfDocEncoding.Instance.GetBytes(textValue, translatedSpan);
        Span<byte> formattedSpan = stackalloc byte[textValue.Length * 2];
        stm.Write(WritePdfString.ToSpan(translatedSpan, formattedSpan));
    }


    private static ReadOnlySpan<byte> PrefixFrom(in ReadOnlySpan<byte> priorText)
    {
        var target = priorText.IndexOf("/Tx BMC"u8);
        return target < 0? ReadOnlySpan<byte>.Empty: priorText[..target];
    }
    private static ReadOnlySpan<byte> SuffixFrom(in ReadOnlySpan<byte> priorText)
    {
        var target = priorText.IndexOf("EMC"u8);
        return (target + 3) >= priorText.Length? ReadOnlySpan<byte>.Empty: 
            priorText[(target + 3)..];
    }
}