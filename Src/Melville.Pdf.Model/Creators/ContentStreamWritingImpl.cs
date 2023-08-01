using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// This class implement convenience methods to write content streams.
/// </summary>
public static class ContentStreamWritingImpl
{
    /// <summary>
    /// Write a content stream to a page creator by calling methods on a
    /// ContentStreamWriter
    /// </summary>
    /// <param name="pc">The PageCreator to add a content stream to.</param>
    /// <param name="creator">An action that will call methods of the provided
    ///  ContentStreamWriter to define the desired content stream.</param>
    /// <returns>A ValueTask to signal completion of the operation.</returns>
    public static ValueTask AddToContentStreamAsync(
        this PageCreator pc, Action<ContentStreamWriter> creator) =>
        pc.AddToContentStreamAsync(i =>
        {
            creator(i);
            return ValueTask.CompletedTask;
        });

    /// <summary>
    /// Write a content stream to a page creator by calling methods on a
    /// ContentStreamWriter
    /// </summary>
    /// <param name="pc">The PageCreator to add a content stream to.</param>
    /// <param name="creator">An action that will call methods of the provided
    ///  ContentStreamWriter to define the desired content stream.</param>
    /// <returns>A ValueTask to signal completion of the operation.</returns>
    public static ValueTask AddToContentStreamAsync(
        this PageCreator pc, Func<ContentStreamWriter, ValueTask> creator) =>
        pc.AddToContentStreamAsync(new ValueDictionaryBuilder().WithFilter(FilterName.FlateDecode), creator);

    /// <summary>
    /// Write a content stream to a page creator by calling methods on a
    /// ContentStreamWriter
    /// </summary>
    /// <param name="pc">The PageCreator to add a content stream to.</param>
    /// <param name="dict">The DictionaryBuilder that will be used to create the content stream</param>
    /// <param name="creator">An action that will call methods of the provided
    ///  ContentStreamWriter to define the desired content stream.</param>
    /// <returns>A ValueTask to signal completion of the operation.</returns>
    public static async ValueTask AddToContentStreamAsync(
        this ContentStreamCreator pc, ValueDictionaryBuilder dict, Func<ContentStreamWriter, ValueTask> creator)
    {
        var streamData = new MultiBufferStream();
        var pipe = PipeWriter.Create(streamData);
        await creator(new ContentStreamWriter(pipe)).CA();
        await pipe.FlushAsync().CA();
        pc.AddToContentStream(dict, streamData);
    }
}