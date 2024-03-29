﻿using System;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// Create LowLevelDocumentBuilders for new or modified documents.
/// </summary>
public static class LowLevelDocumentBuilderFactory
{
    /// <summary>
    /// Create an ILowLevelDocumentCreator that can be used to build a new PDF document.
    /// </summary>
    public static ILowLevelDocumentCreator New() => new LowLevelDocumentBuilder();

    /// <summary>
    /// Create an ILowLevelDocumentCreator that can be used to append a modification trailer to an existing document.
    /// </summary>
    public static ILowLevelDocumentModifier Modify(this PdfLoadedLowLevelDocument doc) =>
        new LowLevelDocumentModifier(doc);
}

/// <summary>
/// Uszed to create new Pdf Documents
/// </summary>
public interface ILowLevelDocumentCreator : IPdfObjectCreatorRegistry
{
    /// <summary>
    /// Create a Pdf Document with a given version and the current contents.
    ///
    /// It is the consumer's responsibility to only use PDF features for the version declared.
    /// </summary>
    /// <param name="major">The major version of PDF standard for the document</param>
    /// <param name="minor">The minor version of PDF standard for the document.</param>
    /// <returns></returns>
    PdfLowLevelDocument CreateDocument(byte major = 1, byte minor = 7);

    /// <summary>
    /// Ensure that the document trailer has an ID array -- creating one if necessary.
    /// </summary>
    /// <returns>The ID array</returns>
    public PdfArray EnsureDocumentHasId();

}

/// <summary>
/// Methods to create or modify a PdfDocument
/// </summary>
public interface IPdfObjectCreatorRegistry
{

    /// <summary>
    /// Add an object to the document.  If it is not a PdfIndirectObject, one will be created to contain the object.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The PdfIndirectValue that refers to the added object.</returns>
    PdfIndirectObject Add(in PdfDirectObject item);

    /// <summary>
    /// Add an object with a specific object and generation number.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="objectNumber">The object number to add at</param>
    /// <param name="generation">The desired generation number</param>
    /// <returns></returns>
    internal PdfIndirectObject Add(in PdfDirectObject item, int objectNumber, int generation);

    /// <summary>
    /// Assign a new mapping to an existing PdfIndirectValue -- fails if the value is resolved.
    /// </summary>
    /// <param name="item">The indirect object to be reassigned</param>
    /// <param name="newValue">The value to be a ssigned.</param>
    /// <returns></returns>
    void Reassign(in PdfIndirectObject item, in PdfDirectObject newValue);


    /// <summary>
    /// Add an item to the document's trailer dictionary.
    /// </summary>
    /// <param name="key">The key to add the item under</param>
    /// <param name="item">The item to add.</param>
    void AddToTrailerDictionary(in PdfDirectObject key, in PdfIndirectObject item);

    /// <summary>
    /// Creates a objectstream context.  Objects added before the context is destroyed will be added to the object stream.
    /// </summary>
    /// <param name="dictionaryBuilder">The dictionary builder to use in constructing the object string.</param>
    /// <returns></returns>
    IDisposable ObjectStreamContext(DictionaryBuilder? dictionaryBuilder = null);
}

/// <summary>
/// Additional operations on the PdfCreatorRegistry
/// </summary>
public static class PdfObjectCreatorRegistry
{
    /// <summary>
    /// Adds value to the registry, ensuring that the return is an indirect reference to an object
    /// </summary>
    /// <param name="registry">The registry to add to.</param>
    /// <param name="value">The value to add</param>
    /// <returns>A guarenteed indirect reference to the passed value.</returns>
    public static PdfIndirectObject AddIfDirect(this IPdfObjectCreatorRegistry registry, PdfIndirectObject value) =>
        value.TryGetEmbeddedDirectValue(out var directVal) ? registry.Add(directVal) : value;
}