using Melville.Parsing.StreamFilters;

namespace Melville.CCITT;

/// <summary>
/// This class will create a CCITT encoder or decoder for a given set of CCITT parameters
/// </summary>
public class CcittCodecFactory
{
    /// <summary>
    /// Create a CCITTT encoder for a given set of parameters (which may come from a PDF file)
    /// </summary>
    /// <param name="args">The description of the encoder to create</param>
    /// <returns></returns>
    public static IStreamFilterDefinition SelectEncoder(CcittParameters args) => args.K switch
    {
        < 0 => new CcittType4Encoder(args),
        0 => new CcittType31dEncoder(args),
        > 0 => new CcittType3SwitchingEncoder(args)
    };

    /// <summary>
    /// Create a CCITTT decoder for a given set of parameters (which may come from a PDF file)
    /// </summary>
    /// <param name="args">The description of the decoder to create</param>
    /// <returns></returns>
    public static IStreamFilterDefinition SelectDecoder(CcittParameters args) => args.K switch
    {
        < 0 => new CcittType4Decoder(args, new TwoDimensionalLineCodeDictionary()),
        0 => new CcittType4Decoder(args, new Type3K0LineCodeDictionary()),
        > 0 => new CcittType4Decoder(args, new Type3SwitchingLineCodeDictionary())
    };

}

