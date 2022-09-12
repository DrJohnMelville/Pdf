using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.Model.Creators;

public static class ContentStreamWritingImpl
{
    public static ValueTask AddToContentStreamAsync(
        this PageCreator pc, Action<ContentStreamWriter> creator) =>
        pc.AddToContentStreamAsync(i =>
        {
            creator(i);
            return ValueTask.CompletedTask;
        });
    public static ValueTask AddToContentStreamAsync(
        this PageCreator pc, Func<ContentStreamWriter, ValueTask> creator) =>
        pc.AddToContentStreamAsync(new DictionaryBuilder().WithFilter(FilterName.FlateDecode), creator);
    public static async ValueTask AddToContentStreamAsync(
        this ContentStreamCreator pc, DictionaryBuilder dict, Func<ContentStreamWriter, ValueTask> creator)
    {
        var streamData = new MultiBufferStream();
        var pipe = PipeWriter.Create(streamData);
        await creator(new ContentStreamWriter(pipe)).CA();
        await pipe.FlushAsync().CA();
        pc.AddToContentStream(dict, streamData);
    }
}