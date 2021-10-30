using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateWriters
{
    private readonly MultiBufferStream destStream = new ();
    private readonly PipeWriter destPipe;
    private readonly ContentStreamWriter sut;

    public GraphicStateWriters()
    {
        destPipe = PipeWriter.Create(destStream);
        sut = new ContentStreamWriter(destPipe);
    }
    private async Task<string> WrittenText()
    {
        await destPipe.FlushAsync();
        return await destStream.CreateReader().ReadAsStringAsync();
    }

    [Fact]
    public async Task PushGraphicState()
    {
        sut.SaveGraphicsState();
        Assert.Equal("q\n", await WrittenText());
    }

    [Fact]
    public async Task RestoreGraphicsState()
    {
        sut.RestoreGraphicsState();
        Assert.Equal("Q\n", await WrittenText());
        
    }
    [Fact]
    public async Task CompositePush()
    {
        sut.SaveGraphicsState();
        sut.RestoreGraphicsState();
        Assert.Equal("q\nQ\n", await WrittenText());
    }
    // [Fact]
    // public async Task ModifyTransformMatrix()
    // {
    //     sut.ModifyTransformMatrix(1,2,3,4,5,6);
    //     Assert.Equal("1 2 3 4 5 6 cm\n", await WrittenText());
    // }

}