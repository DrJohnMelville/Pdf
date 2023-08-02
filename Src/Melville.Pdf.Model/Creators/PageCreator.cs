using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// This class creates PDF pages
/// </summary>
public class PageCreator: ContentStreamCreator
{
    private readonly List<PdfStream> streamSegments = new();
    private PdfIndirectObject? promisedPageObject;
    internal PageCreator(IObjectStreamCreationStrategy objStreamStrategy) : base(objStreamStrategy)
    {
        MetaData.WithItem(KnownNames.Type, KnownNames.Page);
    }

    /// <inheritdoc />
    public override void AddToContentStream(DictionaryBuilder builder, MultiBufferStreamSource data) => 
        streamSegments.Add(builder.AsStream(data));

    /// <inheritdoc />
    public override (PdfIndirectObject Reference, int PageCount)
        ConstructItem(IPdfObjectCreatorRegistry creator, PdfIndirectObject parent)
    {
        if (parent.TryGetEmbeddedDirectValue(out var _)) 
            throw new ArgumentException("Pages must have a parent.");
        MetaData.WithItem(KnownNames.Parent, parent);
        return base.ConstructItem(creator, parent);
    }

    /// <inheritdoc />
    protected override PdfIndirectObject CreateFinalObject(IPdfObjectCreatorRegistry creator)
    {
        TryAddContent(creator);
        var page = MetaData.AsDictionary();
        return AddPageDictionaryToRegistry(creator, page);
    }

    private PdfIndirectObject AddPageDictionaryToRegistry(IPdfObjectCreatorRegistry creator, PdfDictionary dict)
    {
        if (!promisedPageObject.HasValue)            
            return creator.Add(dict);

        creator.Reassign(promisedPageObject.Value, dict);
        return promisedPageObject.Value;
    }

    /// <summary>
    /// Create an indirect object that will eventually become the Page dictionary.  This is used
    /// wehn the page definition needs to refer back to the page object.  A client can get the
    /// promised indirect object that will later be filled in with the page value.
    /// </summary>
    /// <param name="builder">The IPdfObjectRegistry from which to create the promise object</param>
    /// <returns>the promise object</returns>
    public PdfIndirectObject InitializePromiseObject(IPdfObjectCreatorRegistry builder)
    {
        if (promisedPageObject.HasValue)
            throw new InvalidOperationException("Already created a promise object.");
        var target = builder.Add(PdfDirectObject.CreateNull());
        promisedPageObject = target;
        return target;
    }

    private void TryAddContent(IPdfObjectCreatorRegistry creator)
    {
        if (streamSegments.Count > 0)
            MetaData.WithItem(KnownNames.Contents, CreateContents(creator));
    }

    private PdfIndirectObject CreateContents(IPdfObjectCreatorRegistry creator) =>
        streamSegments.Count == 1
            ? CreateStreamSegment(creator, streamSegments[0])
            : new PdfArray(streamSegments.Select(i => CreateStreamSegment(creator, i)).ToArray());

    private PdfIndirectObject CreateStreamSegment(IPdfObjectCreatorRegistry creator, PdfStream stream) => 
        creator.Add(stream);

    /// <summary>
    /// Add a last modified time to the page object.
    /// </summary>
    /// <param name="dateAndTime">The last modified time to add</param>
    public void AddLastModifiedTime(PdfTime dateAndTime) => 
        MetaData.WithItem(KnownNames.LastModified, dateAndTime.AsPdfBytes());
}