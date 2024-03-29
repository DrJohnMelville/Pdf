﻿namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

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
    void RegisterDeletedBlock(int number, int next, int generation);
    
    /// <summary>
    /// Register a PdfObject at a given position in the PDF file.
    /// </summary>
    /// <param name="number">The object number</param>
    /// <param name="generation">The generation number</param>
    /// <param name="offset">The offset from the %%PDF header to the beginning of this object.</param>
    void RegisterIndirectBlock(int number, int generation, long offset);

    /// <summary>
    /// Register a PDF object at a given position in a given object stream.
    /// </summary>
    /// <param name="number">The object in the stream</param>
    /// <param name="referredStreamOrdinal">Object number of the referred to stream.</param>
    /// <param name="positionInStream">The </param>
    void RegisterObjectStreamBlock(int number, int referredStreamOrdinal, int positionInStream);
}