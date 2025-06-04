/* 
* CVS identifier:
* 
* $Id: InvWTFull.java,v 1.20 2002/05/22 15:01:32 grosbois Exp $
* 
* Class:                   InvWTFull
* 
* Description:             This class implements a full page inverse DWT for
*                          int and float data.
* 
*                          the InvWTFullInt and InvWTFullFloat
*                          classes by Bertrand Berthelot, Apr-19-1999
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
using System;
using System.Collections.Generic;
using CoreJ2K.j2k.decoder;
using CoreJ2K.j2k.image;

namespace CoreJ2K.j2k.wavelet.synthesis
{
	
	/// <summary> This class implements the InverseWT with the full-page approach for int and
	/// float data.
	/// 
	/// The image can be reconstructed at different (image) resolution levels
	/// indexed from the lowest resolution available for each tile-component. This
	/// is controlled by the setImgResLevel() method.
	/// 
	/// Note: Image resolution level indexes may differ from tile-component
	/// resolution index. They are indeed indexed starting from the lowest number
	/// of decomposition levels of each component of each tile.
	/// 
	/// Example: For an image (1 tile) with 2 components (component 0 having 2
	/// decomposition levels and component 1 having 3 decomposition levels), the
	/// first (tile-) component has 3 resolution levels and the second one has 4
	/// resolution levels, whereas the image has only 3 resolution levels
	/// available.
	/// 
	/// This implementation does not support progressive data: Data is
	/// considered to be non-progressive (i.e. "final" data) and the 'progressive'
	/// attribute of the 'DataBlk' class is always set to false, see the 'DataBlk'
	/// class.
	/// 
	/// </summary>
	/// <seealso cref="DataBlk" />
	public class InvWTFull:InverseWT
	{
		
		/// <summary>The total number of code-blocks to decode </summary>
		private int cblkToDecode = 0;
		
		/// <summary>the code-block buffer's source i.e. the quantizer </summary>
		private CBlkWTDataSrcDec src;
		
		/// <summary>Current data type </summary>
		private int dtype;
		
		/// <summary>Block storing the reconstructed image for each component </summary>
		private DataBlk[] reconstructedComps;
		
		/// <summary>Number of decomposition levels in each component </summary>
		private int[] ndl;
		
		/// <summary> The reversible flag for each component in each tile. The first index is
		/// the tile index, the second one is the component index. The
		/// reversibility of the components for each tile are calculated on a as
		/// needed basis.
		/// 
		/// </summary>
        private Dictionary<int, bool[]> reversible = new Dictionary<int, bool[]>();
		//private bool[][] reversible;
		
		/// <summary> Initializes this object with the given source of wavelet
		/// coefficients. It initializes the resolution level for full resolutioin
		/// reconstruction.
		/// 
		/// </summary>
		/// <param name="src">from where the wavelet coefficinets should be obtained.
		/// 
		/// </param>
		/// <param name="decSpec">The decoder specifications
		/// 
		/// </param>
		public InvWTFull(CBlkWTDataSrcDec src, DecoderSpecs decSpec):base(src, decSpec)
		{
			this.src = src;
			var nc = src.NumComps;
			reconstructedComps = new DataBlk[nc];
			ndl = new int[nc];
		}
		
		/// <summary> Returns the reversibility of the current subband. It computes
		/// iteratively the reversibility of the child subbands. For each subband
		/// it tests the reversibility of the horizontal and vertical synthesis
		/// filters used to reconstruct this subband.
		/// 
		/// </summary>
		/// <param name="subband">The current subband.
		/// 
		/// </param>
		/// <returns> true if all the  filters used to reconstruct the current 
		/// subband are reversible
		/// 
		/// </returns>
		private bool isSubbandReversible(Subband subband)
		{
			if (subband.isNode)
			{
				// It's reversible if the filters to obtain the 4 subbands are
				// reversible and the ones for this one are reversible too.
				return isSubbandReversible(subband.LL) && isSubbandReversible(subband.HL) && isSubbandReversible(subband.LH) && isSubbandReversible(subband.HH) && ((SubbandSyn) subband).hFilter.Reversible && ((SubbandSyn) subband).vFilter.Reversible;
			}
			else
			{
				// Leaf subband. Reversibility of data depends on source, so say
				// it's true
				return true;
			}
		}
		
		/// <summary> Returns the reversibility of the wavelet transform for the specified
		/// component, in the current tile. A wavelet transform is reversible when
		/// it is suitable for lossless and lossy-to-lossless compression.
		/// 
		/// </summary>
		/// <param name="t">The index of the tile.
		/// 
		/// </param>
		/// <param name="c">The index of the component.
		/// 
		/// </param>
		/// <returns> true is the wavelet transform is reversible, false if not.
		/// 
		/// </returns>
		public override bool isReversible(int t, int c)
		{
			if (reversible[t] == null)
			{
				// Reversibility not yet calculated for this tile
				reversible[t] = new bool[NumComps];
				for (var i = reversible[t].Length - 1; i >= 0; i--)
				{
					reversible[t][i] = isSubbandReversible(src.getSynSubbandTree(t, i));
				}
			}
			return reversible[t][c];
		}
		
		/// <summary> Returns the number of bits, referred to as the "range bits",
		/// corresponding to the nominal range of the data in the specified
		/// component.
		/// 
		/// The returned value corresponds to the nominal dynamic range of the
		/// reconstructed image data, as long as the getNomRangeBits() method of
		/// the source returns a value corresponding to the nominal dynamic range
		/// of the image data and not not of the wavelet coefficients.
		/// 
		/// If this number is <i>b</b> then for unsigned data the nominal range
		/// is between 0 and 2^b-1, and for signed data it is between -2^(b-1) and
		/// 2^(b-1)-1.
		/// 
		/// </summary>
		/// <param name="compIndex">The index of the component.
		/// 
		/// </param>
		/// <returns> The number of bits corresponding to the nominal range of the
		/// data.
		/// 
		/// </returns>
		public override int getNomRangeBits(int compIndex)
		{
			return src.getNomRangeBits(compIndex);
		}
		
		/// <summary> Returns the position of the fixed point in the specified
		/// component. This is the position of the least significant integral
		/// (i.e. non-fractional) bit, which is equivalent to the number of
		/// fractional bits. For instance, for fixed-point values with 2 fractional
		/// bits, 2 is returned. For floating-point data this value does not apply
		/// and 0 should be returned. Position 0 is the position of the least
		/// significant bit in the data.
		/// 
		/// This default implementation assumes that the wavelet transform does
		/// not modify the fixed point. If that were the case this method should be
		/// overriden.
		/// 
		/// </summary>
		/// <param name="compIndex">The index of the component.
		/// 
		/// </param>
		/// <returns> The position of the fixed-point, which is the same as the
		/// number of fractional bits. For floating-point data 0 is returned.
		/// 
		/// </returns>
		public override int GetFixedPoint(int compIndex)
		{
			return src.getFixedPoint(compIndex);
		}
		
		/// <summary> Returns a block of image data containing the specifed rectangular area,
		/// in the specified component, as a reference to the internal buffer (see
		/// below). The rectangular area is specified by the coordinates and
		/// dimensions of the 'blk' object.
		/// 
		/// The area to return is specified by the 'ulx', 'uly', 'w' and 'h'
		/// members of the 'blk' argument. These members are not modified by this
		/// method.
		/// 
		/// The data returned by this method can be the data in the internal
		/// buffer of this object, if any, and thus can not be modified by the
		/// caller. The 'offset' and 'scanw' of the returned data can be
		/// arbitrary. See the 'DataBlk' class.
		/// 
		/// The returned data has its 'progressive' attribute unset
		/// (i.e. false).
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to return.
		/// 
		/// </param>
		/// <param name="compIndex">The index of the component from which to get the data.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="GetInternCompData" />
		public override DataBlk GetInternCompData(DataBlk blk, int compIndex)
		{
			var tIdx = TileIdx;
			dtype = src.getSynSubbandTree(tIdx, compIndex).HorWFilter == null 
				? DataBlk.TYPE_INT 
				: src.getSynSubbandTree(tIdx, compIndex).HorWFilter.DataType;
			
			//If the source image has not been decomposed 
			if (reconstructedComps[compIndex] == null)
			{
				//Allocate component data buffer
				switch (dtype)
				{
					
					case DataBlk.TYPE_FLOAT: 
						reconstructedComps[compIndex] = new DataBlkFloat(0, 0, getTileCompWidth(tIdx, compIndex), getTileCompHeight(tIdx, compIndex));
						break;
					
					case DataBlk.TYPE_INT: 
						reconstructedComps[compIndex] = new DataBlkInt(0, 0, getTileCompWidth(tIdx, compIndex), getTileCompHeight(tIdx, compIndex));
						break;
					}
				//Reconstruct source image
				waveletTreeReconstruction(reconstructedComps[compIndex], src.getSynSubbandTree(tIdx, compIndex), compIndex);
			}
			
			if (blk.DataType != dtype)
			{
				if (dtype == DataBlk.TYPE_INT)
				{
					blk = new DataBlkInt(blk.ulx, blk.uly, blk.w, blk.h);
				}
				else
				{
					blk = new DataBlkFloat(blk.ulx, blk.uly, blk.w, blk.h);
				}
			}
			// Set the reference to the internal buffer
			blk.Data = reconstructedComps[compIndex].Data;
			blk.offset = reconstructedComps[compIndex].w * blk.uly + blk.ulx;
			blk.scanw = reconstructedComps[compIndex].w;
			blk.progressive = false;
			return blk;
		}
		
		/// <summary> Returns a block of image data containing the specifed rectangular area,
		/// in the specified component, as a copy (see below). The rectangular area
		/// is specified by the coordinates and dimensions of the 'blk' object.
		/// 
		/// The area to return is specified by the 'ulx', 'uly', 'w' and 'h'
		/// members of the 'blk' argument. These members are not modified by this
		/// method.
		/// 
		/// The data returned by this method is always a copy of the internal
		/// data of this object, if any, and it can be modified "in place" without
		/// any problems after being returned. The 'offset' of the returned data is
		/// 0, and the 'scanw' is the same as the block's width. See the 'DataBlk'
		/// class.
		/// 
		/// If the data array in 'blk' is <tt>null</tt>, then a new one is
		/// created. If the data array is not <tt>null</tt> then it must be big
		/// enough to contain the requested area.
		/// 
		/// The returned data always has its 'progressive' attribute unset (i.e
		/// false)
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to
		/// return. If it contains a non-null data array, then it must be large
		/// enough. If it contains a null data array a new one is created. The
		/// fields in this object are modified to return the data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="GetCompData" />
		public override DataBlk GetCompData(DataBlk blk, int c)
		{
			//int j;
            object dst_data; // src_data removed
            int[] dst_data_int; // src_data_int removed
            float[] dst_data_float; // src_data_float removed
			
			// To keep compiler happy
			dst_data = null;
			
			// Ensure output buffer
			switch (blk.DataType)
			{
				
				case DataBlk.TYPE_INT: 
					dst_data_int = (int[]) blk.Data;
					if (dst_data_int == null || dst_data_int.Length < blk.w * blk.h)
					{
						dst_data_int = new int[blk.w * blk.h];
					}
					dst_data = dst_data_int;
					break;
				
				case DataBlk.TYPE_FLOAT: 
					dst_data_float = (float[]) blk.Data;
					if (dst_data_float == null || dst_data_float.Length < blk.w * blk.h)
					{
						dst_data_float = new float[blk.w * blk.h];
					}
					dst_data = dst_data_float;
					break;
				}
			
			// Use getInternCompData() to get the data, since getInternCompData()
			// returns reference to internal buffer, we must copy it.
			blk = GetInternCompData(blk, c);
			
			// Copy the data
			blk.Data = dst_data;
			blk.offset = 0;
			blk.scanw = blk.w;
			return blk;
		}
		
		/// <summary> Performs the 2D inverse wavelet transform on a subband of the image, on
		/// the specified component. This method will successively perform 1D
		/// filtering steps on all columns and then all lines of the subband.
		/// 
		/// </summary>
		/// <param name="db">the buffer for the image/wavelet data.
		/// 
		/// </param>
		/// <param name="sb">The subband to reconstruct.
		/// 
		/// </param>
		/// <param name="c">The index of the component to reconstruct 
		/// 
		/// </param>
		private void  wavelet2DReconstruction(DataBlk db, SubbandSyn sb, int c)
		{
			object data;
			object buf;
			int ulx, uly, w, h;
			int i, j, k;
			int offset;
			
			// If subband is empty (i.e. zero size) nothing to do
			if (sb.w == 0 || sb.h == 0)
			{
				return ;
			}
			
			data = db.Data;
			
			ulx = sb.ulx;
			uly = sb.uly;
			w = sb.w;
			h = sb.h;
			
			buf = null; // To keep compiler happy
			
			switch (sb.HorWFilter.DataType)
			{
				
				case DataBlk.TYPE_INT: 
					buf = new int[(w >= h)?w:h];
					break;
				
				case DataBlk.TYPE_FLOAT: 
					buf = new float[(w >= h)?w:h];
					break;
				}
			
			//Perform the horizontal reconstruction
			offset = (uly - db.uly) * db.w + ulx - db.ulx;
			if (sb.ulcx % 2 == 0)
			{
				// start index is even => use LPF
				for (i = 0; i < h; i++, offset += db.w)
				{
					Array.Copy((Array)data, offset, (Array)buf, 0, w);
					sb.hFilter.synthetize_lpf(buf, 0, (w + 1) / 2, 1, buf, (w + 1) / 2, w / 2, 1, data, offset, 1);
				}
			}
			else
			{
				// start index is odd => use HPF
				for (i = 0; i < h; i++, offset += db.w)
				{
					Array.Copy((Array)data, offset, (Array)buf, 0, w);
					sb.hFilter.synthetize_hpf(buf, 0, w / 2, 1, buf, w / 2, (w + 1) / 2, 1, data, offset, 1);
				}
			}
			
			//Perform the vertical reconstruction 
			offset = (uly - db.uly) * db.w + ulx - db.ulx;
			switch (sb.VerWFilter.DataType)
			{
				
				case DataBlk.TYPE_INT: 
					int[] data_int, buf_int;
					data_int = (int[]) data;
					buf_int = (int[]) buf;
					if (sb.ulcy % 2 == 0)
					{
						// start index is even => use LPF
						for (j = 0; j < w; j++, offset++)
						{
							for (i = h - 1, k = offset + i * db.w; i >= 0; i--, k -= db.w)
								buf_int[i] = data_int[k];
							sb.vFilter.synthetize_lpf(buf, 0, (h + 1) / 2, 1, buf, (h + 1) / 2, h / 2, 1, data, offset, db.w);
						}
					}
					else
					{
						// start index is odd => use HPF
						for (j = 0; j < w; j++, offset++)
						{
							for (i = h - 1, k = offset + i * db.w; i >= 0; i--, k -= db.w)
								buf_int[i] = data_int[k];
							sb.vFilter.synthetize_hpf(buf, 0, h / 2, 1, buf, h / 2, (h + 1) / 2, 1, data, offset, db.w);
						}
					}
					break;
				
				case DataBlk.TYPE_FLOAT: 
					float[] data_float, buf_float;
					data_float = (float[]) data;
					buf_float = (float[]) buf;
					if (sb.ulcy % 2 == 0)
					{
						// start index is even => use LPF
						for (j = 0; j < w; j++, offset++)
						{
							for (i = h - 1, k = offset + i * db.w; i >= 0; i--, k -= db.w)
								buf_float[i] = data_float[k];
							sb.vFilter.synthetize_lpf(buf, 0, (h + 1) / 2, 1, buf, (h + 1) / 2, h / 2, 1, data, offset, db.w);
						}
					}
					else
					{
						// start index is odd => use HPF
						for (j = 0; j < w; j++, offset++)
						{
							for (i = h - 1, k = offset + i * db.w; i >= 0; i--, k -= db.w)
								buf_float[i] = data_float[k];
							sb.vFilter.synthetize_hpf(buf, 0, h / 2, 1, buf, h / 2, (h + 1) / 2, 1, data, offset, db.w);
						}
					}
					break;
				}
		}
		
		/// <summary> Performs the inverse wavelet transform on the whole component. It
		/// iteratively reconstructs the subbands from leaves up to the root
		/// node. This method is recursive, the first call to it the 'sb' must be
		/// the root of the subband tree. The method will then process the entire
		/// subband tree by calling itslef recursively.
		/// 
		/// </summary>
		/// <param name="img">The buffer for the image/wavelet data.
		/// 
		/// </param>
		/// <param name="sb">The subband to reconstruct.
		/// 
		/// </param>
		/// <param name="c">The index of the component to reconstruct 
		/// 
		/// </param>
		private void  waveletTreeReconstruction(DataBlk img, SubbandSyn sb, int c)
		{
			
			DataBlk subbData;
			
			// If the current subband is a leaf then get the data from the source
			if (!sb.isNode)
			{
				int i, m, n;
				object src_data, dst_data;
				Coord ncblks;
				
				if (sb.w == 0 || sb.h == 0)
				{
					return ; // If empty subband do nothing
				}
				
				// Get all code-blocks in subband
				if (dtype == DataBlk.TYPE_INT)
				{
					subbData = new DataBlkInt();
				}
				else
				{
					subbData = new DataBlkFloat();
				}
				ncblks = sb.numCb;
				dst_data = img.Data;
				for (m = 0; m < ncblks.y; m++)
				{
					for (n = 0; n < ncblks.x; n++)
					{
						subbData = src.getInternCodeBlock(c, m, n, sb, subbData);
						src_data = subbData.Data;

						// Copy the data line by line
						for (i = subbData.h - 1; i >= 0; i--)
						{
                            // CONVERSION PROBLEM
							Array.Copy((Array)src_data, subbData.offset + i * subbData.scanw, (Array)dst_data, (subbData.uly + i) * img.w + subbData.ulx, subbData.w);
						}
					}
				}
			}
			else if (sb.isNode)
			{
				// Reconstruct the lower resolution levels if the current subbands
				// is a node
				
				//Perform the reconstruction of the LL subband
				waveletTreeReconstruction(img, (SubbandSyn) sb.LL, c);
				
				if (sb.resLvl <= reslvl - maxImgRes + ndl[c])
				{
					//Reconstruct the other subbands
					waveletTreeReconstruction(img, (SubbandSyn) sb.HL, c);
					waveletTreeReconstruction(img, (SubbandSyn) sb.LH, c);
					waveletTreeReconstruction(img, (SubbandSyn) sb.HH, c);
					
					//Perform the 2D wavelet decomposition of the current subband
					wavelet2DReconstruction(img, sb, c);
				}
			}
		}
		
		/// <summary> Returns the implementation type of this wavelet transform, WT_IMPL_FULL
		/// (full-page based transform). All components return the same.
		/// 
		/// </summary>
		/// <param name="c">The index of the component.
		/// 
		/// </param>
		/// <returns> WT_IMPL_FULL
		/// 
		/// </returns>
		/// <seealso cref="WaveletTransform.WT_IMPL_FULL" />
		public override int getImplementationType(int c)
		{
			return WaveletTransform_Fields.WT_IMPL_FULL;
		}
		
		/// <summary> Changes the current tile, given the new indexes. An
		/// IllegalArgumentException is thrown if the indexes do not correspond to
		/// a valid tile.
		/// 
		/// </summary>
		/// <param name="x">The horizontal index of the tile.
		/// 
		/// </param>
		/// <param name="y">The vertical index of the new tile.
		/// 
		/// </param>
		public override void  setTile(int x, int y)
		{
			int i;
			
			// Change tile
			base.setTile(x, y);
			
			var nc = src.NumComps;
			var tIdx = src.TileIdx;
			for (var c = 0; c < nc; c++)
			{
				ndl[c] = src.getSynSubbandTree(tIdx, c).resLvl;
			}
			
			// Reset the decomposed component buffers.
			if (reconstructedComps != null)
			{
				for (i = reconstructedComps.Length - 1; i >= 0; i--)
				{
					reconstructedComps[i] = null;
				}
			}
			
			cblkToDecode = 0;
			SubbandSyn root, sb;
			for (var c = 0; c < nc; c++)
			{
				root = src.getSynSubbandTree(tIdx, c);
				for (var r = 0; r <= reslvl - maxImgRes + root.resLvl; r++)
				{
					if (r == 0)
					{
						sb = (SubbandSyn) root.getSubbandByIdx(0, 0);
						if (sb != null)
							cblkToDecode += sb.numCb.x * sb.numCb.y;
					}
					else
					{
						sb = (SubbandSyn) root.getSubbandByIdx(r, 1);
						if (sb != null)
							cblkToDecode += sb.numCb.x * sb.numCb.y;
						sb = (SubbandSyn) root.getSubbandByIdx(r, 2);
						if (sb != null)
							cblkToDecode += sb.numCb.x * sb.numCb.y;
						sb = (SubbandSyn) root.getSubbandByIdx(r, 3);
						if (sb != null)
							cblkToDecode += sb.numCb.x * sb.numCb.y;
					}
				} // Loop on resolution levels
			} // Loop on components
		}
		
		/// <summary> Advances to the next tile, in standard scan-line order (by rows then
		/// columns). An 'NoNextElementException' is thrown if the current tile is
		/// the last one (i.e. there is no next tile).
		/// 
		/// </summary>
		public override void  nextTile()
		{
			int i;
			
			// Change tile
			base.nextTile();
			
			var nc = src.NumComps;
			var tIdx = src.TileIdx;
			for (var c = 0; c < nc; c++)
			{
				ndl[c] = src.getSynSubbandTree(tIdx, c).resLvl;
			}
			
			// Reset the decomposed component buffers.
			if (reconstructedComps != null)
			{
				for (i = reconstructedComps.Length - 1; i >= 0; i--)
				{
					reconstructedComps[i] = null;
				}
			}
		}
	}
}