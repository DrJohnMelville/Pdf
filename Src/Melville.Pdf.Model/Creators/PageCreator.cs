using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// This class creates PDF pages
/// </summary>
public class PageCreator: ContentStreamCreator
{
    private readonly List<PdfValueStream> streamSegments = new();
    private PdfIndirectValue? promisedPageObject;
    internal PageCreator(IObjectStreamCreationStrategy objStreamStrategy) : base(objStreamStrategy)
    {
        MetaData.WithItem(KnownNames.TypeTName, KnownNames.PageTName);
    }

    /// <inheritdoc />
    public override void AddToContentStream(ValueDictionaryBuilder builder, MultiBufferStreamSource data) => 
        streamSegments.Add(builder.AsStream(data));

    /// <inheritdoc />
    public override (PdfIndirectValue Reference, int PageCount)
        ConstructItem(IPdfObjectCreatorRegistry creator, PdfIndirectValue parent)
    {
        if (parent.TryGetEmbeddedDirectValue(out var _)) 
            throw new ArgumentException("Pages must have a parent.");
        MetaData.WithItem(KnownNames.ParentTName, parent);
        return base.ConstructItem(creator, parent);
    }

    /// <inheritdoc />
    protected override PdfIndirectValue CreateFinalObject(IPdfObjectCreatorRegistry creator)
    {
        TryAddContent(creator);
        var page = MetaData.AsDictionary();
        AddPageDictionaryToRegistry(creator, page);
        return page;
    }

    private void AddPageDictionaryToRegistry(IPdfObjectCreatorRegistry creator, PdfValueDictionary dict)
    {
        if (promisedPageObject.HasValue)
            creator.Reassign(promisedPageObject.Value, dict);
        else
            creator.Add(dict);
    }

    /// <summary>
    /// Create an indirect object that will eventually become the Page dictionary.  This is used
    /// wehn the page definition needs to refer back to the page object.  A client can get the
    /// promised indirect object that will later be filled in with the page value.
    /// </summary>
    /// <param name="builder">The IPdfObjectRegistry from which to create the promise object</param>
    /// <returns>the promise object</returns>
    public PdfIndirectValue InitializePromiseObject(IPdfObjectCreatorRegistry builder)
    {
        if (promisedPageObject.HasValue)
            throw new InvalidOperationException("Already created a promise object.");
        var target = builder.Add(PdfDirectValue.CreateNull());
        promisedPageObject = target;
        return target;
    }

    private void TryAddContent(IPdfObjectCreatorRegistry creator)
    {
        if (streamSegments.Count > 0)
            MetaData.WithItem(KnownNames.ContentsTName, CreateContents(creator));
    }

    private PdfIndirectValue CreateContents(IPdfObjectCreatorRegistry creator) =>
        streamSegments.Count == 1
            ? CreateStreamSegment(creator, streamSegments[0])
            : new PdfValueArray(streamSegments.Select(i => CreateStreamSegment(creator, i)).ToArray());

    private PdfIndirectValue CreateStreamSegment(IPdfObjectCreatorRegistry creator, PdfValueStream stream) => 
        creator.Add(stream);

    /// <summary>
    /// Add a last modified time to the page object.
    /// </summary>
    /// <param name="dateAndTime">The last modified time to add</param>
    public void AddLastModifiedTime(PdfTime dateAndTime) => 
        MetaData.WithItem(KnownNames.LastModifiedTName, dateAndTime.AsPdfBytes());
}