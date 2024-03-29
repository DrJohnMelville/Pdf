﻿using System.Buffers;
using Melville.Icc.ColorTransforms;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents a multiprocess transdorm as a matrix multiplication and translation 
/// </summary>
public class MultiProcessMatrix : IColorTransform
{
    /// <inheritdoc />
    public int Inputs { get; }

    /// <inheritdoc />
    public int Outputs { get; }

    /// <summary>
    /// Values that form the transform matrix
    /// </summary>
    public IReadOnlyList<float> Values;

    internal MultiProcessMatrix(int inputs, int outputs, params float[] values)
    {
        Inputs = inputs;
        Outputs = outputs;
        Values = values;
    }
    
    internal static MultiProcessMatrix Parse(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        var inputs = reader.ReadBigEndianUint16();
        var outputs = reader.ReadBigEndianUint16();
        return new MultiProcessMatrix(inputs, outputs,
         reader.ReadIEEE754FloatArray((inputs * outputs) + outputs));
    }

    /// <inheritdoc />
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Span<float> intermediate = stackalloc float[Outputs];
        MatrixMultiplication(input, intermediate);
        AddVectorOutput(output, intermediate);
    }

    private void MatrixMultiplication(in ReadOnlySpan<float> input, in Span<float> intermediate)
    {
        int pointer = 0;
        for (int i = 0; i < Outputs; i++)
        {
            for (int j = 0; j < Inputs; j++)
            {
                intermediate[i] += input[j] * Values[pointer++];
            }
        }
    }

    private void AddVectorOutput(in Span<float> output, in Span<float> intermediate)
    {
        var translateBase = Inputs * Outputs;
        for (int i = 0; i < Outputs; i++)
        {
            output[i] = intermediate[i] + Values[translateBase + i];
        }
    }
}