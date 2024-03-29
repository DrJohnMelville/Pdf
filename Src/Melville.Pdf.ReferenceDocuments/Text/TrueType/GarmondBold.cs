﻿using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.ReferenceDocuments.Text.TrueType;

public class GarmondBold : FontDefinitionTest
{
    public GarmondBold() : base("Use a builtin font with bold font descriptor")
    {
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.BaseFont, PdfDirectObject.CreateName("GarmondBold"))
            .WithItem(KnownNames.FontDescriptor,
                arg.Add(new DictionaryBuilder()
                    .WithItem(KnownNames.Flags, (long)FontFlags.ForceBold)
                    .AsDictionary())
                )
            .AsDictionary();
}
public class GarmondItalic : FontDefinitionTest
{
    public GarmondItalic() : base("Use builtin font with italic descriptor")
    {
    }

    protected override PdfDirectObject CreateFont(IPdfObjectCreatorRegistry arg) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.TrueType)
            .WithItem(KnownNames.BaseFont, PdfDirectObject.CreateName("GarmondItalic"))
            .WithItem(KnownNames.FontDescriptor,
                arg.Add(new DictionaryBuilder()
                    .WithItem(KnownNames.Flags, (long)FontFlags.Italic)
                    .AsDictionary())
                )
            .AsDictionary();
}