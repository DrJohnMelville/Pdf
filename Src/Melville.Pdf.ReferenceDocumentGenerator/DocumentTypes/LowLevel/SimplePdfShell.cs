using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel
{
    public static class SimplePdfShell
    {
        public static ILowLevelDocumentCreator Generate(
            int major, int minor, 
            Func<ILowLevelDocumentCreator,PdfIndirectReference, IReadOnlyList<PdfObject>> createPages)
        {
            var builder = new LowLevelDocumentCreator();
            builder.SetVersion((byte)major, (byte)minor);
            var catalog = builder.AsIndirectReference();
            var outlines = builder.AsIndirectReference(builder.NewDictionary(
                (KnownNames.Type, KnownNames.Outlines), (KnownNames.Count, new PdfInteger(0))));
            var pages = builder.AsIndirectReference();
            var page = builder.AsIndirectReference();
            var stream = builder.AsIndirectReference(builder.NewStream("... Page0marking operators ..."));
            var procset = builder.AsIndirectReference(new PdfArray(KnownNames.PDF));
            builder.AssignValueToReference(page, builder.NewDictionary(
                (KnownNames.Type, KnownNames.Page),
                (KnownNames.Parent, pages),
                (KnownNames.MediaBox, new PdfArray(
                    new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792))),
                (KnownNames.Contents, stream),
                (KnownNames.Resources, builder.NewDictionary((KnownNames.ProcSet, procset)))));
            builder.AssignValueToReference(pages, builder.NewDictionary(
                (KnownNames.Type, KnownNames.Pages),
                (KnownNames.Kids, new PdfArray(createPages(builder, pages))),
                (KnownNames.Count, new PdfInteger(1))
                ));
            builder.AssignValueToReference(catalog, builder.NewDictionary(
                (KnownNames.Type, KnownNames.Catalog),
                (KnownNames.Outlines, outlines),
                (KnownNames.Pages, pages)
                ));

            builder.Add(catalog);
            builder.Add(outlines);
            builder.Add(pages);
            builder.Add(page);
            builder.Add(stream);
            builder.Add(procset);
            
            builder.AddToTrailerDictionary(KnownNames.Root, catalog);
            return builder;       
        }

    }
}