using System.Buffers;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// This type represents a 3z3 matrix multiplication followed by translation along each axis
/// </summary>
public readonly struct AugmentedMatrix3x3
{
    /// <summary>
    /// Describes an IdentityMatrix, which does not change an
    /// </summary>
    public static readonly AugmentedMatrix3x3 Identity = new(Matrix3x3.Identity, 0,0,0);
    /// <summary>
    /// The 3x3 Matrix used in the transform
    /// </summary>
    public Matrix3x3 Kernel { get; }
    /// <summary>
    /// Translation value on the X axis
    /// </summary>
    public float TranslateX { get; }
    /// <summary>
    /// Translation value for the Y axis 
    /// </summary>
    public float TranslateY { get; }
    /// <summary>
    /// Translation value for the Z axis
    /// </summary>
    public float TranslateZ { get; }

    /// <summary>
    /// Create a new augmented matrix
    /// </summary>
    /// <param name="kernel">A kernel matrix reprepsenting scale, rotate, and skew</param>
    /// <param name="translateX">X translation parameter</param>
    /// <param name="translateY">Y translation parameter</param>
    /// <param name="translateZ">Z translation parameter</param>
    public AugmentedMatrix3x3(Matrix3x3 kernel, float translateX, float translateY, float translateZ)
    {
        Kernel = kernel;
        TranslateX = translateX;
        TranslateY = translateY;
        TranslateZ = translateZ;
    }
    
    /// <summary>
    /// Read an augmentedMatrix from a sequence reader.
    /// </summary>
    /// <param name="reader"></param>
    public AugmentedMatrix3x3(ref SequenceReader<byte> reader):
        this (new Matrix3x3(ref reader),
            reader.Reads15Fixed16(), reader.Reads15Fixed16(), reader.Reads15Fixed16()){}

    /// <summary>
    /// This performs the transform described by the class
    /// </summary>
    /// <param name="input">A span of 3 floats to perform the transform upon</param>
    /// <param name="output">A span of 3 floats to receive the output, may be equal to input</param>
    public void PostMultiplyBy(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Kernel.PostMultiplyBy(input, output);
        output[0] += TranslateX;
        output[1] += TranslateY;
        output[2] += TranslateZ;
    }
}