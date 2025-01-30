using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.ComparingReader.Viewers.ContentStreamOperationsFilters;

internal class NopSpacedStringBuilder : ISpacedStringBuilder
{
    /// <inheritdoc />
    public ValueTask DisposeAsync() => default;

    /// <inheritdoc />
    public ValueTask SpacedStringComponentAsync(double value) => default;

    /// <inheritdoc />
    public ValueTask SpacedStringComponentAsync(ReadOnlyMemory<byte> value) => default;
}