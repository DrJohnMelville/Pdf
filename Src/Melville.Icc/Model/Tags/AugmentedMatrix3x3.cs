using System.Buffers;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

public readonly struct AugmentedMatrix3x3
{
    public static readonly AugmentedMatrix3x3 Identity = new(Matrix3x3.Identity, 0,0,0);
    public Matrix3x3 Kernel { get; }
    public float TranslateX { get; }
    public float TranslateY { get; }
    public float TranslateZ { get; }

    public AugmentedMatrix3x3(Matrix3x3 kernel, float translateX, float translateY, float translateZ)
    {
        Kernel = kernel;
        TranslateX = translateX;
        TranslateY = translateY;
        TranslateZ = translateZ;
    }
    
    public AugmentedMatrix3x3(ref SequenceReader<byte> reader):
        this (new Matrix3x3(ref reader),
            reader.Reads15Fixed16(), reader.Reads15Fixed16(), reader.Reads15Fixed16()){}

    public void PostMultiplyBy(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Kernel.PostMultiplyBy(input, output);
        output[0] += TranslateX;
        output[1] += TranslateY;
        output[2] += TranslateZ;
    }
}