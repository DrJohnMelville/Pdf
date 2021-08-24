using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Primitives;
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
                    public long Position => lastSeek + reader.Position;
                    public long StreamLength => Owner.StreamLength;
                    public IPdfObjectParser RootObjectParser => Owner.RootObjectParser;
                    public IIndirectObjectResolver IndirectResolver => Owner.IndirectResolver;
        
                    public ParsingFileOwner Owner { get; }
                    private CountingPipeReader reader;
        
                    public ParsingReader(ParsingFileOwner owner, PipeReader reader, long lastSeek)
                    {
                        this.lastSeek = lastSeek;
                        this.reader = new CountingPipeReader(reader);
                        Owner = owner;
                    }
        
                    public void Dispose()
                    {
                        Owner.ReturnReader(this);
                        reader = null!; // we want the exception if we try to touch a disposed reader
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
                        reader.MarkSequenceAsExamined();
                        return true;
                    }

                    public ValueTask<ReadResult> ReadAsync(CancellationToken token = default) => reader.ReadAsync(token);
                    public void AdvanceTo(SequencePosition consumed) =>
                        AdvanceTo(consumed, consumed);
        
                    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
                    {
                        reader.AdvanceTo(consumed, examined);
                    }
                }
    }
}