/// <summary>**************************************************************************
/// 
/// $Id: LookUpTable32Gamma.java,v 1.1 2002/07/25 14:56:47 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using Tags_ICCCurveType = CoreJ2K.Icc.Tags.ICCCurveType;

namespace CoreJ2K.Icc.Lut
{
	
	/// <summary> A Gamma based 32 bit lut.
	/// 
	/// </summary>
	/// <seealso cref="j2k.icc.tags.ICCCurveType" />
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	
	public class LookUpTable32Gamma:LookUpTable32
	{
		
		
		/* Construct the lut    
		*   @param curve data 
		*   @param dwNumInput size of lut  
		*   @param dwMaxOutput max value of lut   
		*/
		public LookUpTable32Gamma(Tags_ICCCurveType curve, int dwNumInput, int dwMaxOutput):base(curve, dwNumInput, dwMaxOutput)
		{
			var dfE = Tags_ICCCurveType.CurveGammaToDouble(curve.entry(0)); // Gamma exponent for inverse transformation
			for (var i = 0; i < dwNumInput; i++)
			{
				//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
				lut[i] = (int) Math.Floor(Math.Pow((double) i / (dwNumInput - 1), dfE) * dwMaxOutput + 0.5);
			}
		}
		
		/* end class LookUpTable32Gamma */
	}
}