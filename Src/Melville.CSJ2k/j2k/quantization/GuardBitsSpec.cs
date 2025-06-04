/* 
* CVS identifier:
* 
* $Id: GuardBitsSpec.java,v 1.13 2000/09/19 14:11:01 grosbois Exp $
* 
* Class:                   GuardBitsSpec
* 
* Description:             Guard bits specifications
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
using System;
using CoreJ2K.j2k.util;

namespace CoreJ2K.j2k.quantization
{
	
	/// <summary> This class extends ModuleSpec class in order to hold specifications about
	/// number of guard bits in each tile-component.
	/// 
	/// </summary>
	/// <seealso cref="ModuleSpec" />
	public sealed class GuardBitsSpec:ModuleSpec
	{
		
		/// <summary> Constructs an empty 'GuardBitsSpec' with specified number of tile and
		/// components. This constructor is called by the decoder.
		/// 
		/// </summary>
		/// <param name="nt">Number of tiles
		/// 
		/// </param>
		/// <param name="nc">Number of components
		/// 
		/// </param>
		/// <param name="type">the type of the specification module i.e. tile specific,
		/// component specific or both.
		/// 
		/// </param>
		public GuardBitsSpec(int nt, int nc, byte type):base(nt, nc, type)
		{
		}
		
		/// <summary> Constructs a new 'GuardBitsSpec' for the specified number of components
		/// and tiles and the arguments of "-Qguard_bits" option.
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
		/// <param name="pl">The ParameterList
		/// 
		/// </param>
		public GuardBitsSpec(int nt, int nc, byte type, ParameterList pl):base(nt, nc, type)
		{
			
			var param = pl.getParameter("Qguard_bits");
			if (param == null)
			{
				throw new ArgumentException("Qguard_bits option not specified");
			}
			
			// Parse argument
			var stk = new SupportClass.Tokenizer(param);
			string word; // current word
			var curSpecType = SPEC_DEF; // Specification type of the
			// current parameter
			bool[] tileSpec = null; // Tiles concerned by the specification
			bool[] compSpec = null; // Components concerned by the specification
			int value_Renamed; // value of the guard bits
			
			while (stk.HasMoreTokens())
			{
				word = stk.NextToken().ToLower();
				
				switch (word[0])
				{
					
					case 't':  // Tiles specification
						tileSpec = parseIdx(word, nTiles);
						curSpecType = curSpecType == SPEC_COMP_DEF ? SPEC_TILE_COMP : SPEC_TILE_DEF;
						break;
					
					case 'c':  // Components specification
						compSpec = parseIdx(word, nComp);
						curSpecType = curSpecType == SPEC_TILE_DEF ? SPEC_TILE_COMP : SPEC_COMP_DEF;
						break;
					
					default:  // Step size value
						try
						{
							value_Renamed = int.Parse(word);
						}
						catch (FormatException)
						{
							throw new ArgumentException($"Bad parameter for -Qguard_bits option : {word}");
						}
						
						if (value_Renamed <= 0.0f)
						{
							throw new ArgumentException($"Guard bits value must be positive : {value_Renamed}");
						}
						
						
						if (curSpecType == SPEC_DEF)
						{
							setDefault(value_Renamed);
						}
						else if (curSpecType == SPEC_TILE_DEF)
						{
							for (var i = tileSpec.Length - 1; i >= 0; i--)
								if (tileSpec[i])
								{
									setTileDef(i, value_Renamed);
								}
						}
						else if (curSpecType == SPEC_COMP_DEF)
						{
							for (var i = compSpec.Length - 1; i >= 0; i--)
								if (compSpec[i])
								{
									setCompDef(i, value_Renamed);
								}
						}
						else
						{
							for (var i = tileSpec.Length - 1; i >= 0; i--)
							{
								for (var j = compSpec.Length - 1; j >= 0; j--)
								{
									if (tileSpec[i] && compSpec[j])
									{
										setTileCompVal(i, j, value_Renamed);
									}
								}
							}
						}
						
						// Re-initialize
						curSpecType = SPEC_DEF;
						tileSpec = null;
						compSpec = null;
						break;
					
				}
			}
			
			// Check that default value has been specified
			if (getDefault() == null)
			{
				var ndefspec = 0;
				for (var t = nt - 1; t >= 0; t--)
				{
					for (var c = nc - 1; c >= 0; c--)
					{
						if (specValType[t][c] == SPEC_DEF)
						{
							ndefspec++;
						}
					}
				}
				
				// If some tile-component have received no specification, it takes
				// the default value defined in ParameterList
				if (ndefspec != 0)
				{
					setDefault(int.Parse(pl.DefaultParameterList.getParameter("Qguard_bits")));
				}
				else
				{
					// All tile-component have been specified, takes the first
					// tile-component value as default.
					setDefault(getTileCompVal(0, 0));
					switch (specValType[0][0])
					{
						
						case SPEC_TILE_DEF: 
							for (var c = nc - 1; c >= 0; c--)
							{
								if (specValType[0][c] == SPEC_TILE_DEF)
									specValType[0][c] = SPEC_DEF;
							}
							tileDef[0] = null;
							break;
						
						case SPEC_COMP_DEF: 
							for (var t = nt - 1; t >= 0; t--)
							{
								if (specValType[t][0] == SPEC_COMP_DEF)
									specValType[t][0] = SPEC_DEF;
							}
							compDef[0] = null;
							break;
						
						case SPEC_TILE_COMP: 
							specValType[0][0] = SPEC_DEF;
							tileCompVal["t0c0"] = null;
							break;
						}
				}
			}
		}
	}
}