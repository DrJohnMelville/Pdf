using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel
{
    public static class SimplePdfShell
    {
        public static async ValueTask<ILowLevelDocumentCreator> Generate(
            int major, int minor, 
            Func<ILowLevelDocumentCreator,PdfIndirectReference, 
                ValueTask<IReadOnlyList<PdfObject>>> createPages)
        {
            var builder = new LowLevelDocumentCreator();
            builder.SetVersion((byte)major, (byte)minor);
            var catalog = builder.AsIndirectReference();
            var outlines = builder.AsIndirectReference(builder.NewDictionary(
                (KnownNames.Type, KnownNames.Outlines), (KnownNames.Count, new PdfInteger(0))));
            var pages = builder.AsIndirectReference();
            var pageGroup = await createPages(builder, pages);
            builder.AssignValueToReference(pages, builder.NewDictionary(
                (KnownNames.Type, KnownNames.Pages),
                (KnownNames.Kids, new PdfArray(pageGroup)),
                (KnownNames.Count, new PdfInteger(pageGroup.Count))
                ));
            builder.AssignValueToReference(catalog, builder.NewDictionary(
                (KnownNames.Type, KnownNames.Catalog),
                (KnownNames.Outlines, outlines),
                (KnownNames.Pages, pages)
                ));

            builder.Add(catalog);
            builder.Add(outlines);
            builder.Add(pages);
            
            builder.AddToTrailerDictionary(KnownNames.Root, catalog);
            return builder;       
        }

    }
}