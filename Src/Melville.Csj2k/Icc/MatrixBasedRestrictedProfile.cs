/// <summary>**************************************************************************
/// 
/// $Id: MatrixBasedRestrictedProfile.java,v 1.1 2002/07/25 14:56:56 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using ICCCurveType = Melville.CSJ2K.Icc.Tags.ICCCurveType;
using ICCXYZType = Melville.CSJ2K.Icc.Tags.ICCXYZType;
namespace Melville.CSJ2K.Icc
{
	
	/// <summary> This class is a 3 component RestrictedICCProfile
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A Kern
	/// </author>
	public class MatrixBasedRestrictedProfile:RestrictedICCProfile
	{
		/// <summary> Get the type of RestrictedICCProfile for this object</summary>
		/// <returns> kThreeCompInput
		/// </returns>
		override public int Type
		{
			get
			{
				return kThreeCompInput;
			}
			
		}

		/// <summary> Construct a 3 component RestrictedICCProfile</summary>
		/// <param name="rcurve">Red TRC curve
		/// </param>
		/// <param name="gcurve">Green TRC curve
		/// </param>
		/// <param name="bcurve">Blue TRC curve
		/// </param>
		/// <param name="rcolorant">Red colorant
		/// </param>
		/// <param name="gcolorant">Green colorant
		/// </param>
		/// <param name="bcolorant">Blue colorant
		/// </param>
		public MatrixBasedRestrictedProfile(ICCCurveType rcurve, ICCCurveType gcurve, ICCCurveType bcurve, ICCXYZType rcolorant, ICCXYZType gcolorant, ICCXYZType bcolorant):base(rcurve, gcurve, bcurve, rcolorant, gcolorant, bcolorant)
		{
		}
		
		/// <returns> String representation of a MatrixBasedRestrictedProfile
		/// </returns>
		public override System.String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[Matrix-Based Input Restricted ICC profile").Append(eol);
			
			rep.Append("trc[RED]:").Append(eol).Append(trc[ICCProfile.RED]).Append(eol);
			rep.Append("trc[RED]:").Append(eol).Append(trc[ICCProfile.GREEN]).Append(eol);
			rep.Append("trc[RED]:").Append(eol).Append(trc[ICCProfile.BLUE]).Append(eol);
			
			rep.Append("Red colorant:  ").Append(colorant[ICCProfile.RED]).Append(eol);
			rep.Append("Red colorant:  ").Append(colorant[ICCProfile.GREEN]).Append(eol);
			rep.Append("Red colorant:  ").Append(colorant[ICCProfile.BLUE]).Append(eol);
			
			return rep.Append("]").ToString();
		}
		
		/* end class MatrixBasedRestrictedProfile */
	}
}