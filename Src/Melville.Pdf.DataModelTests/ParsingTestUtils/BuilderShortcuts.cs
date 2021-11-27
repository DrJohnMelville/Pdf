using System;
using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public static class BuilderShortcuts
{
    public static async Task<byte[]> AsBytesAsync(this ILowLevelDocumentCreator creator)
    {
        return (await AsStreamAsync(creator)).ReadToArray();
    }

    public static async Task<Stream> AsStreamAsync(this ILowLevelDocumentCreator creator)
    {
        var doc = creator.CreateDocument();
        var output = new MultiBufferStream();
        await doc.WriteToAsync(output);
        return output.CreateReader();
    }
    public static async Task<IFile> AsFileAsync(this ILowLevelDocumentCreator creator) =>
        new MemoryFile("S:\\d.pdf", await creator.AsBytesAsync());
    public static async Task<String> AsStringAsync(this ILowLevelDocumentCreator creator) =>
        ExtendedAsciiEncoding.ExtendedAsciiString(await creator.AsBytesAsync());
}