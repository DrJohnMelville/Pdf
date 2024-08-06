using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel;

/// <summary>
/// Reads a low level PDF document from a variety of different CodeSource objects.
/// </summary>
public readonly struct PdfLowLevelReader
{
    private readonly IPasswordSource passwordSource;

    /// <summary>
    /// Create a low-level PDF reader.
    /// </summary>
    /// <param name="passwordSource">An interface that allows the reader to query the user for a password.</param>
    public PdfLowLevelReader(IPasswordSource? passwordSource = null)
    {
        this.passwordSource = passwordSource ?? NullPasswordSource.Instance;
    }


    /// <summary>
    /// Weakly typed method to create a low level document from a string, byte array, or stream
    /// </summary>
    /// <param name="argument">The CodeSource to read from</param>
    /// <returns> The PdfLowLevelDocument read;</returns>
    /// <exception cref="ArgumentException">If the CodeSource object is not a string, stream, oy
    /// byte array.</exception>
    public ValueTask<PdfLoadedLowLevelDocument> ReadFromAsync(object argument) => argument switch
    {
        string s => ReadFromFileAsync(s),
        byte[] s => ReadFromAsync(s),
        Stream s => ReadFromAsync(s),
        IMultiplexSource ims => ReadFromAsync(ims),
        _ => throw new ArgumentException("Must be string, byte array, or stream.", nameof(argument))
    };

    /// <summary>
    /// Read a PDF document from a file.
    /// </summary>
    /// <param name="path">Path name for the file to read from.</param>
    /// <returns> The PdfLowLevelDocument read;</returns>
    public ValueTask<PdfLoadedLowLevelDocument> ReadFromFileAsync(string path) =>
        ReadFromAsync(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));

    /// <summary>
    /// Load a PdfLowLevelDocument from a byte array.  After the call, the PdfLowLevelDocument
    /// owns the byte array, and the caller may not change it.
    /// </summary>
    /// <param name="input">The byte array to read from</param>
    /// <returns> The PdfLowLevelDocument read;</returns>
    public ValueTask<PdfLoadedLowLevelDocument> ReadFromAsync(byte[] input) =>
        ReadFromAsync(MultiplexSourceFactory.Create(input));

    /// <summary>
    /// Load a PdfLowLevelDocument from a stream.  After the call, the PdfLowLevelDocument
    /// owns the stream, and the caller may not change it.
    /// </summary>
    /// <param name="input">The stream to read from</param>
    /// <returns> The PdfLowLevelDocument read;</returns>
    public ValueTask<PdfLoadedLowLevelDocument> ReadFromAsync(Stream input) =>
            ReadFromAsync(MultiplexSourceFactory.Create(input));

    /// <summary>
    /// Load a PdfLowLevelDocument from a stream.  After the call, the PdfLowLevelDocument
    /// owns the stream, and the caller may not change it.
    /// </summary>
    /// <param name="input">The stream to read from</param>
    /// <returns> The PdfLowLevelDocument read;</returns>
    public ValueTask<PdfLoadedLowLevelDocument> ReadFromAsync(IMultiplexSource input) =>
        RandomAccessFileParser.ParseAsync(
            new ParsingFileOwner(input, passwordSource));
}