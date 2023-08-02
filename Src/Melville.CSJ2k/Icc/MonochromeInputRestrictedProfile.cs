/// <summary>**************************************************************************
/// 
/// $Id: MonochromeInputRestrictedProfile.java,v 1.1 2002/07/25 14:56:56 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>

using ICCCurveType = Melville.CSJ2K.Icc.Tags.ICCCurveType;
namespace Melville.CSJ2K.Icc
{
	
	/// <summary> This class is a 1 component RestrictedICCProfile
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A Kern
	/// </author>
	internal class MonochromeInputRestrictedProfile:RestrictedICCProfile
	{
		/// <summary> Get the type of RestrictedICCProfile for this object</summary>
		/// <returns> kMonochromeInput
		/// </returns>
		override public int Type
		{
			get
			{
				return kMonochromeInput;
			}
			
		}
		
		/// <summary> Factory method which returns a 1 component RestrictedICCProfile</summary>
		/// <param name="c">Gray TRC curve
		/// </param>
		/// <returns> the RestrictedICCProfile
		/// </returns>
		public static new RestrictedICCProfile createInstance(ICCCurveType c)
		{
			return new MonochromeInputRestrictedProfile(c);
		}
		
		/// <summary> Construct a 1 component RestrictedICCProfile</summary>
		/// <param name="c">Gray TRC curve
		/// </param>
		private MonochromeInputRestrictedProfile(ICCCurveType c):base(c)
		{
		}
		/* end class MonochromeInputRestrictedProfile */
	}
}