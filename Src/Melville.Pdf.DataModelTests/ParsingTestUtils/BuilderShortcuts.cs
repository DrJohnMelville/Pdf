using System;
using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public static class BuilderShortcuts
{
    public static async Task<Stream> AsStreamAsync(this ILowLevelDocumentCreator creator, byte major = 1, byte minor = 7)
    {
        var doc = creator.CreateDocument(major, minor);
        var output = new MultiBufferStream();
        await doc.WriteToAsync(output);
        return output.CreateReader();
    }

    public static async Task<byte[]> AsBytesAsync(this ILowLevelDocumentCreator creator, byte major = 1, byte minor = 7) => 
        (await AsStreamAsync(creator, major, minor)).ReadToArray();

    public static async Task<IFile> AsFileAsync(this ILowLevelDocumentCreator creator, byte major = 1, byte minor = 7) =>
        new MemoryFile("S:\\d.pdf", await creator.AsBytesAsync(major, minor));

    public static async Task<String> AsStringAsync(this ILowLevelDocumentCreator creator, byte major = 1, byte minor = 7) =>
        ExtendedAsciiEncoding.ExtendedAsciiString(await creator.AsBytesAsync(major, minor));

    public static async Task<Stream> AsStreamAsync(this PdfLowLevelDocument creator)
    {
        var doc = creator;
        var output = new MultiBufferStream();
        await doc.WriteToAsync(output);
        return output.CreateReader();
    }

    public static async Task<byte[]> AsBytesAsync(this PdfLowLevelDocument creator) => 
        (await AsStreamAsync(creator)).ReadToArray();

    public static async Task<IFile> AsFileAsync(this PdfLowLevelDocument creator) =>
        new MemoryFile("S:\\d.pdf", await creator.AsBytesAsync());

    public static async Task<String> AsStringAsync(this PdfLowLevelDocument creator) =>
        ExtendedAsciiEncoding.ExtendedAsciiString(await creator.AsBytesAsync());
}