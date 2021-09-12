using System;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    [Flags]
    public enum PdfPermission
    {
        None = 0,
        ChangeEncryption = 1 << 2,
        PrintDocument = 1 << 2,
        ModifyDocument = 1 << 3,
        CopyFrom = 1 << 4,
        Annotate = 1 << 5,
        FillForms = 1 << 8,
        ExtractTextForDisabilitis = 1 << 9,
        AssembleDocument = 1 << 10,
        PrintFaithfulCopy = 1 << 11,
    }
}