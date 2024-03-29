﻿using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal partial class CrossReferenceTableParser
{
    private int firstUnspecifiedLine = 0;
    private int line = 1;

    [FromConstructor] private readonly IByteSource byteSource;
    [FromConstructor] private readonly IIndirectObjectRegistry registry;

    public async Task ParseAsync()
    {
        while (!ParseNextLine(await byteSource.ReadAsync().CA()))
        {
        }       
    }

    private bool ParseNextLine(ReadResult data)
    {
        var reader = new SequenceReader<byte>(data.Buffer);
        var ret = ParseNextLine(ref reader);
        byteSource.AdvanceTo(reader.Position, data.Buffer.End);
        return ret;
    }

    private bool ParseNextLine(ref SequenceReader<byte> reader)
    {
        while (true)
        {
            if (NeedAHeaderLine())
            {
                if (IsDoneReadingTable(ref reader)) return true;
                if (!HandleHeaderLine(ref reader)) return false;
            }else if (!HandelXefLine(ref reader)) return false;
        }
    }
    private bool NeedAHeaderLine() => line >= firstUnspecifiedLine;

    private bool IsDoneReadingTable(ref SequenceReader<byte> reader) => 
        reader.TryPeek(out var peek) && !WholeNumberParser.IsDigit(peek);

    private bool HandleHeaderLine(ref SequenceReader<byte> reader)
    {
        var copy = reader;
        if (!ParseHeaderLine(ref reader, out var first, out var second))
        {
            reader = copy;
            return false;
        }

        line = first;
        firstUnspecifiedLine = first + second;
        return true;
    }

    private static bool ParseHeaderLine(ref SequenceReader<byte> reader, out int first, out int second)
    {
        second = 0; 
        return WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out first, out _) &&
               reader.SkipToNextToken() &&
               WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out second, out _) &&
               reader.SkipToNextToken();
    }

    private bool HandelXefLine(ref SequenceReader<byte> reader)
    {
        if (reader.Remaining < 20) return false;
        var (first, second, operation) = ParseXrefLine(reader);
        reader.Advance(20);
        HandleXrefLine(first, second, operation);
        return true;
    }

    private (long first, int second, byte operation) ParseXrefLine(SequenceReader<byte> reader)
    {
        var first = GetInt(reader.UnreadSequence.Slice(0, 10));
        var second = GetInt(reader.UnreadSequence.Slice(11, 5));
        reader.TryPeek(17, out var operation);
        return (first, second, operation);
    }

    private int GetInt(ReadOnlySequence<byte> slice)
    {
        var reader = new SequenceReader<byte>(slice);
        var ret = 0;
        while (reader.TryRead(out byte digit))
        {
            ret *= 10;
            ret += digit - '0';
        }
        return ret;
    }

    private void HandleXrefLine(long first, int second, byte operation)
    {
        switch (operation)
        {
            case (byte)'n':
                registry.RegisterIndirectBlock(line, second, first);
                break;
            case (byte)'f':
                registry.RegisterDeletedBlock(line, (int)first, second);
                break;
            default: throw new PdfParseException("Invalid Xref Table Operation");
        }

        line++;
    }
}