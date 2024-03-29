﻿using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class EmbeddedTrueType : FontDefinitionTest
{
    public EmbeddedTrueType() : base("Render with an embedded truetype font.")
    {
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var fontStream = GetType().Assembly.GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Text.Zev.ttf")!;
        var stream = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Length1, fontStream.Length)
            .WithFilter(FilterName.FlateDecode)
            .AsStream(fontStream));
        var widthArray = arg.Add(new PdfArray(Enumerable.Repeat<PdfIndirectObject>(600, 256).ToArray()));
        var descrip = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.FontDescriptor)
            .WithItem(KnownNames.Flags, (int)FontFlags.NonSymbolic)
            .WithItem(KnownNames.FontBBox, new PdfArray(-511, -250, 1390, 750))
            .WithItem(KnownNames.FontFile2, stream)
            .AsDictionary());
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.FontDescriptor, descrip)
            .WithItem(KnownNames.Widths, widthArray)
            .WithItem(KnownNames.BaseFont, PdfDirectObject.CreateName("Zev"))
            .AsDictionary();
    }
}