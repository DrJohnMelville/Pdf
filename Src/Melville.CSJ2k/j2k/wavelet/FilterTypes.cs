/*
* CVS identifier:
*
* $Id: FilterTypes.java,v 1.12 2001/05/08 16:14:28 grosbois Exp $
*
* Class:                   FilterTypes
*
* Description:             Defines the interface for Filter types
*
*
*
* COPYRIGHT:
* 
* This software module was originally developed by Rapha�l Grosbois and
* Diego Santa Cruz (Swiss Federal Institute of Technology-EPFL); Joel
* Askel�f (Ericsson Radio Systems AB); and Bertrand Berthelot, David
* Bouchard, F�lix Henry, Gerard Mozelle and Patrice Onno (Canon Research
* Centre France S.A) in the course of development of the JPEG2000
* standard as specified by ISO/IEC 15444 (JPEG 2000 Standard). This
* software module is an implementation of a part of the JPEG 2000
* Standard. Swiss Federal Institute of Technology-EPFL, Ericsson Radio
* Systems AB and Canon Research Centre France S.A (collectively JJ2000
* Partners) agree not to assert against ISO/IEC and users of the JPEG
* 2000 Standard (Users) any of their rights under the copyright, not
* including other intellectual property rights, for this software module
* with respect to the usage by ISO/IEC and Users of this software module
* or modifications thereof for use in hardware or software products
* claiming conformance to the JPEG 2000 Standard. Those intending to use
* this software module in hardware or software products are advised that
* their use may infringe existing patents. The original developers of
* this software module, JJ2000 Partners and ISO/IEC assume no liability
* for use of this software module or modifications thereof. No license
* or right to this software module is granted for non JPEG 2000 Standard
* conforming products. JJ2000 Partners have full right to use this
* software module for his/her own purpose, assign or donate this
* software module to any third party and to inhibit third parties from
* using this software module for non JPEG 2000 Standard conforming
* products. This copyright notice must be included in all copies or
* derivative works of this software module.
* 
* Copyright (c) 1999/2000 JJ2000 Partners.
* */

namespace CoreJ2K.j2k.wavelet
{
	
	/// <summary> This interface defines the identifiers for the different types of filters
	/// that are supported.
	/// 
	/// The identifier values are the same as those used in the codestream
	/// syntax, for the filters that are defined in the standard.
	/// 
	/// </summary>
	public struct FilterTypes_Fields{
		/// <summary>W7x9 filter: 0x00 </summary>
		public const int W9X7 = 0;
		/// <summary>W5x3 filter: 0x01 </summary>
		public const int W5X3 = 1;
		/// <summary>User-defined filter: -1 </summary>
		public const int CUSTOM = - 1;
	}
	public interface FilterTypes
	{
		//UPGRADE_NOTE: Members of interface 'FilterTypes' were extracted into structure 'FilterTypes_Fields'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1045'"
		
	}
}