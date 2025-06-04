/*
* CVS identifier:
*
* $Id: HeaderDecoder.java,v 1.61 2002/07/25 15:01:00 grosbois Exp $
*
* Class:                   HeaderDecoder
*
* Description:             Reads main and tile-part headers.
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
using System;
using System.Collections.Generic;
using CoreJ2K.Color;
using CoreJ2K.Icc;
using CoreJ2K.j2k.decoder;
using CoreJ2K.j2k.entropy;
using CoreJ2K.j2k.entropy.decoder;
using CoreJ2K.j2k.image;
using CoreJ2K.j2k.io;
using CoreJ2K.j2k.quantization.dequantizer;
using CoreJ2K.j2k.roi;
using CoreJ2K.j2k.util;
using CoreJ2K.j2k.wavelet;
using CoreJ2K.j2k.wavelet.synthesis;

namespace CoreJ2K.j2k.codestream.reader
{
	
	/// <summary> This class reads main and tile-part headers from the codestream given a
	/// RandomAccessIO instance located at the beginning of the codestream (i.e
	/// just before the SOC marker) or at the beginning of a tile-part (i.e. just
	/// before a SOT marker segment) respectively.
	/// 
	/// A marker segment includes a marker and eventually marker segment
	/// parameters. It is designed by the three letters code of the marker
	/// associated with the marker segment. JPEG 2000 part 1 defines 6 types of
	/// markers segments:
	/// 
	/// <ul> 
	/// <li> Delimiting : SOC, SOT, SOD, EOC</li> 
	/// 
	/// <li> Fixed information: SIZ.</li>
	/// 
	/// <li> Functional: COD, COC, RGN, QCD, QCC,POC.</li> 
	/// 
	/// <li> In bit-stream: SOP, EPH.</li>
	/// 
	/// <li> Pointer: TLM, PLM, PLT, PPM, PPT.</li>
	/// 
	/// <li> Informational: CRG, COM.</li>
	/// </ul>
	/// 
	/// The main header is read when the constructor is called whereas tile-part
	/// headers are read when the FileBitstreamReaderAgent instance is created. The
	/// reading is done in 2 passes:
	/// 
	/// <ol> 
	/// <li>All marker segments are buffered and their corresponding flag is
	/// activated (extractMainMarkSeg and extractTilePartMarkSeg methods).</li>
	/// 
	/// <li>Buffered marker segment are analyzed in a logical way and
	/// specifications are stored in appropriate member of DecoderSpecs instance
	/// (readFoundMainMarkSeg and readFoundTilePartMarkSeg methods).</li>
	/// </ol>
	/// 
	/// Whenever a marker segment is not recognized a warning message is
	/// displayed and its length parameter is used to skip it.
	/// 
	/// The information found in this header is stored in HeaderInfo and
	/// DecoderSpecs instances.
	/// 
	/// </summary>
	/// <seealso cref="DecoderSpecs" />
	/// <seealso cref="HeaderInfo" />
	/// <seealso cref="Decoder" />
	/// <seealso cref="FileBitstreamReaderAgent" />
	public class HeaderDecoder
	{
		/// <summary> Return the maximum height among all components 
		/// 
		/// </summary>
		/// <returns> Maximum component height
		/// 
		/// </returns>
		public virtual int MaxCompImgHeight => hi.sizValue.MaxCompHeight;

		/// <summary> Return the maximum width among all components 
		/// 
		/// </summary>
		/// <returns> Maximum component width
		/// 
		/// </returns>
		public virtual int MaxCompImgWidth => hi.sizValue.MaxCompWidth;

		/// <summary> Returns the image width in the reference grid.
		/// 
		/// </summary>
		/// <returns> The image width in the reference grid
		/// 
		/// </returns>
		public virtual int ImgWidth => hi.sizValue.xsiz - hi.sizValue.x0siz;

		/// <summary> Returns the image height in the reference grid.
		/// 
		/// </summary>
		/// <returns> The image height in the reference grid
		/// 
		/// </returns>
		public virtual int ImgHeight => hi.sizValue.ysiz - hi.sizValue.y0siz;

		/// <summary> Return the horizontal upper-left coordinate of the image in the
		/// reference grid.
		/// 
		/// </summary>
		/// <returns> The horizontal coordinate of the image origin.
		/// 
		/// </returns>
		public virtual int ImgULX => hi.sizValue.x0siz;

		/// <summary> Return the vertical upper-left coordinate of the image in the reference
		/// grid.
		/// 
		/// </summary>
		/// <returns> The vertical coordinate of the image origin.
		/// 
		/// </returns>
		public virtual int ImgULY => hi.sizValue.y0siz;

		/// <summary> Returns the nominal width of the tiles in the reference grid.
		/// 
		/// </summary>
		/// <returns> The nominal tile width, in the reference grid.
		/// 
		/// </returns>
		public virtual int NomTileWidth => hi.sizValue.xtsiz;

		/// <summary> Returns the nominal width of the tiles in the reference grid.
		/// 
		/// </summary>
		/// <returns> The nominal tile width, in the reference grid.
		/// 
		/// </returns>
		public virtual int NomTileHeight => hi.sizValue.ytsiz;

		/// <summary> Returns the number of components in the image.
		/// 
		/// </summary>
		/// <returns> The number of components in the image.
		/// 
		/// </returns>
		public virtual int NumComps => nComp;

		/// <summary> Returns the horizontal code-block partition origin.Allowable values are
		/// 0 and 1, nothing else.
		/// 
		/// </summary>
		public virtual int CbULX => cb0x;

		/// <summary> Returns the vertical code-block partition origin. Allowable values are
		/// 0 and 1, nothing else.
		/// 
		/// </summary>
		public virtual int CbULY => cb0y;

		/// <summary> Return the DecoderSpecs instance filled when reading the headers
		/// 
		/// </summary>
		/// <returns> The DecoderSpecs of the decoder
		/// 
		/// </returns>
		public virtual DecoderSpecs DecoderSpecs => decSpec;

		/// <summary> Returns the parameters that are used in this class. It returns a 2D
		/// String array. Each of the 1D arrays is for a different option, and they
		/// have 3 elements. The first element is the option name, the second one
		/// is the synopsis and the third one is a long description of what the
		/// parameter is. The synopsis or description may be 'null', in which case
		/// it is assumed that there is no synopsis or description of the option,
		/// respectively.
		/// 
		/// </summary>
		/// <returns> the options name, their synopsis and their explanation.
		/// 
		/// </returns>
		public static string[][] ParameterInfo => pinfo;

		/// <summary> Return the number of tiles in the image
		/// 
		/// </summary>
		/// <returns> The number of tiles
		/// 
		/// </returns>
		public virtual int NumTiles => nTiles;

		/// <summary> Sets the tile of each tile part in order. This information is needed
		/// for identifying which packet header belongs to which tile when using
		/// the PPM marker.
		/// 
		/// </summary>
		/// <param name="tile">The tile number that the present tile part belongs to.
		/// 
		/// </param>
		public virtual int TileOfTileParts
		{
			set
			{
				if (nPPMMarkSeg != 0)
				{
					tileOfTileParts.Add(value);
				}
			}
			
		}
		/// <summary> Returns the number of found marker segments in the current header.
		/// 
		/// </summary>
		/// <returns> The number of marker segments found in the current header.
		/// 
		/// </returns>
		public virtual int NumFoundMarkSeg => nfMarkSeg;

		/// <summary>The prefix for header decoder options: 'H' </summary>
		public const char OPT_PREFIX = 'H';
		
		/// <summary>The list of parameters that is accepted for quantization. Options 
		/// for quantization start with 'Q'. 
		/// </summary>
		private static readonly string[][] pinfo = null;
		
		/// <summary>The reference to the HeaderInfo instance holding the information found
		/// in headers 
		/// </summary>
		private HeaderInfo hi;
		
		/// <summary>Whether or not to display general information </summary>
		//private bool verbose;
		
		/// <summary>Current header information in a string </summary>
		private string hdStr = "";
		
		/// <summary>The number of tiles within the image </summary>
		private int nTiles;
		
		/// <summary>The number of tile parts in each tile </summary>
		public int[] nTileParts;
		
		/// <summary>Used to store which markers have been already read, by using flag
		/// bits. The different markers are marked with XXX_FOUND flags, such as
		/// SIZ_FOUND 
		/// </summary>
		private int nfMarkSeg = 0;
		
		/// <summary>Counts number of COC markers found in the header </summary>
		private int nCOCMarkSeg = 0;
		
		/// <summary>Counts number of QCC markers found in the header </summary>
		private int nQCCMarkSeg = 0;
		
		/// <summary>Counts number of COM markers found in the header </summary>
		private int nCOMMarkSeg = 0;
		
		/// <summary>Counts number of RGN markers found in the header </summary>
		private int nRGNMarkSeg = 0;
		
		/// <summary>Counts number of PPM markers found in the header </summary>
		private int nPPMMarkSeg = 0;
		
		/// <summary>Counts number of PPT markers found in the header </summary>
		private int[][] nPPTMarkSeg = null;
		
		/// <summary>Flag bit for SIZ marker segment found </summary>
		private const int SIZ_FOUND = 1;
		
		/// <summary>Flag bit for COD marker segment found </summary>
		private const int COD_FOUND = 1 << 1;
		
		/// <summary>Flag bit for COC marker segment found </summary>
		private const int COC_FOUND = 1 << 2;
		
		/// <summary>Flag bit for QCD marker segment found </summary>
		private const int QCD_FOUND = 1 << 3;
		
		/// <summary>Flag bit for TLM marker segment found </summary>
		private const int TLM_FOUND = 1 << 4;
		
		/// <summary>Flag bit for PLM marker segment found </summary>
		private const int PLM_FOUND = 1 << 5;
		
		/// <summary>Flag bit for SOT marker segment found </summary>
		private const int SOT_FOUND = 1 << 6;
		
		/// <summary>Flag bit for PLT marker segment found </summary>
		private const int PLT_FOUND = 1 << 7;
		
		/// <summary>Flag bit for QCC marker segment found </summary>
		private const int QCC_FOUND = 1 << 8;
		
		/// <summary>Flag bit for RGN marker segment found </summary>
		private const int RGN_FOUND = 1 << 9;
		
		/// <summary>Flag bit for POC marker segment found </summary>
		private const int POC_FOUND = 1 << 10;
		
		/// <summary>Flag bit for COM marker segment found </summary>
		private const int COM_FOUND = 1 << 11;
		
		/// <summary>Flag bit for SOD marker segment found </summary>
		public const int SOD_FOUND = 1 << 13;
		
		/// <summary>Flag bit for SOD marker segment found </summary>
		public const int PPM_FOUND = 1 << 14;
		
		/// <summary>Flag bit for SOD marker segment found </summary>
		public const int PPT_FOUND = 1 << 15;
		
		/// <summary>Flag bit for CRG marker segment found </summary>
		public const int CRG_FOUND = 1 << 16;
		
		/// <summary>The reset mask for new tiles </summary>
		//private static readonly int TILE_RESET = ~ (PLM_FOUND | SIZ_FOUND | RGN_FOUND);
		
		/// <summary>HashTable used to store temporary marker segment byte buffers </summary>
		private Dictionary<string, byte[]> ht = null;
		
		/// <summary>The number of components in the image </summary>
		private int nComp;
		
		/// <summary>The horizontal code-block partition origin </summary>
		private int cb0x = - 1;
		
		/// <summary>The vertical code-block partition origin </summary>
		private int cb0y = - 1;
		
		/// <summary>The decoder specifications </summary>
		private DecoderSpecs decSpec;
		
		/// <summary>Is the precinct partition used </summary>
		internal bool precinctPartitionIsUsed;
		
		/// <summary>The offset of the main header in the input stream </summary>
		public int mainHeadOff;
		
		/// <summary>Vector containing info as to which tile each tilepart belong </summary>
		private List<int> tileOfTileParts;
		
		/// <summary>Array containing the Nppm and Ippm fields of the PPM marker segments</summary>
		private byte[][] pPMMarkerData;
		
		/// <summary>Array containing the Ippm fields of the PPT marker segments </summary>
		private byte[][][][] tilePartPkdPktHeaders;
		
		/// <summary>The packed packet headers if the PPM or PPT markers are used </summary>
		private System.IO.MemoryStream[] pkdPktHeaders;
		
		/// <summary> Returns the tiling origin, referred to as '(Px,Py)' in the 'ImgData'
		/// interface.
		/// 
		/// </summary>
		/// <param name="co">If not null this object is used to return the information. If
		/// null a new one is created and returned.
		/// 
		/// </param>
		/// <returns> The coordinate of the tiling origin, in the canvas system, on
		/// the reference grid.
		/// 
		/// </returns>
		/// <seealso cref="j2k.image.ImgData" />
		public Coord getTilingOrigin(Coord co)
		{
			if (co != null)
			{
				co.x = hi.sizValue.xt0siz;
				co.y = hi.sizValue.yt0siz;
				return co;
			}
			else
			{
				return new Coord(hi.sizValue.xt0siz, hi.sizValue.yt0siz);
			}
		}
		
		/// <summary> Returns true if the original data of the specified component was
		/// signed. If the data was not signed a level shift has to be applied at
		/// the end of the decompression chain.
		/// 
		/// </summary>
		/// <param name="c">The index of the component
		/// 
		/// </param>
		/// <returns> True if the original image component was signed.
		/// 
		/// </returns>
		public bool isOriginalSigned(int c)
		{
			return hi.sizValue.isOrigSigned(c);
		}
		
		/// <summary> Returns the original bitdepth of the specified component.
		/// 
		/// </summary>
		/// <param name="c">The index of the component
		/// 
		/// </param>
		/// <returns> The bitdepth of the component
		/// 
		/// </returns>
		public int getOriginalBitDepth(int c)
		{
			return hi.sizValue.getOrigBitDepth(c);
		}
		
		/// <summary> Returns the component sub-sampling factor, with respect to the
		/// reference grid, along the horizontal direction for the specified
		/// component.
		/// 
		/// </summary>
		/// <param name="c">The index of the component
		/// 
		/// </param>
		/// <returns> The component sub-sampling factor X-wise.
		/// 
		/// </returns>
		public int getCompSubsX(int c)
		{
			return hi.sizValue.xrsiz[c];
		}
		
		/// <summary> Returns the component sub-sampling factor, with respect to the
		/// reference grid, along the vertical direction for the specified
		/// component.
		/// 
		/// </summary>
		/// <param name="c">The index of the component
		/// 
		/// </param>
		/// <returns> The component sub-sampling factor Y-wise.
		/// 
		/// </returns>
		public int getCompSubsY(int c)
		{
			return hi.sizValue.yrsiz[c];
		}
		
		/// <summary> Returns the dequantizer parameters. Dequantizer parameters normally are
		/// the quantization step sizes, see DequantizerParams.
		/// 
		/// </summary>
		/// <param name="src">The source of data for the dequantizer.
		/// 
		/// </param>
		/// <param name="rb">The number of range bits for each component. Must be
		/// the number of range bits of the mixed components.
		/// 
		/// </param>
		/// <param name="decSpec2">The DecoderSpecs instance after any image manipulation.
		/// 
		/// </param>
		/// <returns> The dequantizer
		/// 
		/// </returns>
		public static Dequantizer createDequantizer(CBlkQuantDataSrcDec src, int[] rb, DecoderSpecs decSpec2)
		{
			return new StdDequantizer(src, rb, decSpec2);
		}
		
		/// <summary> Returns the precinct partition width for the specified tile-component
		/// and resolution level.
		/// 
		/// </summary>
		/// <param name="c">the component index
		/// 
		/// </param>
		/// <param name="t">the tile index
		/// 
		/// </param>
		/// <param name="rl">the resolution level
		/// 
		/// </param>
		/// <returns> The precinct partition width for the specified tile-component
		/// and resolution level
		/// 
		/// </returns>
		public int getPPX(int t, int c, int rl)
		{
			return decSpec.pss.getPPX(t, c, rl);
		}
		
		/// <summary> Returns the precinct partition height for the specified tile-component
		/// and resolution level.
		/// 
		/// </summary>
		/// <param name="c">the component index
		/// 
		/// </param>
		/// <param name="t">the tile index
		/// 
		/// </param>
		/// <param name="rl">the resolution level
		/// 
		/// </param>
		/// <returns> The precinct partition height for the specified tile-component
		/// and resolution level
		/// 
		/// </returns>
		public int getPPY(int t, int c, int rl)
		{
			return decSpec.pss.getPPY(t, c, rl);
		}
		
		/// <summary> Returns the boolean used to know if the precinct partition is used
		/// 
		/// </summary>
		public bool precinctPartitionUsed()
		{
			return precinctPartitionIsUsed;
		}
		
		/// <summary> Reads a wavelet filter from the codestream and returns the filter
		/// object that implements it.
		/// 
		/// </summary>
		/// <param name="ehs">The encoded header stream from where to read the info
		/// 
		/// </param>
		/// <param name="filtIdx">Int array of one element to return the type of the
		/// wavelet filter.
		/// 
		/// </param>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private SynWTFilter readFilter(System.IO.BinaryReader ehs, int[] filtIdx)
		{
			int kid; // the filter id
			
			kid = filtIdx[0] = ehs.ReadByte();
			if (kid >= (1 << 7))
			{
				throw new NotImplementedException("Custom filters not supported");
			}
			// Return filter based on ID
			switch (kid)
			{
				
				case FilterTypes_Fields.W9X7: 
					return new SynWTFilterFloatLift9x7();
				
				case FilterTypes_Fields.W5X3: 
					return new SynWTFilterIntLift5x3();
				
				default: 
					throw new CorruptedCodestreamException("Specified wavelet filter " + "not" + " JPEG 2000 part I " + "compliant");
				
			}
		}
		
		/// <summary> Checks that the marker segment length is correct. 
		/// 
		/// </summary>
		/// <param name="ehs">The encoded header stream
		/// 
		/// </param>
		/// <param name="str">The string identifying the marker, such as "SIZ marker"
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		public virtual void  checkMarkerLength(System.IO.BinaryReader ehs, string str)
		{
			long available;
			available = ehs.BaseStream.Length - ehs.BaseStream.Position;
			if ((int) available != 0)
			{
				FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING,
					$"{str} length was short, attempting to resync.");
			}
		}
		
		/// <summary> Reads the SIZ marker segment and realigns the codestream at the point
		/// where the next marker segment should be found. 
		/// 
		/// SIZ is a fixed information marker segment containing informations
		/// about image and tile sizes. It is required in the main header
		/// immediately after SOC.
		/// 
		/// </summary>
		/// <param name="ehs">The encoded header stream
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoded header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readSIZ(System.IO.BinaryReader ehs)
		{
			var ms = hi.NewSIZ;
			hi.sizValue = ms;
			
			// Read the length of SIZ marker segment (Lsiz)
			ms.lsiz = ehs.ReadUInt16();
			
			// Read the capability of the codestream (Rsiz)
			ms.rsiz = ehs.ReadUInt16();
			if (ms.rsiz > 2)
			{
				throw new InvalidOperationException("Codestream capabiities not JPEG 2000 - Part I" + " compliant");
			}
			
			// Read image size
			ms.xsiz = ehs.ReadInt32();
			ms.ysiz = ehs.ReadInt32();
			if (ms.xsiz <= 0 || ms.ysiz <= 0)
			{
				throw new System.IO.IOException("JJ2000 does not support images whose " + "width and/or height not in the " + "range: 1 -- (2^31)-1");
			}
			
			// Read image offset
			ms.x0siz = ehs.ReadInt32();
			ms.y0siz = ehs.ReadInt32();
			if (ms.x0siz < 0 || ms.y0siz < 0)
			{
				throw new System.IO.IOException("JJ2000 does not support images offset " + "not in the range: 0 -- (2^31)-1");
			}
			
			// Read size of tile
			ms.xtsiz = ehs.ReadInt32();
			ms.ytsiz = ehs.ReadInt32();
			if (ms.xtsiz <= 0 || ms.ytsiz <= 0)
			{
				throw new System.IO.IOException("JJ2000 does not support tiles whose " + "width and/or height are not in  " + "the range: 1 -- (2^31)-1");
			}
			
			// Read upper-left tile offset
			ms.xt0siz = ehs.ReadInt32();
			ms.yt0siz = ehs.ReadInt32();
			if (ms.xt0siz < 0 || ms.yt0siz < 0)
			{
				throw new System.IO.IOException("JJ2000 does not support tiles whose " + "offset is not in  " + "the range: 0 -- (2^31)-1");
			}
			
			// Read number of components and initialize related arrays
			nComp = ms.csiz = ehs.ReadUInt16();
			if (nComp < 1 || nComp > 16384)
			{
				throw new ArgumentException($"Number of component out of range 1--16384: {nComp}");
			}
			
			ms.ssiz = new int[nComp];
			ms.xrsiz = new int[nComp];
			ms.yrsiz = new int[nComp];
			
			// Read bit-depth and down-sampling factors of each component
			for (var i = 0; i < nComp; i++)
			{
				ms.ssiz[i] = ehs.ReadByte();
				ms.xrsiz[i] = ehs.ReadByte();
				ms.yrsiz[i] = ehs.ReadByte();
			}
			
			// Check marker length
			checkMarkerLength(ehs, "SIZ marker");
			
			// Create needed ModuleSpec
			nTiles = ms.NumTiles;
			
			// Finish initialization of decSpec
			decSpec = new DecoderSpecs(nTiles, nComp);
		}
		
		/// <summary> Reads a CRG marker segment and checks its length. CRG is an
		/// informational marker segment that allows specific registration of
		/// components with respect to each other.
		/// 
		/// </summary>
		/// <param name="ehs">The encoded header stream
		/// 
		/// </param>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readCRG(System.IO.BinaryReader ehs)
		{
			var ms = hi.NewCRG;
			hi.crgValue = ms;
			
			ms.lcrg = ehs.ReadUInt16();
			ms.xcrg = new int[nComp];
			ms.ycrg = new int[nComp];
			
			FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING, "Information in CRG marker segment " + "not taken into account. This may affect the display " + "of the decoded image.");
			for (var c = 0; c < nComp; c++)
			{
				ms.xcrg[c] = ehs.ReadUInt16();
				ms.ycrg[c] = ehs.ReadUInt16();
			}
			
			// Check marker length
			checkMarkerLength(ehs, "CRG marker");
		}
		
		/// <summary> Reads a COM marker segments and realigns the bit stream at the point
		/// where the next marker segment should be found. COM is an informational
		/// marker segment that allows to include unstructured data in the main and
		/// tile-part headers.
		/// 
		/// </summary>
		/// <param name="ehs">The encoded header stream
		/// 
		/// </param>
		/// <param name="mainh">Flag indicating whether or not this marker segment is read
		/// from the main header.
		/// 
		/// </param>
		/// <param name="tileIdx">The index of the current tile
		/// 
		/// </param>
		/// <param name="comIdx">Occurence of this COM marker in eith main or tile-part
		/// header 
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoded header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readCOM(System.IO.BinaryReader ehs, bool mainh, int tileIdx, int comIdx)
		{
			var ms = hi.NewCOM;
			
			// Read length of COM field
			ms.lcom = ehs.ReadUInt16();
			
			// Read the registration value of the COM marker segment
			ms.rcom = ehs.ReadUInt16();
			switch (ms.rcom)
			{
				
				case Markers.RCOM_GEN_USE: 
					ms.ccom = new byte[ms.lcom - 4];
					for (var i = 0; i < ms.lcom - 4; i++)
					{
						ms.ccom[i] = ehs.ReadByte();
					}
					break;
				
				default: 
					// --- Unknown or unsupported markers ---
					// (skip them and see if we can get way with it)
					FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING,
						$"COM marker registered as 0x{Convert.ToString(ms.rcom, 16)} unknown, ignoring (this might crash the decoder or decode a quality degraded or even useless image)");
					System.IO.BinaryReader temp_BinaryReader;
					long temp_Int64;
					temp_BinaryReader = ehs;
					temp_Int64 = temp_BinaryReader.BaseStream.Position;
					temp_Int64 = temp_BinaryReader.BaseStream.Seek(ms.lcom - 4, System.IO.SeekOrigin.Current) - temp_Int64;
                    // CONVERSION PROBLEM?
					var generatedAux2 = (int)temp_Int64; //Ignore this field for the moment
					break;
				
			}
			
			if (mainh)
			{
				hi.comValue[$"main_{comIdx}"] = ms;
			}
			else
			{
				hi.comValue[$"t{tileIdx}_{comIdx}"] = ms;
			}
			
			// Check marker length
			checkMarkerLength(ehs, "COM marker");
		}
		
		/// <summary> Reads a QCD marker segment and realigns the codestream at the point
		/// where the next marker should be found. QCD is a functional marker
		/// segment that describes the quantization default.
		/// 
		/// </summary>
		/// <param name="ehs">The encoded stream.
		/// 
		/// </param>
		/// <param name="mainh">Flag indicating whether or not this marker segment is read
		/// from the main header.
		/// 
		/// </param>
		/// <param name="tileIdx">The index of the current tile
		/// 
		/// </param>
		/// <param name="tpIdx">Tile-part index
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoded header stream.
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readQCD(System.IO.BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
		{
			StdDequantizerParams qParms;
			int guardBits;
			int[][] exp;
			float[][] nStep = null;
			var ms = hi.NewQCD;
			
			// Lqcd (length of QCD field)
			ms.lqcd = ehs.ReadUInt16();
			
			// Sqcd (quantization style)
			ms.sqcd = ehs.ReadByte();
			
			guardBits = ms.NumGuardBits;
			var qType = ms.QuantType;
			
			if (mainh)
			{
				hi.qcdValue["main"] = ms;
				// If the main header is being read set default value of
				// dequantization spec
				switch (qType)
				{
					
					case Markers.SQCX_NO_QUANTIZATION: 
						decSpec.qts.setDefault("reversible");
						break;
					
					case Markers.SQCX_SCALAR_DERIVED: 
						decSpec.qts.setDefault("derived");
						break;
					
					case Markers.SQCX_SCALAR_EXPOUNDED: 
						decSpec.qts.setDefault("expounded");
						break;
					
					default: 
						throw new CorruptedCodestreamException("Unknown or " + "unsupported " + "quantization style " + "in Sqcd field, QCD " + "marker main header");
					
				}
			}
			else
			{
				hi.qcdValue[$"t{tileIdx}"] = ms;
				// If the tile header is being read set default value of
				// dequantization spec for tile
				switch (qType)
				{
					
					case Markers.SQCX_NO_QUANTIZATION: 
						decSpec.qts.setTileDef(tileIdx, "reversible");
						break;
					
					case Markers.SQCX_SCALAR_DERIVED: 
						decSpec.qts.setTileDef(tileIdx, "derived");
						break;
					
					case Markers.SQCX_SCALAR_EXPOUNDED: 
						decSpec.qts.setTileDef(tileIdx, "expounded");
						break;
					
					default: 
						throw new CorruptedCodestreamException("Unknown or " + "unsupported " + "quantization style " + "in Sqcd field, QCD " + "marker, tile header");
					
				}
			}
			
			qParms = new StdDequantizerParams();
			
			if (qType == Markers.SQCX_NO_QUANTIZATION)
			{
				var maxrl = (mainh?((int) decSpec.dls.getDefault()):((int) decSpec.dls.getTileDef(tileIdx)));
				int j, rl; // i removed
				int minb, maxb, hpd;
				int tmp;
				
				exp = qParms.exp = new int[maxrl + 1][];
				var tmpArray = new int[maxrl + 1][];
				for (var i2 = 0; i2 < maxrl + 1; i2++)
				{
					tmpArray[i2] = new int[4];
				}
				ms.spqcd = tmpArray;
				
				for (rl = 0; rl <= maxrl; rl++)
				{
					// Loop on resolution levels
					// Find the number of subbands in the resolution level
					if (rl == 0)
					{
						// Only the LL subband
						minb = 0;
						maxb = 1;
					}
					else
					{
						// Dyadic decomposition
						hpd = 1;
						
						// Adapt hpd to resolution level
						if (hpd > maxrl - rl)
						{
							hpd -= (maxrl - rl);
						}
						else
						{
							hpd = 1;
						}
						// Determine max and min subband index
						minb = 1 << ((hpd - 1) << 1); // minb = 4^(hpd-1)
						maxb = 1 << (hpd << 1); // maxb = 4^hpd
					}
					// Allocate array for subbands in resolution level
					exp[rl] = new int[maxb];
					
					for (j = minb; j < maxb; j++)
					{
						tmp = ms.spqcd[rl][j] = ehs.ReadByte();
						exp[rl][j] = (tmp >> Markers.SQCX_EXP_SHIFT) & Markers.SQCX_EXP_MASK;
					}
				} // end for rl
			}
			else
			{
				var maxrl = (qType == Markers.SQCX_SCALAR_DERIVED)?0:(mainh?((int) decSpec.dls.getDefault()):((int) decSpec.dls.getTileDef(tileIdx)));
				int j, rl; // i removed
				int minb, maxb, hpd;
				int tmp;
				
				exp = qParms.exp = new int[maxrl + 1][];
				nStep = qParms.nStep = new float[maxrl + 1][];
				var tmpArray2 = new int[maxrl + 1][];
				for (var i3 = 0; i3 < maxrl + 1; i3++)
				{
					tmpArray2[i3] = new int[4];
				}
				ms.spqcd = tmpArray2;
				
				for (rl = 0; rl <= maxrl; rl++)
				{
					// Loop on resolution levels
					// Find the number of subbands in the resolution level
					if (rl == 0)
					{
						// Only the LL subband
						minb = 0;
						maxb = 1;
					}
					else
					{
						// Dyadic decomposition
						hpd = 1;
						
						// Adapt hpd to resolution level
						if (hpd > maxrl - rl)
						{
							hpd -= (maxrl - rl);
						}
						else
						{
							hpd = 1;
						}
						// Determine max and min subband index
						minb = 1 << ((hpd - 1) << 1); // minb = 4^(hpd-1)
						maxb = 1 << (hpd << 1); // maxb = 4^hpd
					}
					// Allocate array for subbands in resolution level
					exp[rl] = new int[maxb];
					nStep[rl] = new float[maxb];
					
					for (j = minb; j < maxb; j++)
					{
						tmp = ms.spqcd[rl][j] = ehs.ReadUInt16();
						exp[rl][j] = (tmp >> 11) & 0x1f;
						// NOTE: the formula below does not support more than 5
						// bits for the exponent, otherwise (-1<<exp) might
						// overflow (the - is used to be able to represent 2**31)
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						nStep[rl][j] = (- 1f - ((float) (tmp & 0x07ff)) / (1 << 11)) / (- 1 << exp[rl][j]);
					}
				} // end for rl
			} // end if (qType != SQCX_NO_QUANTIZATION)
			
			// Fill qsss, gbs
			if (mainh)
			{
				decSpec.qsss.setDefault(qParms);
				decSpec.gbs.setDefault(guardBits);
			}
			else
			{
				decSpec.qsss.setTileDef(tileIdx, qParms);
				decSpec.gbs.setTileDef(tileIdx, guardBits);
			}
			
			// Check marker length
			checkMarkerLength(ehs, "QCD marker");
		}
		
		/// <summary> Reads a QCC marker segment and realigns the codestream at the point
		/// where the next marker should be found. QCC is a functional marker
		/// segment that describes the quantization of one component.
		/// 
		/// </summary>
		/// <param name="ehs">The encoded stream.
		/// 
		/// </param>
		/// <param name="mainh">Flag indicating whether or not this marker segment is read
		/// from the main header.
		/// 
		/// </param>
		/// <param name="tileIdx">The index of the current tile
		/// 
		/// </param>
		/// <param name="tpIdx">Tile-part index
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoded header stream.
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readQCC(System.IO.BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
		{
			int cComp; // current component
			int tmp;
			StdDequantizerParams qParms;
			int[][] expC;
			float[][] nStepC = null;
			var ms = hi.NewQCC;
			
			// Lqcc (length of QCC field)
			ms.lqcc = ehs.ReadUInt16();
			
			// Cqcc
			if (nComp < 257)
			{
				cComp = ms.cqcc = ehs.ReadByte();
			}
			else
			{
				cComp = ms.cqcc = ehs.ReadUInt16();
			}
			if (cComp >= nComp)
			{
				throw new CorruptedCodestreamException("Invalid component " + "index in QCC marker");
			}
			
			// Sqcc (quantization style)
			ms.sqcc = ehs.ReadByte();
			var guardBits = ms.NumGuardBits;
			var qType = ms.QuantType;
			
			if (mainh)
			{
				hi.qccValue[$"main_c{cComp}"] = ms;
				// If main header is being read, set default for component in all
				// tiles
				switch (qType)
				{
					
					case Markers.SQCX_NO_QUANTIZATION: 
						decSpec.qts.setCompDef(cComp, "reversible");
						break;
					
					case Markers.SQCX_SCALAR_DERIVED: 
						decSpec.qts.setCompDef(cComp, "derived");
						break;
					
					case Markers.SQCX_SCALAR_EXPOUNDED: 
						decSpec.qts.setCompDef(cComp, "expounded");
						break;
					
					default: 
						throw new CorruptedCodestreamException("Unknown or " + "unsupported " + "quantization style " + "in Sqcd field, QCD " + "marker, main header");
					
				}
			}
			else
			{
				hi.qccValue[$"t{tileIdx}_c{cComp}"] = ms;
				// If tile header is being read, set value for component in
				// this tiles
				switch (qType)
				{
					
					case Markers.SQCX_NO_QUANTIZATION: 
						decSpec.qts.setTileCompVal(tileIdx, cComp, "reversible");
						break;
					
					case Markers.SQCX_SCALAR_DERIVED: 
						decSpec.qts.setTileCompVal(tileIdx, cComp, "derived");
						break;
					
					case Markers.SQCX_SCALAR_EXPOUNDED: 
						decSpec.qts.setTileCompVal(tileIdx, cComp, "expounded");
						break;
					
					default: 
						throw new CorruptedCodestreamException("Unknown or " + "unsupported " + "quantization style " + "in Sqcd field, QCD " + "marker, main header");
					
				}
			}
			
			// Decode all dequantizer params
			qParms = new StdDequantizerParams();
			
			if (qType == Markers.SQCX_NO_QUANTIZATION)
			{
				var maxrl = (mainh?((int) decSpec.dls.getCompDef(cComp)):((int) decSpec.dls.getTileCompVal(tileIdx, cComp)));
				int j, rl; // i removed
				int minb, maxb, hpd;
				
				expC = qParms.exp = new int[maxrl + 1][];
				var tmpArray = new int[maxrl + 1][];
				for (var i2 = 0; i2 < maxrl + 1; i2++)
				{
					tmpArray[i2] = new int[4];
				}
				ms.spqcc = tmpArray;
				
				for (rl = 0; rl <= maxrl; rl++)
				{
					// Loop on resolution levels
					// Find the number of subbands in the resolution level
					if (rl == 0)
					{
						// Only the LL subband
						minb = 0;
						maxb = 1;
					}
					else
					{
						// Dyadic decomposition
						hpd = 1;
						
						// Adapt hpd to resolution level
						if (hpd > maxrl - rl)
						{
							hpd -= (maxrl - rl);
						}
						else
						{
							hpd = 1;
						}
						// Determine max and min subband index
						minb = 1 << ((hpd - 1) << 1); // minb = 4^(hpd-1)
						maxb = 1 << (hpd << 1); // maxb = 4^hpd
					}
					// Allocate array for subbands in resolution level
					expC[rl] = new int[maxb];
					
					for (j = minb; j < maxb; j++)
					{
						tmp = ms.spqcc[rl][j] = ehs.ReadByte();
						expC[rl][j] = (tmp >> Markers.SQCX_EXP_SHIFT) & Markers.SQCX_EXP_MASK;
					}
				} // end for rl
			}
			else
			{
				var maxrl = (qType == Markers.SQCX_SCALAR_DERIVED)?0:(mainh?((int) decSpec.dls.getCompDef(cComp)):((int) decSpec.dls.getTileCompVal(tileIdx, cComp)));
				int j, rl; // i removed
				int minb, maxb, hpd;
				
				nStepC = qParms.nStep = new float[maxrl + 1][];
				expC = qParms.exp = new int[maxrl + 1][];
				var tmpArray2 = new int[maxrl + 1][];
				for (var i3 = 0; i3 < maxrl + 1; i3++)
				{
					tmpArray2[i3] = new int[4];
				}
				ms.spqcc = tmpArray2;
				
				for (rl = 0; rl <= maxrl; rl++)
				{
					// Loop on resolution levels
					// Find the number of subbands in the resolution level
					if (rl == 0)
					{
						// Only the LL subband
						minb = 0;
						maxb = 1;
					}
					else
					{
						// Dyadic decomposition
						hpd = 1;
						
						// Adapt hpd to resolution level
						if (hpd > maxrl - rl)
						{
							hpd -= (maxrl - rl);
						}
						else
						{
							hpd = 1;
						}
						// Determine max and min subband index
						minb = 1 << ((hpd - 1) << 1); // minb = 4^(hpd-1)
						maxb = 1 << (hpd << 1); // maxb = 4^hpd
					}
					// Allocate array for subbands in resolution level
					expC[rl] = new int[maxb];
					nStepC[rl] = new float[maxb];
					
					for (j = minb; j < maxb; j++)
					{
						tmp = ms.spqcc[rl][j] = ehs.ReadUInt16();
						expC[rl][j] = (tmp >> 11) & 0x1f;
						// NOTE: the formula below does not support more than 5
						// bits for the exponent, otherwise (-1<<exp) might
						// overflow (the - is used to be able to represent 2**31)
						//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
						nStepC[rl][j] = (- 1f - ((float) (tmp & 0x07ff)) / (1 << 11)) / (- 1 << expC[rl][j]);
					}
				} // end for rl
			} // end if (qType != SQCX_NO_QUANTIZATION)
			
			// Fill qsss, gbs
			if (mainh)
			{
				decSpec.qsss.setCompDef(cComp, qParms);
				decSpec.gbs.setCompDef(cComp, guardBits);
			}
			else
			{
				decSpec.qsss.setTileCompVal(tileIdx, cComp, qParms);
				decSpec.gbs.setTileCompVal(tileIdx, cComp, guardBits);
			}
			
			// Check marker length
			checkMarkerLength(ehs, "QCC marker");
		}
		
		/// <summary> Reads a COD marker segment and realigns the codestream where the next
		/// marker should be found. 
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <param name="mainh">Flag indicating whether or not this marker segment is read
		/// from the main header.
		/// 
		/// </param>
		/// <param name="tileIdx">The index of the current tile
		/// 
		/// </param>
		/// <param name="tpIdx">Tile-part index
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readCOD(System.IO.BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
		{
			int cstyle; // The block style
			SynWTFilter[] hfilters, vfilters;
			//int l;
			int[] cblk;
			string errMsg;
			//bool sopUsed = false;
			//bool ephUsed = false;
			var ms = hi.NewCOD;
			
			// Lcod (marker length)
			ms.lcod = ehs.ReadUInt16();
			
			// Scod (block style)
			// We only support wavelet transformed data
			cstyle = ms.scod = ehs.ReadByte();
			
			if ((cstyle & Markers.SCOX_PRECINCT_PARTITION) != 0)
			{
				precinctPartitionIsUsed = true;
				// Remove flag
				cstyle &= ~ (Markers.SCOX_PRECINCT_PARTITION);
			}
			else
			{
				precinctPartitionIsUsed = false;
			}
			
			// SOP markers
			if (mainh)
			{
				hi.codValue["main"] = ms;
				
				if ((cstyle & Markers.SCOX_USE_SOP) != 0)
				{
					// SOP markers are used
					decSpec.sops.setDefault("true".ToUpper().Equals("TRUE"));
					//sopUsed = true;
					// Remove flag
					cstyle &= ~ (Markers.SCOX_USE_SOP);
				}
				else
				{
					// SOP markers are not used
					decSpec.sops.setDefault("false".ToUpper().Equals("TRUE"));
				}
			}
			else
			{
				hi.codValue[$"t{tileIdx}"] = ms;
				
				if ((cstyle & Markers.SCOX_USE_SOP) != 0)
				{
					// SOP markers are used
					decSpec.sops.setTileDef(tileIdx, "true".ToUpper().Equals("TRUE"));
					//sopUsed = true;
					// Remove flag
					cstyle &= ~ (Markers.SCOX_USE_SOP);
				}
				else
				{
					// SOP markers are not used
					decSpec.sops.setTileDef(tileIdx, "false".ToUpper().Equals("TRUE"));
				}
			}
			
			// EPH markers
			if (mainh)
			{
				if ((cstyle & Markers.SCOX_USE_EPH) != 0)
				{
					// EPH markers are used
					decSpec.ephs.setDefault("true".ToUpper().Equals("TRUE"));
					//ephUsed = true;
					// Remove flag
					cstyle &= ~ (Markers.SCOX_USE_EPH);
				}
				else
				{
					// EPH markers are not used
					decSpec.ephs.setDefault("false".ToUpper().Equals("TRUE"));
				}
			}
			else
			{
				if ((cstyle & Markers.SCOX_USE_EPH) != 0)
				{
					// EPH markers are used
					decSpec.ephs.setTileDef(tileIdx, "true".ToUpper().Equals("TRUE"));
					//ephUsed = true;
					// Remove flag
					cstyle &= ~ (Markers.SCOX_USE_EPH);
				}
				else
				{
					// EPH markers are not used
					decSpec.ephs.setTileDef(tileIdx, "false".ToUpper().Equals("TRUE"));
				}
			}
			
			// Code-block partition origin
			if ((cstyle & (Markers.SCOX_HOR_CB_PART | Markers.SCOX_VER_CB_PART)) != 0)
			{
				FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING, "Code-block partition origin " + "different from (0,0). This is defined in JPEG 2000" + " part 2 and may not be supported by all JPEG " + "2000 decoders.");
			}
			if ((cstyle & Markers.SCOX_HOR_CB_PART) != 0)
			{
				if (cb0x != - 1 && cb0x == 0)
				{
					throw new ArgumentException("Code-block partition " + "origin redefined in new" + " COD marker segment. Not" + " supported by JJ2000");
				}
				cb0x = 1;
				cstyle &= ~ (Markers.SCOX_HOR_CB_PART);
			}
			else
			{
				if (cb0x != - 1 && cb0x == 1)
				{
					throw new ArgumentException("Code-block partition " + "origin redefined in new" + " COD marker segment. Not" + " supported by JJ2000");
				}
				cb0x = 0;
			}
			if ((cstyle & Markers.SCOX_VER_CB_PART) != 0)
			{
				if (cb0y != - 1 && cb0y == 0)
				{
					throw new ArgumentException("Code-block partition " + "origin redefined in new" + " COD marker segment. Not" + " supported by JJ2000");
				}
				cb0y = 1;
				cstyle &= ~ (Markers.SCOX_VER_CB_PART);
			}
			else
			{
				if (cb0y != - 1 && cb0y == 1)
				{
					throw new ArgumentException("Code-block partition " + "origin redefined in new" + " COD marker segment. Not" + " supported by JJ2000");
				}
				cb0y = 0;
			}
			
			// SGcod
			// Read the progressive order
			ms.sgcod_po = ehs.ReadByte();
			
			// Read the number of layers
			ms.sgcod_nl = ehs.ReadUInt16();
			if (ms.sgcod_nl <= 0 || ms.sgcod_nl > 65535)
			{
				throw new CorruptedCodestreamException("Number of layers out of " + "range: 1--65535");
			}
			
			// Multiple component transform
			ms.sgcod_mct = ehs.ReadByte();
			
			// SPcod
			// decomposition levels
			var mrl = ms.spcod_ndl = ehs.ReadByte();
			if (mrl > 32)
			{
				throw new CorruptedCodestreamException("Number of decomposition " + "levels out of range: " + "0--32");
			}
			
			// Read the code-blocks dimensions
			cblk = new int[2];
			ms.spcod_cw = ehs.ReadByte();
			cblk[0] = 1 << (ms.spcod_cw + 2);
			if (cblk[0] < StdEntropyCoderOptions.MIN_CB_DIM || cblk[0] > StdEntropyCoderOptions.MAX_CB_DIM)
			{
				errMsg = "Non-valid code-block width in SPcod field, " + "COD marker";
				throw new CorruptedCodestreamException(errMsg);
			}
			ms.spcod_ch = ehs.ReadByte();
			cblk[1] = 1 << (ms.spcod_ch + 2);
			if (cblk[1] < StdEntropyCoderOptions.MIN_CB_DIM || cblk[1] > StdEntropyCoderOptions.MAX_CB_DIM)
			{
				errMsg = "Non-valid code-block height in SPcod field, " + "COD marker";
				throw new CorruptedCodestreamException(errMsg);
			}
			if ((cblk[0] * cblk[1]) > StdEntropyCoderOptions.MAX_CB_AREA)
			{
				errMsg = "Non-valid code-block area in SPcod field, " + "COD marker";
				throw new CorruptedCodestreamException(errMsg);
			}
			if (mainh)
			{
				decSpec.cblks.setDefault(cblk);
			}
			else
			{
				decSpec.cblks.setTileDef(tileIdx, cblk);
			}
			
			// Style of the code-block coding passes
			var ecOptions = ms.spcod_cs = ehs.ReadByte();
			if ((ecOptions & ~ (StdEntropyCoderOptions.OPT_BYPASS | StdEntropyCoderOptions.OPT_RESET_MQ | StdEntropyCoderOptions.OPT_TERM_PASS | StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL | StdEntropyCoderOptions.OPT_PRED_TERM | StdEntropyCoderOptions.OPT_SEG_SYMBOLS)) != 0)
			{
				throw new CorruptedCodestreamException(
					$"Unknown \"code-block style\" in SPcod field, COD marker: 0x{Convert.ToString(ecOptions, 16)}");
			}
			
			// Read wavelet filter for tile or image
			hfilters = new SynWTFilter[1];
			vfilters = new SynWTFilter[1];
			hfilters[0] = readFilter(ehs, ms.spcod_t);
			vfilters[0] = hfilters[0];
			
			// Fill the filter spec
			// If this is the main header, set the default value, if it is the
			// tile header, set default for this tile 
			var hvfilters = new SynWTFilter[2][];
			hvfilters[0] = hfilters;
			hvfilters[1] = vfilters;
			
			// Get precinct partition sizes
			var v = new List<int>[2];
			v[0] = new List<int>(10);
			v[1] = new List<int>(10);
			var val = Markers.PRECINCT_PARTITION_DEF_SIZE;
			if (!precinctPartitionIsUsed)
			{
				int w, h;
				w = 1 << (val & 0x000F);
				v[0].Add(w);
				h = 1 << (((val & 0x00F0) >> 4));
				v[1].Add(h);
			}
			else
			{
				ms.spcod_ps = new int[mrl + 1];
				for (var rl = mrl; rl >= 0; rl--)
				{
					int w, h;
					val = ms.spcod_ps[mrl - rl] = ehs.ReadByte();
					w = 1 << (val & 0x000F);
					v[0].Insert(0, w);
					h = 1 << (((val & 0x00F0) >> 4));
					v[1].Insert(0, h);
				}
			}
			if (mainh)
			{
				decSpec.pss.setDefault(v);
			}
			else
			{
				decSpec.pss.setTileDef(tileIdx, v);
			}
			precinctPartitionIsUsed = true;
			
			// Check marker length
			checkMarkerLength(ehs, "COD marker");
			
			// Store specifications in decSpec
			if (mainh)
			{
				decSpec.wfs.setDefault(hvfilters);
				decSpec.dls.setDefault(mrl);
				decSpec.ecopts.setDefault(ecOptions);
				decSpec.cts.setDefault(ms.sgcod_mct);
				decSpec.nls.setDefault(ms.sgcod_nl);
				decSpec.pos.setDefault(ms.sgcod_po);
			}
			else
			{
				decSpec.wfs.setTileDef(tileIdx, hvfilters);
				decSpec.dls.setTileDef(tileIdx, mrl);
				decSpec.ecopts.setTileDef(tileIdx, ecOptions);
				decSpec.cts.setTileDef(tileIdx, ms.sgcod_mct);
				decSpec.nls.setTileDef(tileIdx, ms.sgcod_nl);
				decSpec.pos.setTileDef(tileIdx, ms.sgcod_po);
			}
		}
		
		/// <summary> Reads the COC marker segment and realigns the codestream where the next
		/// marker should be found.
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <param name="mainh">Flag indicating whether or not this marker segment is read
		/// from the main header.
		/// 
		/// </param>
		/// <param name="tileIdx">The index of the current tile
		/// 
		/// </param>
		/// <param name="tpIdx">Tile-part index
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readCOC(System.IO.BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
		{
			int cComp; // current component
			SynWTFilter[] hfilters, vfilters;
			//int tmp, l;
			int ecOptions;
			int[] cblk;
			string errMsg;
			var ms = hi.NewCOC;
			
			// Lcoc (marker length)
			ms.lcoc = ehs.ReadUInt16();
			
			// Ccoc
			if (nComp < 257)
			{
				cComp = ms.ccoc = ehs.ReadByte();
			}
			else
			{
				cComp = ms.ccoc = ehs.ReadUInt16();
			}
			if (cComp >= nComp)
			{
				throw new CorruptedCodestreamException("Invalid component index " + "in QCC marker");
			}
			
			// Scoc (block style)
			var cstyle = ms.scoc = ehs.ReadByte();
			if ((cstyle & Markers.SCOX_PRECINCT_PARTITION) != 0)
			{
				precinctPartitionIsUsed = true;
				// Remove flag
				cstyle &= ~ (Markers.SCOX_PRECINCT_PARTITION);
			}
			else
			{
				precinctPartitionIsUsed = false;
			}
			
			// SPcoc
			
			// decomposition levels
			var mrl = ms.spcoc_ndl = ehs.ReadByte();
			
			// Read the code-blocks dimensions
			cblk = new int[2];
			ms.spcoc_cw = ehs.ReadByte();
			cblk[0] = 1 << (ms.spcoc_cw + 2);
			if (cblk[0] < StdEntropyCoderOptions.MIN_CB_DIM || cblk[0] > StdEntropyCoderOptions.MAX_CB_DIM)
			{
				errMsg = "Non-valid code-block width in SPcod field, " + "COC marker";
				throw new CorruptedCodestreamException(errMsg);
			}
			ms.spcoc_ch = ehs.ReadByte();
			cblk[1] = 1 << (ms.spcoc_ch + 2);
			if (cblk[1] < StdEntropyCoderOptions.MIN_CB_DIM || cblk[1] > StdEntropyCoderOptions.MAX_CB_DIM)
			{
				errMsg = "Non-valid code-block height in SPcod field, " + "COC marker";
				throw new CorruptedCodestreamException(errMsg);
			}
			if ((cblk[0] * cblk[1]) > StdEntropyCoderOptions.MAX_CB_AREA)
			{
				errMsg = "Non-valid code-block area in SPcod field, " + "COC marker";
				throw new CorruptedCodestreamException(errMsg);
			}
			if (mainh)
			{
				decSpec.cblks.setCompDef(cComp, cblk);
			}
			else
			{
				decSpec.cblks.setTileCompVal(tileIdx, cComp, cblk);
			}
			
			// Read entropy block mode options
			// NOTE: currently OPT_SEG_SYMBOLS is not included here
			ecOptions = ms.spcoc_cs = ehs.ReadByte();
			if ((ecOptions & ~ (StdEntropyCoderOptions.OPT_BYPASS | StdEntropyCoderOptions.OPT_RESET_MQ | StdEntropyCoderOptions.OPT_TERM_PASS | StdEntropyCoderOptions.OPT_VERT_STR_CAUSAL | StdEntropyCoderOptions.OPT_PRED_TERM | StdEntropyCoderOptions.OPT_SEG_SYMBOLS)) != 0)
			{
				throw new CorruptedCodestreamException(
					$"Unknown \"code-block context\" in SPcoc field, COC marker: 0x{Convert.ToString(ecOptions, 16)}");
			}
			
			// Read wavelet filter for tile or image
			hfilters = new SynWTFilter[1];
			vfilters = new SynWTFilter[1];
			hfilters[0] = readFilter(ehs, ms.spcoc_t);
			vfilters[0] = hfilters[0];
			
			// Fill the filter spec
			// If this is the main header, set the default value, if it is the
			// tile header, set default for this tile 
			var hvfilters = new SynWTFilter[2][];
			hvfilters[0] = hfilters;
			hvfilters[1] = vfilters;
			
			// Get precinct partition sizes
			var v = new List<int>[2];
			v[0] = new List<int>(10);
			v[1] = new List<int>(10);
			var val = Markers.PRECINCT_PARTITION_DEF_SIZE;
			if (!precinctPartitionIsUsed)
			{
				int w, h;
				w = 1 << (val & 0x000F);
				v[0].Add(w);
				h = 1 << (((val & 0x00F0) >> 4));
				v[1].Add(h);
			}
			else
			{
				ms.spcoc_ps = new int[mrl + 1];
				for (var rl = mrl; rl >= 0; rl--)
				{
					int w, h;
					val = ms.spcoc_ps[rl] = ehs.ReadByte();
					w = 1 << (val & 0x000F);
					v[0].Insert(0, w);
					h = 1 << (((val & 0x00F0) >> 4));
					v[1].Insert(0, h);
				}
			}
			if (mainh)
			{
				decSpec.pss.setCompDef(cComp, v);
			}
			else
			{
				decSpec.pss.setTileCompVal(tileIdx, cComp, v);
			}
			precinctPartitionIsUsed = true;
			
			// Check marker length
			checkMarkerLength(ehs, "COD marker");
			
			if (mainh)
			{
				hi.cocValue[$"main_c{cComp}"] = ms;
				decSpec.wfs.setCompDef(cComp, hvfilters);
				decSpec.dls.setCompDef(cComp, mrl);
				decSpec.ecopts.setCompDef(cComp, ecOptions);
			}
			else
			{
				hi.cocValue[$"t{tileIdx}_c{cComp}"] = ms;
				decSpec.wfs.setTileCompVal(tileIdx, cComp, hvfilters);
				decSpec.dls.setTileCompVal(tileIdx, cComp, mrl);
				decSpec.ecopts.setTileCompVal(tileIdx, cComp, ecOptions);
			}
		}
		
		/// <summary> Reads the POC marker segment and realigns the codestream where the next
		/// marker should be found.
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <param name="mainh">Flag indicating whether or not this marker segment is read
		/// from the main header.
		/// 
		/// </param>
		/// <param name="t">The index of the current tile
		/// 
		/// </param>
		/// <param name="tpIdx">Tile-part index
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readPOC(System.IO.BinaryReader ehs, bool mainh, int t, int tpIdx)
		{
			
			var useShort = nComp >= 256;
			int tmp;
			var nOldChg = 0;
			HeaderInfo.POC ms;
			if (mainh || hi.pocValue[$"t{t}"] == null)
			{
				ms = hi.NewPOC;
			}
			else
			{
				ms = hi.pocValue[$"t{t}"];
				nOldChg = ms.rspoc.Length;
			}
			
			// Lpoc
			ms.lpoc = ehs.ReadUInt16();
			
			// Compute the number of new progression changes
			// newChg = (lpoc - Lpoc(2)) / (RSpoc(1) + CSpoc(2) +
			//  LYEpoc(2) + REpoc(1) + CEpoc(2) + Ppoc (1) )
			var newChg = (ms.lpoc - 2) / (5 + (useShort?4:2));
			var ntotChg = nOldChg + newChg;
			
			int[][] change;
			if (nOldChg != 0)
			{
				// Creates new arrays
				var tmpArray = new int[ntotChg][];
				for (var i = 0; i < ntotChg; i++)
				{
					tmpArray[i] = new int[6];
				}
				change = tmpArray;
				var tmprspoc = new int[ntotChg];
				var tmpcspoc = new int[ntotChg];
				var tmplyepoc = new int[ntotChg];
				var tmprepoc = new int[ntotChg];
				var tmpcepoc = new int[ntotChg];
				var tmpppoc = new int[ntotChg];
				
				// Copy old values
				var prevChg = (int[][]) decSpec.pcs.getTileDef(t);
				for (var chg = 0; chg < nOldChg; chg++)
				{
					change[chg] = prevChg[chg];
					tmprspoc[chg] = ms.rspoc[chg];
					tmpcspoc[chg] = ms.cspoc[chg];
					tmplyepoc[chg] = ms.lyepoc[chg];
					tmprepoc[chg] = ms.repoc[chg];
					tmpcepoc[chg] = ms.cepoc[chg];
					tmpppoc[chg] = ms.ppoc[chg];
				}
				ms.rspoc = tmprspoc;
				ms.cspoc = tmpcspoc;
				ms.lyepoc = tmplyepoc;
				ms.repoc = tmprepoc;
				ms.cepoc = tmpcepoc;
				ms.ppoc = tmpppoc;
			}
			else
			{
				var tmpArray2 = new int[newChg][];
				for (var i2 = 0; i2 < newChg; i2++)
				{
					tmpArray2[i2] = new int[6];
				}
				change = tmpArray2;
				ms.rspoc = new int[newChg];
				ms.cspoc = new int[newChg];
				ms.lyepoc = new int[newChg];
				ms.repoc = new int[newChg];
				ms.cepoc = new int[newChg];
				ms.ppoc = new int[newChg];
			}
			
			for (var chg = nOldChg; chg < ntotChg; chg++)
			{
				// RSpoc
				change[chg][0] = ms.rspoc[chg] = ehs.ReadByte();
				
				// CSpoc
				if (useShort)
				{
					change[chg][1] = ms.cspoc[chg] = ehs.ReadUInt16();
				}
				else
				{
					change[chg][1] = ms.cspoc[chg] = ehs.ReadByte();
				}
				
				// LYEpoc
				change[chg][2] = ms.lyepoc[chg] = ehs.ReadUInt16();
				if (change[chg][2] < 1)
				{
					throw new CorruptedCodestreamException(
						$"LYEpoc value must be greater than 1 in POC marker segment of tile {t}, tile-part {tpIdx}");
				}
				
				// REpoc
				change[chg][3] = ms.repoc[chg] = ehs.ReadByte();
				if (change[chg][3] <= change[chg][0])
				{
					throw new CorruptedCodestreamException(
						$"REpoc value must be greater than RSpoc in POC marker segment of tile {t}, tile-part {tpIdx}");
				}
				
				// CEpoc
				if (useShort)
				{
					change[chg][4] = ms.cepoc[chg] = ehs.ReadUInt16();
				}
				else
				{
					tmp = ms.cepoc[chg] = ehs.ReadByte();
					if (tmp == 0)
					{
						change[chg][4] = 0;
					}
					else
					{
						change[chg][4] = tmp;
					}
				}
				if (change[chg][4] <= change[chg][1])
				{
					throw new CorruptedCodestreamException(
						$"CEpoc value must be greater than CSpoc in POC marker segment of tile {t}, tile-part {tpIdx}");
				}
				
				// Ppoc
				change[chg][5] = ms.ppoc[chg] = ehs.ReadByte();
			}
			
			// Check marker length
			checkMarkerLength(ehs, "POC marker");
			
			// Register specifications
			if (mainh)
			{
				hi.pocValue["main"] = ms;
				decSpec.pcs.setDefault(change);
			}
			else
			{
				hi.pocValue[$"t{t}"] = ms;
				decSpec.pcs.setTileDef(t, change);
			}
		}
		
		/// <summary> Reads TLM marker segment and realigns the codestream where the next
		/// marker should be found. Informations stored in these fields are
		/// currently NOT taken into account.
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readTLM(System.IO.BinaryReader ehs)
		{
			int length;
			
			length = ehs.ReadUInt16();
			//Ignore all informations contained
			System.IO.BinaryReader temp_BinaryReader;
			long temp_Int64;
			temp_BinaryReader = ehs;
			temp_Int64 = temp_BinaryReader.BaseStream.Position;
			temp_Int64 = temp_BinaryReader.BaseStream.Seek(length - 2, System.IO.SeekOrigin.Current) - temp_Int64;
            // CONVERSION PROBLEM?
			var generatedAux = (int)temp_Int64;
			
			FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.INFO, "Skipping unsupported TLM marker");
		}
		
		/// <summary> Reads PLM marker segment and realigns the codestream where the next
		/// marker should be found. Informations stored in these fields are
		/// currently not taken into account.
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readPLM(System.IO.BinaryReader ehs)
		{
			int length;
			
			length = ehs.ReadUInt16();
			//Ignore all informations contained
			System.IO.BinaryReader temp_BinaryReader;
			long temp_Int64;
			temp_BinaryReader = ehs;
			temp_Int64 = temp_BinaryReader.BaseStream.Position;
			temp_Int64 = temp_BinaryReader.BaseStream.Seek(length - 2, System.IO.SeekOrigin.Current) - temp_Int64;
            // CONVERSION PROBLEM?
			var generatedAux = (int)temp_Int64;
			
			FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.INFO, "Skipping unsupported PLM marker");
		}
		
		/// <summary> Reads the PLT fields and realigns the codestream where the next marker
		/// should be found. Informations stored in these fields are currently NOT
		/// taken into account.
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readPLTFields(System.IO.BinaryReader ehs)
		{
			int length;
			
			length = ehs.ReadUInt16();
			//Ignore all informations contained
			System.IO.BinaryReader temp_BinaryReader;
			long temp_Int64;
			temp_BinaryReader = ehs;
			temp_Int64 = temp_BinaryReader.BaseStream.Position;
			temp_Int64 = temp_BinaryReader.BaseStream.Seek(length - 2, System.IO.SeekOrigin.Current) - temp_Int64;
            // CONVERSION PROBLEM?
			var generatedAux = (int)temp_Int64;
			
			FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.INFO, "Skipping unsupported PLT marker");
		}
		
		/// <summary> Reads the RGN marker segment of the codestream header.
		/// 
		/// May be used in tile or main header. If used in main header, it
		/// refers to the maxshift value of a component in all tiles. When used in
		/// tile header, only the particular tile-component is affected.
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <param name="mainh">Flag indicating whether or not this marker segment is read
		/// from the main header.
		/// 
		/// </param>
		/// <param name="tileIdx">The index of the current tile
		/// 
		/// </param>
		/// <param name="tpIdx">Tile-part index
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readRGN(System.IO.BinaryReader ehs, bool mainh, int tileIdx, int tpIdx)
		{
			int comp; // ROI component
			//int i; // loop variable
			//int tempComp; // Component for
			var ms = hi.NewRGN;
			
			// Lrgn (marker length)
			ms.lrgn = ehs.ReadUInt16();
			
			// Read component
			ms.crgn = comp = (nComp < 257)?ehs.ReadByte():ehs.ReadUInt16();
			if (comp >= nComp)
			{
				throw new CorruptedCodestreamException($"Invalid component index in RGN marker{comp}");
			}
			
			// Read type of RGN.(Srgn) 
			ms.srgn = ehs.ReadByte();
			
			// Check that we can handle it.
			if (ms.srgn != Markers.SRGN_IMPLICIT)
				throw new CorruptedCodestreamException("Unknown or unsupported " + "Srgn parameter in ROI " + "marker");
			
			if (decSpec.rois == null)
			{
				// No maxshift spec defined
				// Create needed ModuleSpec
				decSpec.rois = new MaxShiftSpec(nTiles, nComp, ModuleSpec.SPEC_TYPE_TILE_COMP);
			}
			
			// SPrgn
			ms.sprgn = ehs.ReadByte();
			
			if (mainh)
			{
				hi.rgnValue[$"main_c{comp}"] = ms;
				decSpec.rois.setCompDef(comp, ms.sprgn);
			}
			else
			{
				hi.rgnValue[$"t{tileIdx}_c{comp}"] = ms;
				decSpec.rois.setTileCompVal(tileIdx, comp, ms.sprgn);
			}
			
			// Check marker length
			checkMarkerLength(ehs, "RGN marker");
		}
		
		/// <summary> Reads the PPM marker segment of the main header.
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readPPM(System.IO.BinaryReader ehs)
		{
			int curMarkSegLen;
            int indx; // i, len, off removed
			int remSegLen;
			//byte[] b;
			
			// If first time readPPM method is called allocate arrays for packed
			// packet data
			if (pPMMarkerData == null)
			{
				pPMMarkerData = new byte[nPPMMarkSeg][];
				tileOfTileParts = new List<int>(10);
				decSpec.pphs.setDefault(true);
			}
			
			// Lppm (marker length)
			curMarkSegLen = ehs.ReadUInt16();
			remSegLen = curMarkSegLen - 3;
			
			// Zppm (index of PPM marker)
			indx = ehs.ReadByte();
			
			// Read Nppm and Ippm data 
			pPMMarkerData[indx] = new byte[remSegLen];
            ehs.BaseStream.Read(pPMMarkerData[indx], 0, remSegLen); //SupportClass.ReadInput(ehs.BaseStream, pPMMarkerData[indx], 0, remSegLen);
			
			// Check marker length
			checkMarkerLength(ehs, "PPM marker");
		}
		
		/// <summary> Reads the PPT marker segment of the main header.
		/// 
		/// </summary>
		/// <param name="ehs">The encoder header stream.
		/// 
		/// </param>
		/// <param name="tile">The tile to which the current tile part belongs
		/// 
		/// </param>
		/// <param name="tpIdx">Tile-part index
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
		private void  readPPT(System.IO.BinaryReader ehs, int tile, int tpIdx)
		{
			int curMarkSegLen;
			int indx; // len = 0; removed
			byte[] temp;
			
			if (tilePartPkdPktHeaders == null)
			{
				tilePartPkdPktHeaders = new byte[nTiles][][][];
			}
			
			if (tilePartPkdPktHeaders[tile] == null)
			{
				tilePartPkdPktHeaders[tile] = new byte[nTileParts[tile]][][];
			}
			
			if (tilePartPkdPktHeaders[tile][tpIdx] == null)
			{
				tilePartPkdPktHeaders[tile][tpIdx] = new byte[nPPTMarkSeg[tile][tpIdx]][];
			}
			
			// Lppt (marker length)
			curMarkSegLen = ehs.ReadUInt16();
			
			// Zppt (index of PPT marker)
			indx = ehs.ReadByte();
			
			// Ippt (packed packet headers)
			temp = new byte[curMarkSegLen - 3];
            ehs.BaseStream.Read(temp, 0, temp.Length); //SupportClass.ReadInput(ehs.BaseStream, temp, 0, temp.Length);
			tilePartPkdPktHeaders[tile][tpIdx][indx] = temp;
			
			// Check marker length
			checkMarkerLength(ehs, "PPT marker");
			
			decSpec.pphs.setTileDef(tile, true);
		}
		
		/// <summary> This method extract a marker segment from the main header and stores it
		/// into a byte buffer for the second pass. The marker segment is first
		/// identified. Then its flag is activated. Finally, its content is
		/// buffered into a byte array stored in an hashTable.
		/// 
		/// If the marker is not recognized, it prints a warning and skips it
		/// according to its length.
		/// 
		/// SIZ marker segment shall be the first encountered marker segment.
		/// 
		/// </summary>
		/// <param name="marker">The marker segment to process
		/// 
		/// </param>
		/// <param name="ehs">The encoded header stream
		/// 
		/// </param>
		private void  extractMainMarkSeg(short marker, RandomAccessIO ehs)
		{
			if (nfMarkSeg == 0)
			{
				// First non-delimiting marker of the header
				// JPEG 2000 part 1 specify that it must be SIZ
				if (marker != Markers.SIZ)
				{
					throw new CorruptedCodestreamException(
						$"First marker after SOC must be SIZ {Convert.ToString(marker, 16)}");
				}
			}
			
			var htKey = ""; // Name used as a key for the hash-table
			if (ht == null)
			{
				ht = new Dictionary<string, byte[]>();
			}
			
			switch (marker)
			{
				
				case Markers.SIZ: 
					if ((nfMarkSeg & SIZ_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one SIZ marker " + "segment found in main " + "header");
					}
					nfMarkSeg |= SIZ_FOUND;
					htKey = "SIZ";
					break;
				
				case Markers.SOD: 
					throw new CorruptedCodestreamException("SOD found in main header");
				
				case Markers.EOC: 
					throw new CorruptedCodestreamException("EOC found in main header");
				
				case Markers.SOT: 
					if ((nfMarkSeg & SOT_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one SOT " + "marker " + "found right after " + "main " + "or tile header");
					}
					nfMarkSeg |= SOT_FOUND;
					return ;
				
				case Markers.COD: 
					if ((nfMarkSeg & COD_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one COD " + "marker " + "found in main header");
					}
					nfMarkSeg |= COD_FOUND;
					htKey = "COD";
					break;
				
				case Markers.COC: 
					nfMarkSeg |= COC_FOUND;
					htKey = $"COC{(nCOCMarkSeg++)}";
					break;
				
				case Markers.QCD: 
					if ((nfMarkSeg & QCD_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one QCD " + "marker " + "found in main header");
					}
					nfMarkSeg |= QCD_FOUND;
					htKey = "QCD";
					break;
				
				case Markers.QCC: 
					nfMarkSeg |= QCC_FOUND;
					htKey = $"QCC{(nQCCMarkSeg++)}";
					break;
				
				case Markers.RGN: 
					nfMarkSeg |= RGN_FOUND;
					htKey = $"RGN{(nRGNMarkSeg++)}";
					break;
				
				case Markers.COM: 
					nfMarkSeg |= COM_FOUND;
					htKey = $"COM{(nCOMMarkSeg++)}";
					break;
				
				case Markers.CRG: 
					if ((nfMarkSeg & CRG_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one CRG " + "marker " + "found in main header");
					}
					nfMarkSeg |= CRG_FOUND;
					htKey = "CRG";
					break;
				
				case Markers.PPM: 
					nfMarkSeg |= PPM_FOUND;
					htKey = $"PPM{(nPPMMarkSeg++)}";
					break;
				
				case Markers.TLM: 
					if ((nfMarkSeg & TLM_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one TLM " + "marker " + "found in main header");
					}
					nfMarkSeg |= TLM_FOUND;
					break;
				
				case Markers.PLM: 
					if ((nfMarkSeg & PLM_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one PLM " + "marker " + "found in main header");
					}
					FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING, "PLM marker segment found but " + "not used by by JJ2000 decoder.");
					nfMarkSeg |= PLM_FOUND;
					htKey = "PLM";
					break;
				
				case Markers.POC: 
					if ((nfMarkSeg & POC_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one POC " + "marker segment found " + "in main header");
					}
					nfMarkSeg |= POC_FOUND;
					htKey = "POC";
					break;
				
				case Markers.PLT: 
					throw new CorruptedCodestreamException("PLT found in main header");
				
				case Markers.PPT: 
					throw new CorruptedCodestreamException("PPT found in main header");
				
				default: 
					htKey = "UNKNOWN";
					FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING,
						$"Non recognized marker segment (0x{Convert.ToString(marker, 16)}) in main header!");
					break;
				
			}
			
			if (marker < unchecked((short)0xffffff30) || marker > unchecked((short)0xffffff3f))
			{
				// Read marker segment length and create corresponding byte buffer
				var markSegLen = ehs.readUnsignedShort();
				var buf = new byte[markSegLen];
				
				// Copy data (after re-insertion of the marker segment length);
				buf[0] = (byte) ((markSegLen >> 8) & 0xFF);
				buf[1] = (byte) (markSegLen & 0xFF);
				ehs.readFully(buf, 2, markSegLen - 2);
				
				if (!htKey.Equals("UNKNOWN"))
				{
					// Store array in hashTable
					ht[htKey] = buf;
				}
			}
		}
		
		/// <summary> This method extracts a marker segment in a tile-part header and stores
		/// it into a byte buffer for the second pass. The marker is first
		/// recognized, then its flag is activated and, finally, its content is
		/// buffered in an element of byte arrays accessible thanks to a hashTable.
		/// If a marker segment is not recognized, it prints a warning and skip it
		/// according to its length.
		/// 
		/// </summary>
		/// <param name="marker">The marker to process
		/// 
		/// </param>
		/// <param name="ehs">The encoded header stream
		/// 
		/// </param>
		/// <param name="tileIdx">The index of the current tile
		/// 
		/// </param>
		/// <param name="tilePartIdx">The index of the current tile part
		/// 
		/// </param>
		public virtual void  extractTilePartMarkSeg(short marker, RandomAccessIO ehs, int tileIdx, int tilePartIdx)
		{
			
			var htKey = ""; // Name used as a hash-table key
			if (ht == null)
			{
				ht = new Dictionary<string, byte[]>();
			}
			
			switch (marker)
			{
				
				case Markers.SOT: 
					throw new CorruptedCodestreamException("Second SOT marker " + "segment found in tile-" + "part header");
				
				case Markers.SIZ: 
					throw new CorruptedCodestreamException("SIZ found in tile-part" + " header");
				
				case Markers.EOC: 
					throw new CorruptedCodestreamException("EOC found in tile-part" + " header");
				
				case Markers.TLM: 
					throw new CorruptedCodestreamException("TLM found in tile-part" + " header");
				
				case Markers.PLM: 
					throw new CorruptedCodestreamException("PLM found in tile-part" + " header");
				
				case Markers.PPM: 
					throw new CorruptedCodestreamException("PPM found in tile-part" + " header");
				
				case Markers.COD: 
					if ((nfMarkSeg & COD_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one COD " + "marker " + "found in tile-part" + " header");
					}
					nfMarkSeg |= COD_FOUND;
					htKey = "COD";
					break;
				
				case Markers.COC: 
					nfMarkSeg |= COC_FOUND;
					htKey = $"COC{(nCOCMarkSeg++)}";
					break;
				
				case Markers.QCD: 
					if ((nfMarkSeg & QCD_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("More than one QCD " + "marker " + "found in tile-part" + " header");
					}
					nfMarkSeg |= QCD_FOUND;
					htKey = "QCD";
					break;
				
				case Markers.QCC: 
					nfMarkSeg |= QCC_FOUND;
					htKey = $"QCC{(nQCCMarkSeg++)}";
					break;
				
				case Markers.RGN: 
					nfMarkSeg |= RGN_FOUND;
					htKey = $"RGN{(nRGNMarkSeg++)}";
					break;
				
				case Markers.COM: 
					nfMarkSeg |= COM_FOUND;
					htKey = $"COM{(nCOMMarkSeg++)}";
					break;
				
				case Markers.CRG: 
					throw new CorruptedCodestreamException("CRG marker found in " + "tile-part header");
				
				case Markers.PPT: 
					nfMarkSeg |= PPT_FOUND;
					if (nPPTMarkSeg == null)
					{
						nPPTMarkSeg = new int[nTiles][];
					}
					if (nPPTMarkSeg[tileIdx] == null)
					{
						nPPTMarkSeg[tileIdx] = new int[nTileParts[tileIdx]];
					}
					htKey = $"PPT{(nPPTMarkSeg[tileIdx][tilePartIdx]++)}";
					break;
				
				case Markers.SOD: 
					nfMarkSeg |= SOD_FOUND;
					return ;
				
				case Markers.POC: 
					if ((nfMarkSeg & POC_FOUND) != 0)
						throw new CorruptedCodestreamException("More than one POC " + "marker segment found " + "in tile-part" + " header");
					nfMarkSeg |= POC_FOUND;
					htKey = "POC";
					break;
				
				case Markers.PLT: 
					if ((nfMarkSeg & PLM_FOUND) != 0)
					{
						throw new CorruptedCodestreamException("PLT marker found even" + "though PLM marker " + "found in main header");
					}
					FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING, "PLT marker segment found but " + "not used by JJ2000 decoder.");
					htKey = "UNKNOWN";
					break;
				
				default: 
					htKey = "UNKNOWN";
					FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING,
						$"Non recognized marker segment (0x{Convert.ToString(marker, 16)}) in tile-part header of tile {tileIdx} !");
					break;
				
			}
			
			// Read marker segment length and create corresponding byte buffer
			var markSegLen = ehs.readUnsignedShort();
			var buf = new byte[markSegLen];
			
			// Copy data (after re-insertion of marker segment length);
			buf[0] = (byte) ((markSegLen >> 8) & 0xFF);
			buf[1] = (byte) (markSegLen & 0xFF);
			ehs.readFully(buf, 2, markSegLen - 2);
			
			if (!htKey.Equals("UNKNOWN"))
			{
				// Store array in hashTable
				ht[htKey] = buf;
			}
		}
		
		/// <summary> Retrieves and reads all marker segments found in the main header during
		/// the first pass.
		/// 
		/// </summary>
		private void  readFoundMainMarkSeg()
		{
			//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
			//System.IO.BinaryReader dis;
			System.IO.MemoryStream bais;
			
			// SIZ marker segment
			if ((nfMarkSeg & SIZ_FOUND) != 0)
			{
				bais = new System.IO.MemoryStream(ht["SIZ"]);
				readSIZ(new Util.EndianBinaryReader(bais, true));
			}
			
			// COM marker segments
			if ((nfMarkSeg & COM_FOUND) != 0)
			{
				for (var i = 0; i < nCOMMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"COM{i}"]);
					readCOM(new Util.EndianBinaryReader(bais, true), true, 0, i);
				}
			}
			
			// CRG marker segment
			if ((nfMarkSeg & CRG_FOUND) != 0)
			{
				bais = new System.IO.MemoryStream(ht["CRG"]);
				readCRG(new Util.EndianBinaryReader(bais, true));
			}
			
			// COD marker segment
			if ((nfMarkSeg & COD_FOUND) != 0)
			{
				bais = new System.IO.MemoryStream(ht["COD"]);
				readCOD(new Util.EndianBinaryReader(bais, true), true, 0, 0);
			}
			
			// COC marker segments
			if ((nfMarkSeg & COC_FOUND) != 0)
			{
				for (var i = 0; i < nCOCMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"COC{i}"]);
					readCOC(new Util.EndianBinaryReader(bais, true), true, 0, 0);
				}
			}
			
			// RGN marker segment
			if ((nfMarkSeg & RGN_FOUND) != 0)
			{
				for (var i = 0; i < nRGNMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"RGN{i}"]);
					readRGN(new Util.EndianBinaryReader(bais, true), true, 0, 0);
				}
			}
			
			// QCD marker segment
			if ((nfMarkSeg & QCD_FOUND) != 0)
			{
				bais = new System.IO.MemoryStream(ht["QCD"]);
				readQCD(new Util.EndianBinaryReader(bais, true), true, 0, 0);
			}
			
			// QCC marker segments
			if ((nfMarkSeg & QCC_FOUND) != 0)
			{
				for (var i = 0; i < nQCCMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"QCC{i}"]);
					readQCC(new Util.EndianBinaryReader(bais, true), true, 0, 0);
				}
			}
			
			// POC marker segment
			if ((nfMarkSeg & POC_FOUND) != 0)
			{
				bais = new System.IO.MemoryStream(ht["POC"]);
				readPOC(new Util.EndianBinaryReader(bais, true), true, 0, 0);
			}
			
			// PPM marker segments
			if ((nfMarkSeg & PPM_FOUND) != 0)
			{
				for (var i = 0; i < nPPMMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"PPM{i}"]);
					readPPM(new Util.EndianBinaryReader(bais));
				}
			}
			
			// Reset the hashtable
			ht = null;
		}
		
		/// <summary> Retrieves and reads all marker segments previously found in the
		/// tile-part header.
		/// 
		/// </summary>
		/// <param name="tileIdx">The index of the current tile
		/// 
		/// </param>
		/// <param name="tpIdx">Index of the current tile-part
		/// 
		/// </param>
		public virtual void  readFoundTilePartMarkSeg(int tileIdx, int tpIdx)
		{
			
			//UPGRADE_TODO: Class 'java.io.DataInputStream' was converted to 'System.IO.BinaryReader' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioDataInputStream'"
			//CoreJ2K.EndianBinaryReader dis;
			System.IO.MemoryStream bais;
			
			// COD marker segment
			if ((nfMarkSeg & COD_FOUND) != 0)
			{
				bais = new System.IO.MemoryStream(ht["COD"]);
				readCOD(new Util.EndianBinaryReader(bais, true), false, tileIdx, tpIdx);
			}
			
			// COC marker segments
			if ((nfMarkSeg & COC_FOUND) != 0)
			{
				for (var i = 0; i < nCOCMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"COC{i}"]);
					readCOC(new Util.EndianBinaryReader(bais, true), false, tileIdx, tpIdx);
				}
			}
			
			// RGN marker segment
			if ((nfMarkSeg & RGN_FOUND) != 0)
			{
				for (var i = 0; i < nRGNMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"RGN{i}"]);
					readRGN(new Util.EndianBinaryReader(bais, true), false, tileIdx, tpIdx);
				}
			}
			
			// QCD marker segment
			if ((nfMarkSeg & QCD_FOUND) != 0)
			{
				bais = new System.IO.MemoryStream(ht["QCD"]);
				readQCD(new Util.EndianBinaryReader(bais, true), false, tileIdx, tpIdx);
			}
			
			// QCC marker segments
			if ((nfMarkSeg & QCC_FOUND) != 0)
			{
				for (var i = 0; i < nQCCMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"QCC{i}"]);
					readQCC(new Util.EndianBinaryReader(bais, true), false, tileIdx, tpIdx);
				}
			}
			
			// POC marker segment
			if ((nfMarkSeg & POC_FOUND) != 0)
			{
				bais = new System.IO.MemoryStream(ht["POC"]);
				readPOC(new Util.EndianBinaryReader(bais, true), false, tileIdx, tpIdx);
			}
			
			// COM marker segments
			if ((nfMarkSeg & COM_FOUND) != 0)
			{
				for (var i = 0; i < nCOMMarkSeg; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"COM{i}"]);
					readCOM(new Util.EndianBinaryReader(bais, true), false, tileIdx, i);
				}
			}
			
			// PPT marker segments
			if ((nfMarkSeg & PPT_FOUND) != 0)
			{
				for (var i = 0; i < nPPTMarkSeg[tileIdx][tpIdx]; i++)
				{
					bais = new System.IO.MemoryStream(ht[$"PPT{i}"]);
					readPPT(new Util.EndianBinaryReader(bais, true), tileIdx, tpIdx);
				}
			}
			
			// Reset ht
			ht = null;
		}
		
		/// <summary> Creates a HeaderDecoder instance and read in two passes the main header
		/// of the codestream. The first and last marker segments shall be
		/// respectively SOC and SOT.
		/// 
		/// </summary>
		/// <param name="ehs">The encoded header stream where marker segments are
		/// extracted.
		/// 
		/// </param>
		/// <param name="pl">The ParameterList object of the decoder
		/// 
		/// </param>
		/// <param name="hi">The HeaderInfo holding information found in marker segments
		/// 
		/// </param>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoded header stream.
		/// </exception>
		/// <exception cref="EOFException">If the end of the encoded header stream is
		/// reached before getting all the data.
		/// </exception>
		/// <exception cref="CorruptedCodestreamException">If invalid data is found in the
		/// codestream main header.
		/// 
		/// </exception>
		public HeaderDecoder(RandomAccessIO ehs, ParameterList pl, HeaderInfo hi)
		{
			
			this.hi = hi;
            // CONVERSION PROBLEM?
			//this.verbose = verbose;
			
			pl.checkList(OPT_PREFIX, ParameterList.toNameArray(pinfo));
			
			mainHeadOff = ehs.Pos;
			if (ehs.readShort() != Markers.SOC)
			{
				throw new CorruptedCodestreamException("SOC marker segment not " + " found at the " + "beginning of the " + "codestream.");
			}
			
			// First Pass: Decode and store main header information until the SOT
			// marker segment is found
			nfMarkSeg = 0;
			do 
			{
				extractMainMarkSeg(ehs.readShort(), ehs);
			}
			while ((nfMarkSeg & SOT_FOUND) == 0); //Stop when SOT is found
			ehs.seek(ehs.Pos - 2); // Realign codestream on SOT marker
			
			// Second pass: Read each marker segment previously found
			readFoundMainMarkSeg();
		}
		
		/// <summary> Creates and returns the entropy decoder corresponding to the
		/// information read from the codestream header and with the special
		/// additional parameters from the parameter list.
		/// 
		/// </summary>
		/// <param name="src">The bit stream reader agent where to get code-block data
		/// from.
		/// 
		/// </param>
		/// <param name="pl">The parameter list containing parameters applicable to the
		/// entropy decoder (other parameters can also be present).
		/// 
		/// </param>
		/// <returns> The entropy decoder
		/// 
		/// </returns>
		public virtual EntropyDecoder createEntropyDecoder(CodedCBlkDataSrcDec src, ParameterList pl)
		{
			bool doer;
			bool verber;
			int mMax;
			
			// Check parameters
			pl.checkList(EntropyDecoder.OPT_PREFIX, ParameterList.toNameArray(EntropyDecoder.ParameterInfo));
			// Get error detection option
			doer = pl.getBooleanParameter("Cer");
			// Get verbose error detection option
			verber = pl.getBooleanParameter("Cverber");
			
			// Get maximum number of bit planes from m quit condition
			mMax = pl.getIntParameter("m_quit");
			return new StdEntropyDecoder(src, decSpec, doer, verber, mMax);
		}
		
		/// <summary> Creates and returns the EnumeratedColorSpaceMapper
		/// corresponding to the information read from the JP2 image file
		/// via the ColorSpace parameter.
		/// 
		/// </summary>
		/// <param name="src">The bit stream reader agent where to get code-block
		/// data from.
		/// </param>
		/// <param name="csMap">provides color space information from the image file
		/// 
		/// </param>
		/// <returns> The color space mapping object
		/// </returns>
		/// <exception cref="IOException">image access exception
		/// </exception>
		/// <exception cref="ICCProfileException">if image contains a bad icc profile
		/// </exception>
		/// <exception cref="ColorSpaceException">if image contains a bad colorspace box
		/// 
		/// </exception>
		public virtual BlkImgDataSrc createColorSpaceMapper(BlkImgDataSrc src, ColorSpace csMap)
		{
			return ColorSpaceMapper.createInstance(src, csMap);
		}
		
		/// <summary> Creates and returns the ChannelDefinitonMapper which maps the
		/// input channels to the channel definition for the appropriate
		/// colorspace.
		/// 
		/// </summary>
		/// <param name="src">The bit stream reader agent where to get code-block
		/// data from.
		/// </param>
		/// <param name="csMap">provides color space information from the image file
		/// 
		/// </param>
		/// <returns> The channel definition mapping object
		/// </returns>
		/// <exception cref="IOException">image access exception
		/// </exception>
		/// <exception cref="ColorSpaceException">if image contains a bad colorspace box
		/// 
		/// </exception>
		public virtual BlkImgDataSrc createChannelDefinitionMapper(BlkImgDataSrc src, ColorSpace csMap)
		{
			return ChannelDefinitionMapper.createInstance(src, csMap);
		}
		
		/// <summary> Creates and returns the PalettizedColorSpaceMapper which uses
		/// the input samples as indicies into a sample palette to
		/// construct the output.
		/// 
		/// </summary>
		/// <param name="src">The bit stream reader agent where to get code-block
		/// data from.
		/// </param>
		/// <param name="csMap">provides color space information from the image file
		/// 
		/// </param>
		/// <returns> a  PalettizedColorSpaceMapper instance
		/// </returns>
		/// <exception cref="IOException">image access exception
		/// </exception>
		/// <exception cref="ColorSpaceException">if image contains a bad colorspace box
		/// 
		/// </exception>
		public virtual BlkImgDataSrc createPalettizedColorSpaceMapper(BlkImgDataSrc src, ColorSpace csMap)
		{
			return PalettizedColorSpaceMapper.createInstance(src, csMap);
		}
		
		/// <summary> Creates and returns the Resampler which converts the input
		/// source to one in which all channels have the same number of
		/// samples.  This is required for colorspace conversions.
		/// 
		/// </summary>
		/// <param name="src">The bit stream reader agent where to get code-block
		/// data from.
		/// </param>
		/// <param name="csMap">provides color space information from the image file
		/// 
		/// </param>
		/// <returns> The resampled BlkImgDataSrc
		/// </returns>
		/// <exception cref="IOException">image access exception
		/// </exception>
		/// <exception cref="ColorSpaceException">if image contains a bad colorspace box
		/// 
		/// </exception>
		public virtual BlkImgDataSrc createResampler(BlkImgDataSrc src, ColorSpace csMap)
		{
			return Resampler.createInstance(src, csMap);
		}
		
		/// <summary> Creates and returns the ROIDeScaler corresponding to the information
		/// read from the codestream header and with the special additional
		/// parameters from the parameter list.
		/// 
		/// </summary>
		/// <param name="src">The bit stream reader agent where to get code-block data
		/// from.
		/// 
		/// </param>
		/// <param name="pl">The parameter list containing parameters applicable to the
		/// entropy decoder (other parameters can also be present).
		/// 
		/// </param>
		/// <param name="decSpec2">The DecoderSpecs instance after any image manipulation.
		/// 
		/// </param>
		/// <returns> The ROI descaler.
		/// 
		/// </returns>
		public virtual ROIDeScaler createROIDeScaler(CBlkQuantDataSrcDec src, ParameterList pl, DecoderSpecs decSpec2)
		{
			return ROIDeScaler.createInstance(src, pl, decSpec2);
		}
		
		/// <summary> Method that resets members indicating which markers have already been
		/// found
		/// 
		/// </summary>
		public virtual void  resetHeaderMarkers()
		{
			// The found status of PLM remains since only PLM OR PLT allowed
			// Same goes for PPM and PPT
			nfMarkSeg = nfMarkSeg & (PLM_FOUND | PPM_FOUND);
			nCOCMarkSeg = 0;
			nQCCMarkSeg = 0;
			nCOMMarkSeg = 0;
			nRGNMarkSeg = 0;
		}
		
		/// <summary> Print information about the current header.
		/// 
		/// </summary>
		/// <returns> Information in a String
		/// 
		/// </returns>
		public override string ToString()
		{
			return hdStr;
		}
		
		/// <summary> Return the packed packet headers for a given tile.
		/// 
		/// </summary>
		/// <returns> An input stream containing the packed packet headers for a
		/// particular tile
		/// 
		/// </returns>
		/// <exception cref="IOException">If an I/O error occurs while reading from the
		/// encoder header stream
		/// 
		/// </exception>
		public virtual System.IO.MemoryStream getPackedPktHead(int tile)
		{
			
			if (pkdPktHeaders == null)
			{
				int i, t;
				pkdPktHeaders = new System.IO.MemoryStream[nTiles];
				for (i = nTiles - 1; i >= 0; i--)
				{
					pkdPktHeaders[i] = new System.IO.MemoryStream();
				}
				if (nPPMMarkSeg != 0)
				{
					// If this is first time packed packet headers are requested,
					// create packed packet headers from Nppm and Ippm fields
					int nppm;
					var nTileParts = tileOfTileParts.Count;
					byte[] temp;
					System.IO.MemoryStream pph;
					var allNppmIppm = new System.IO.MemoryStream();
					
					// Concatenate all Nppm and Ippm fields
					for (i = 0; i < nPPMMarkSeg; i++)
					{
						byte[] temp_byteArray;
						temp_byteArray = pPMMarkerData[i];
						allNppmIppm.Write(temp_byteArray, 0, temp_byteArray.Length);
					}
					pph = new System.IO.MemoryStream(allNppmIppm.ToArray());
					
					// Read all packed packet headers and concatenate for each
					// tile part
					for (i = 0; i < nTileParts; i++)
					{
						t = tileOfTileParts[i];
						// get Nppm value
						nppm = (pph.ReadByte() << 24) | (pph.ReadByte() << 16) | (pph.ReadByte() << 8) | (pph.ReadByte());
						
						temp = new byte[nppm];
						// get ippm field
                        pph.Read(temp, 0, temp.Length); //SupportClass.ReadInput(pph, temp, 0, temp.Length);
						byte[] temp_byteArray2;
						temp_byteArray2 = temp;
						pkdPktHeaders[t].Write(temp_byteArray2, 0, temp_byteArray2.Length);
					}
				}
				else
				{
					int tp;
					// Write all packed packet headers to pkdPktHeaders
					for (t = nTiles - 1; t >= 0; t--)
					{
						for (tp = 0; tp < nTileParts[t]; tp++)
						{
							for (i = 0; i < nPPTMarkSeg[t][tp]; i++)
							{
								byte[] temp_byteArray3;
								temp_byteArray3 = tilePartPkdPktHeaders[t][tp][i];
								pkdPktHeaders[t].Write(temp_byteArray3, 0, temp_byteArray3.Length);
							}
						}
					}
				}
			}
			
			return new System.IO.MemoryStream(pkdPktHeaders[tile].ToArray());
		}
	}
}