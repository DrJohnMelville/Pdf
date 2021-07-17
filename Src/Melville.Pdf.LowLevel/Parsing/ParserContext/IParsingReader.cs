using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public interface IParsingReader : IDisposable
    {
        long Position { get; }
        long StreamLength { get; }
        IPdfObjectParser RootObjectParser { get; }
        IIndirectObjectResolver IndirectResolver { get; }
        ParsingFileOwner Owner { get; }
        ValueTask<ReadResult> ReadAsync(CancellationToken token = default);

        /// <summary>
        /// This method enables a very specific pattern that is common with parsing from the PipeReader.
        ///
        /// the pattern is do{}while(source.ShouldContinue(Method(await source.ReadAsync)));
        ///
        /// Method returns a pair (bool, SequencePosition).  Method can use out parameters for "real"
        /// return values.
        ///
        /// This pattern repeately reads the stream until method successfully parses, then it advances
        /// the reader to the given sequence position.
        /// </summary>
        bool ShouldContinue((bool Success, SequencePosition Position) result);

        void AdvanceTo(SequencePosition consumed);
        void AdvanceTo(SequencePosition consumed, SequencePosition examined);
    }

    public partial class ParsingFileOwner
    {
                private class ParsingReader : IParsingReader
                {
                    private long lastSeek;
                    private long lastAdvanceOffset;
                    public long Position => lastSeek + lastAdvanceOffset;
                    public long StreamLength => Owner.StreamLength;
                    public IPdfObjectParser RootObjectParser => Owner.RootObjectParser;
                    public IIndirectObjectResolver IndirectResolver => Owner.IndirectResolver;
        
                    public ParsingFileOwner Owner { get; }
                    private PipeReader reader;
                    private ReadOnlySequence<byte> storedSequence;
        
                    public ParsingReader(ParsingFileOwner owner, PipeReader reader, long lastSeek)
                    {
                        this.lastSeek = lastSeek;
                        this.reader = reader;
                        Owner = owner;
                    }
        
                    public void Dispose()
                    {
                        Owner.ReturnReader(this);
                        reader = InvalidReader.Instance;
                    }
                    
                    public ValueTask<ReadResult> ReadAsync(CancellationToken token = default)
                    {
                        var valueTask = reader.ReadAsync(token);
                        if (!valueTask.IsCompleted)
                            return new ValueTask<ReadResult>(WaitForRead(valueTask));
        
                        var res = valueTask.Result;
                        StorePosition(res.Buffer);
                        return new ValueTask<ReadResult>(res);
                    }
        
                    /// <summary>
                    /// This method enables a very specific pattern that is common with parsing from the PipeReader.
                    ///
                    /// the pattern is do{}while(source.ShouldContinue(Method(await source.ReadAsync)));
                    ///
                    /// Method returns a pair (bool, SequencePosition).  Method can use out parameters for "real"
                    /// return values.
                    ///
                    /// This pattern repeately reads the stream until method successfully parses, then it advances
                    /// the reader to the given sequence position.
                    /// </summary>
                    public bool ShouldContinue((bool Success, SequencePosition Position) result)
                    {
                        if (result.Success)
                        {
                            AdvanceTo(result.Position);
                            return false;
                        }
        
                        NeedMoreInputToCompleteParsing();
                        return true;
                    }
        
                    private void NeedMoreInputToCompleteParsing() => AdvanceTo(storedSequence.Start, storedSequence.End);
        
        
                    private async Task<ReadResult> WaitForRead(ValueTask<ReadResult> valueTask)
                    {
                        var ret = await valueTask;
                        StorePosition(ret.Buffer);
                        return ret;
                    }
        
                    private void StorePosition(ReadOnlySequence<byte> resBuffer)
                    {
                        storedSequence = resBuffer;
                    }
        
                    public void AdvanceTo(SequencePosition consumed) =>
                        AdvanceTo(consumed, consumed);
        
                    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
                    {
                        lastAdvanceOffset += BytesAdvancedBy(consumed);
                        reader.AdvanceTo(consumed, examined);
                        storedSequence = default;
                    }
        
                    private long BytesAdvancedBy(SequencePosition consumed) =>
                        storedSequence.Slice(0, consumed).Length;
                }
    }
}