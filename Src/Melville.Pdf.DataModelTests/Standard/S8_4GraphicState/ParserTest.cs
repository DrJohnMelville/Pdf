using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Moq;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public abstract class ParserTest
{
    protected Mock<ConcreteCSO> Target { get; } = new();
    private readonly ContentStreamParser sut;

    protected ParserTest()
    {
        sut = new ContentStreamParser(Target.Object);
    }

    protected ValueTask ParseString(string s) => sut.Parse(PipeReaderFromString(s));

    private static PipeReader PipeReaderFromString(string s) =>
        PipeReader.Create(new MemoryStream(s.AsExtendedAsciiBytes()));

    protected async Task TestInput(
        string input, Expression<Action<ConcreteCSO>> action)
    {
        await ParseString(input);
        Target.Verify(action);
        Target.VerifyNoOtherCalls();
    }
    
}