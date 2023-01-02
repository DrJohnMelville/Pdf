using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal interface IIndirectObjectResolver
{
    IReadOnlyDictionary<(int, int), PdfIndirectObject> GetObjects();
    PdfIndirectObject FindIndirect(int number, int generation);
    void AddLocationHint(PdfIndirectObject newItem);
}

/// <summary>
/// This interface represents an object that stores the locations of indirect PDF objects
/// </summary>
public interface IIndirectObjectRegistry
{
    /// <summary>
    /// Indicate that the specific block should be considered deleted.
    /// </summary>
    /// <param name="number">Object number of the deleted block.</param>
    /// <param name="next">Object number of the next deleted block in the deleted block chain.</param>
    /// <param name="generation">Generation number of the </param>
    void RegisterDeletedBlock(int number, ulong next, ulong generation);
    
    /// <summary>
    /// Registers an object of a type unknown to this version of the PDF parser.  This is either a
    /// corrupted file, or a file in a later version of PDF.  The suggested behavior is to map this
    /// object to PdfNull, since there is no way this parser can find the target object in a format
    /// it does not recognize.
    /// </summary>
    /// <param name="number">The object number</param>
    /// <param name="next">A datum associated with this object</param>
    /// <param name="generation">The generation of the object</param>
    void RegisterNullObject(int number, ulong next, ulong generation);
    
    /// <summary>
    /// Register a PdfObject at a given position in the PDF file.
    /// </summary>
    /// <param name="number">The object number</param>
    /// <param name="generation">The generation number</param>
    /// <param name="offset">The offset from the %%PDF header to the beginning of this object.</param>
    void RegisterIndirectBlock(int number, ulong generation, ulong offset);

    /// <summary>
    /// Register a PDF object at a given position in a given object stream.
    /// </summary>
    /// <param name="number">The object in the stream</param>
    /// <param name="referredStreamOrdinal">Object number of the referred to stream.</param>
    /// <param name="positionInStream">The </param>
    void RegisterObjectStreamBlock(int number, ulong referredStreamOrdinal, ulong positionInStream);
}