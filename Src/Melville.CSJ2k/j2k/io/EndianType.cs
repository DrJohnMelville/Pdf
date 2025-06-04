/*
* CVS Identifier:
*
* $Id: EndianType.java,v 1.10 2000/09/05 09:24:36 grosbois Exp $
*
* Interface:           EndianType
*
* Description:         Defines the two types of endianess (i.e. byte
*                      ordering).
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
* 
* 
* 
*/

namespace CoreJ2K.j2k.io
{
	
	/// <summary> This interface defines constants for the two types of byte
	/// ordering: little- and big-endian.
	/// 
	/// Little-endian is least significant byte first.
	/// 
	/// Big-endian is most significant byte first.
	/// 
	/// This interface defines the constants only. In order to use the
	/// constants in any other class you can either use the fully qualified 
	/// name (e.g., <tt>EndianType.LITTLE_ENDIAN</tt>) or declare this
	/// interface in the implements clause of the class and then access the 
	/// identifier directly.
	/// 
	/// </summary>
	public struct EndianType_Fields{
		/// <summary>Identifier for big-endian byte ordering (i.e. most significant 
		/// byte first) 
		/// </summary>
		public const int BIG_ENDIAN = 0;
		/// <summary>Identifier for little-endian byte ordering (i.e. least
		/// significant byte first) 
		/// </summary>
		public const int LITTLE_ENDIAN = 1;
	}
	public interface EndianType
	{
		//UPGRADE_NOTE: Members of interface 'EndianType' were extracted into structure 'EndianType_Fields'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1045'"
		
	}
}