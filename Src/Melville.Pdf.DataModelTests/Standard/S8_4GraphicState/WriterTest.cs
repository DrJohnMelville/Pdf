using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public abstract class WriterTest
{
    private readonly MultiBufferStream destStream = new ();
    private readonly PipeWriter destPipe;
    protected readonly ContentStreamWriter sut;

    protected WriterTest()
    {
        destPipe = PipeWriter.Create(destStream);
        sut = new ContentStreamWriter(destPipe);
    }
    protected async Task<string> WrittenTextAsync()
    {
        await destPipe.FlushAsync();
        return await ((IMultiplexSource)destStream).ReadFrom(0).ReadAsStringAsync();
    }
}