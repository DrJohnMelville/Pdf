using System.IO;
using FluentAssertions;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;
using Melville.Pdf.ReferenceDocumentGenerator.Targets;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Melville.Pdf.DataModelTests.ParseMappings;

public class ParseMappingTest
{
    [Fact]
    public void ParseSingle()
    {
        var stream = new MemoryStream();
        var monitor = stream.MonitorParsing();
        stream.LogParsePosition("This is a Label");
        stream.LogParsePosition("Label 0");
        monitor.Entries.Should().BeEquivalentTo([
            new ParseMapEntry("This is a Label", 0, 0),
            new ParseMapEntry("Label 0", 0, 0),
            ]);
    }
    [Fact]
    public void ExplicitOffset()
    {
        var stream = new MemoryStream();
        var monitor = stream.MonitorParsing();
        stream.LogParsePosition("This is a Label", 5);
        stream.LogParsePosition("Label 0", 10);
        monitor.Entries.Should().BeEquivalentTo([
            new ParseMapEntry("This is a Label", 0, 5),
            new ParseMapEntry("Label 0", 5, 10),
            ]);
    }
    [Fact]
    public void UseStreamPosition()
    {
        var stream = new MemoryStream();
        var monitor = stream.MonitorParsing();
        stream.Write("12345"u8);
        stream.LogParsePosition("This is a Label");
        stream.Write("12345"u8);
        stream.LogParsePosition("Label 0", 12);
        monitor.Entries.Should().BeEquivalentTo([
            new ParseMapEntry("This is a Label", 0, 5),
            new ParseMapEntry("Label 0", 5, 22),
            ]);
    }

    [Fact]
    public void PipeReaderSource()
    {
        var msf = MultiplexSourceFactory.Create("01234567890"u8);
        var reader = msf.ReadPipeFrom(3);
        var monitor = reader.MonitorParsing();
        reader.LogParsePosition("This is a Label");
        var result = reader.Read();
        reader.AdvanceTo(result.Buffer.GetPosition(3));
        reader.LogParsePosition("Label 0");
        monitor.Entries.Should().BeEquivalentTo([
            new ParseMapEntry("This is a Label", 0, 0),
            new ParseMapEntry("Label 0", 0, 3),
            ]);
    }
}