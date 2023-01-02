using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// Clients use this interface to add objects to a PDF document which can then be written in the PDF format.
/// </summary>
public interface ILowLevelDocumentBuilder
{
    /// <summary>
    /// Create a PdfIndirectObject that points to a given object.  This does not add the object to the document.
    ///
    /// If value is null, this creates a promised object, and the 
    /// </summary>
    /// <param name="value">The PdfObject to be pointed to.</param>
    /// <returns>A PdfIndirectObject with a number than points to value.</returns>
    PdfIndirectObject AsIndirectReference(PdfObject value);

    /// <summary>
    /// Create a promise object that promises a literal object will be given at a later time.
    /// </summary>
    /// <returns>The initialized promise object.</returns>
    PromisedIndirectObject CreatePromiseObject();

    /// <summary>
    /// Create a promise object and add it to the current document. 
    /// </summary>
    /// <returns>The PromisedIndirectObject</returns>
    PromisedIndirectObject AddPromisedObject()
    {
        var ret = CreatePromiseObject();
        Add(ret);
        return ret;
    }

    /// <summary>
    /// Add an object to the document.  If it is not a PdfIndirectObject, one will be created to contain the object.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The PdfIndirectObject that refers to the added object.</returns>
    PdfIndirectObject Add(PdfObject item);

    /// <summary>
    /// Add an item to the document's trailer dictionary.
    /// </summary>
    /// <param name="key">The key to add the item under</param>
    /// <param name="item">The item to add.</param>
    void AddToTrailerDictionary(PdfName key, PdfObject item);

    /// <summary>
    /// Ensure that the document trailer has an ID array -- creating one if necessary.
    /// </summary>
    /// <returns>The ID array</returns>
    public PdfArray EnsureDocumentHasId();
    
    /// <summary>
    /// User password for encrypted documents.
    /// </summary>
    public string UserPassword { get; set; }
    
    /// <summary>
    /// Creates a objectstream context.  Objects added before the context is destroyed will be added to the object stream.
    /// </summary>
    /// <param name="dictionaryBuilder">The dictionary builder to use in constructing the object string.</param>
    /// <returns></returns>
    IDisposable ObjectStreamContext(DictionaryBuilder? dictionaryBuilder = null);

    /// <summary>
    /// Add an object with a specific object and generation number.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="objectNumber">The object number to add at</param>
    /// <param name="generation">The desired generation number</param>
    /// <returns></returns>
    internal PdfIndirectObject Add(PdfObject item, int objectNumber, int generation);

    internal PdfDictionary CreateTrailerDictionary();
    internal List<PdfIndirectObject> Objects { get; }
}