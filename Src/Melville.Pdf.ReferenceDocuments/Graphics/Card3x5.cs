﻿using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics;

public abstract class Card3x5: CreatePdfParser
{
    protected Card3x5(string helpText) : base(helpText)
    {
    }

    public override async ValueTask WritePdfAsync(Stream target)
    {
        var docCreator = new PdfDocumentCreator();
        await AddContentToDocumentAsync(docCreator);
        await docCreator.CreateDocument().WriteToAsync(target);
    }

    protected virtual async ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
    {
        var page = docCreator.Pages.CreatePage();
        page.AddBox(BoxName.CropBox, new PdfRect(0, 0, 5 * 72, 3 * 72));
        await SetPagePropertiesAsync(page);
        await page.AddToContentStreamAsync(CreateContentStreamAsync);
    }

    private ValueTask CreateContentStreamAsync(ContentStreamWriter csw)
    {
        DoPainting(csw);
        return DoPaintingAsync(csw);
    }
    
    protected virtual ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        SetPageProperties(page);
        return ValueTask.CompletedTask;
    }
    protected virtual void SetPageProperties(PageCreator page)
    {
    }

    protected virtual void DoPainting(ContentStreamWriter csw)
    {
    }

    protected virtual ValueTask DoPaintingAsync(ContentStreamWriter csw) => ValueTask.CompletedTask;
}