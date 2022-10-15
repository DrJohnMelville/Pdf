using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel;

public readonly struct PdfLowLevelReader
{
    private readonly IPasswordSource passwordSource;

    public PdfLowLevelReader(IPasswordSource? passwordSource = null)
    {
        this.passwordSource = passwordSource ?? NullPasswordSource.Instance;
    }


    public ValueTask<PdfLoadedLowLevelDocument> ReadFromAsync(object argument) => argument switch
    {
        string s => ReadFromFile(s),
        byte[] s => ReadFromAsync(s),
        Stream s => ReadFromAsync(s),
        _ => throw new ArgumentException("Must be string, byte array, or stream.", nameof(argument))
    };

    public ValueTask<PdfLoadedLowLevelDocument> ReadFromFile(string path) =>
        ReadFromAsync(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));

    public ValueTask<PdfLoadedLowLevelDocument> ReadFromAsync(byte[] input) =>
        ReadFromAsync(new MemoryStream(input));

    public ValueTask<PdfLoadedLowLevelDocument> ReadFromAsync(Stream input) =>
        RandomAccessFileParser.Parse(
            new ParsingFileOwner(input, passwordSource, new IndirectObjectResolver()));
}