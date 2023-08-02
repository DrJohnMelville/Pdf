/* 
* CVS identifier:
* 
* $Id: SynWTFilterSpec.java,v 1.15 2001/08/02 10:01:30 grosbois Exp $
* 
* Class:                   SynWTFilterSpec
* 
* Description:             Synthesis filters specification
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

namespace Melville.CSJ2K.j2k.wavelet.synthesis
{
	
	/// <summary> This class extends ModuleSpec class for synthesis filters specification
	/// holding purpose.
	/// 
	/// </summary>
	/// <seealso cref="ModuleSpec">
	/// 
	/// </seealso>
	internal class SynWTFilterSpec:ModuleSpec
	{
		
		/// <summary> Constructs a new 'SynWTFilterSpec' for the specified number of
		/// components and tiles.
		/// 
		/// </summary>
		/// <param name="nt">The number of tiles
		/// 
		/// </param>
		/// <param name="nc">The number of components
		/// 
		/// </param>
		/// <param name="type">the type of the specification module i.e. tile specific,
		/// component specific or both.
		/// 
		/// </param>
		public SynWTFilterSpec(int nt, int nc, byte type):base(nt, nc, type)
		{
		}
		
		/// <summary> Returns the data type used by the filters in this object, as defined in
		/// the 'DataBlk' interface for specified tile-component.
		/// 
		/// </summary>
		/// <param name="t">Tile index
		/// 
		/// </param>
		/// <param name="c">Component index
		/// 
		/// </param>
		/// <returns> The data type of the filters in this object
		/// 
		/// </returns>
		/// <seealso cref="jj2000.j2k.image.DataBlk">
		/// 
		/// </seealso>
		public virtual int getWTDataType(int t, int c)
		{
			SynWTFilter[][] an = (SynWTFilter[][]) getSpec(t, c);
			return an[0][0].DataType;
		}
		
		/// <summary> Returns the horizontal analysis filters to be used in component 'n' and
		/// tile 't'.
		/// 
		/// <p>The horizontal analysis filters are returned in an array of
		/// SynWTFilter. Each element contains the horizontal filter for each
		/// resolution level starting with resolution level 1 (i.e. the analysis
		/// filter to go from resolution level 1 to resolution level 0). If there
		/// are less elements than the maximum resolution level, then the last
		/// element is assumed to be repeated.</p>
		/// 
		/// </summary>
		/// <param name="t">The tile index, in raster scan order
		/// 
		/// </param>
		/// <param name="c">The component index.
		/// 
		/// </param>
		/// <returns> The array of horizontal analysis filters for component 'n' and
		/// tile 't'.
		/// 
		/// </returns>
		public virtual SynWTFilter[] getHFilters(int t, int c)
		{
			SynWTFilter[][] an = (SynWTFilter[][]) getSpec(t, c);
			return an[0];
		}
		
		/// <summary> Returns the vertical analysis filters to be used in component 'n' and
		/// tile 't'.
		/// 
		/// <p>The vertical analysis filters are returned in an array of
		/// SynWTFilter. Each element contains the vertical filter for each
		/// resolution level starting with resolution level 1 (i.e. the analysis
		/// filter to go from resolution level 1 to resolution level 0). If there
		/// are less elements than the maximum resolution level, then the last
		/// element is assumed to be repeated.</p>
		/// 
		/// </summary>
		/// <param name="t">The tile index, in raster scan order
		/// 
		/// </param>
		/// <param name="c">The component index.
		/// 
		/// </param>
		/// <returns> The array of horizontal analysis filters for component 'n' and
		/// tile 't'.
		/// 
		/// </returns>
		public virtual SynWTFilter[] getVFilters(int t, int c)
		{
			SynWTFilter[][] an = (SynWTFilter[][]) getSpec(t, c);
			return an[1];
		}
		
		/// <summary>Debugging method </summary>
		public override System.String ToString()
		{
			System.String str = "";
			SynWTFilter[][] an;
			
			str += ("nTiles=" + nTiles + "\nnComp=" + nComp + "\n\n");
			
			for (int t = 0; t < nTiles; t++)
			{
				for (int c = 0; c < nComp; c++)
				{
					an = (SynWTFilter[][]) getSpec(t, c);
					
					str += ("(t:" + t + ",c:" + c + ")\n");
					
					// Horizontal filters
					str += "\tH:";
					for (int i = 0; i < an[0].Length; i++)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
						str += (" " + an[0][i]);
					}
					// Horizontal filters
					str += "\n\tV:";
					for (int i = 0; i < an[1].Length; i++)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
						str += (" " + an[1][i]);
					}
					str += "\n";
				}
			}
			
			return str;
		}
		
		/// <summary> Check the reversibility of filters contained is the given
		/// tile-component.
		/// 
		/// </summary>
		/// <param name="t">The index of the tile
		/// 
		/// </param>
		/// <param name="c">The index of the component
		/// 
		/// </param>
		public virtual bool isReversible(int t, int c)
		{
			// Note: no need to buffer the result since this method is normally
			// called once per tile-component.
			SynWTFilter[] hfilter = getHFilters(t, c), vfilter = getVFilters(t, c);
			
			// As soon as a filter is not reversible, false can be returned
			for (int i = hfilter.Length - 1; i >= 0; i--)
				if (!hfilter[i].Reversible || !vfilter[i].Reversible)
					return false;
			return true;
		}
	}
}