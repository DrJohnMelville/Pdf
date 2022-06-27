/// <summary>**************************************************************************
/// 
/// $Id: ICCProfiler.java,v 1.2 2002/08/08 14:08:27 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using Melville.CSJ2K.j2k.decoder;
using Melville.CSJ2K.j2k.image;
using Melville.CSJ2K.j2k.util;
using Melville.CSJ2K.j2k.io;
using Melville.CSJ2K.Color;
using Melville.CSJ2K.Icc.Lut;
using Melville.Icc.Model;
using Melville.Icc.Parser;
using ColorSpace = Melville.CSJ2K.Color.ColorSpace;

namespace Melville.CSJ2K.Icc
{
	
	/// <summary> This class provides ICC Profiling API for the jj2000.j2k imaging chain
	/// by implementing the BlkImgDataSrc interface, in particular the getCompData
	/// and getInternCompData methods.
	/// 
	/// </summary>
	/// <seealso cref="ICCProfile">
	/// </seealso>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCProfiler:ColorSpaceMapper
	{
		
		/// <summary>The prefix for ICC Profiler options </summary>
		new public const char OPT_PREFIX = 'I';
		
		/// <summary>Platform dependant end of line String. </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'eol '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		new protected internal static readonly String eol = Environment.NewLine;
		
		// ICCProfiles.
		internal RestrictedICCProfile? ricc = null;
		internal ICCProfile? icc = null;
		
		// Temporary variables needed during profiling.
		private DataBlkInt[] tempInt; // Holds the results of the transform.
		private DataBlkFloat[] tempFloat; // Holds the results of the transform.
		
		private object xform = null;

		/// <summary> Ctor which creates an ICCProfile for the image and initializes
		/// all data objects (input, working, output).
		/// 
		/// </summary>
		/// <param name="src">-- Source of image data
		/// </param>
		/// <param name="csm">-- provides colorspace info
		/// 
		/// </param>
		/// <exception cref="IOException">
		/// </exception>
		/// <exception cref="ICCProfileException">
		/// </exception>
		/// <exception cref="IllegalArgumentException">
		/// </exception>
		public ICCProfiler(BlkImgDataSrc src, ColorSpace csMap):base(src, csMap)
		{
			initialize();
			
			var iccp = getICCProfile(csMap);
			if (ncomps == 1)
			{
				xform = new MonochromeTransformTosRGB(iccp);
			}
			else
			{
				xform = new MatrixBasedTransformTosRGB(iccp);
			}
			
			/* end ICCProfiler ctor */
		}
		
		/// <summary>General utility used by ctors </summary>
		[MemberNotNull("tempInt")]
		[MemberNotNull("tempFloat")]
		private void  initialize()
		{
			
			tempInt = new DataBlkInt[ncomps];
			tempFloat = new DataBlkFloat[ncomps];
			
			/* For each component, get the maximum data value, a reference
			* to the pixel data and set up working and temporary DataBlks
			* for both integer and float output.
			*/
			for (int i = 0; i < ncomps; ++i)
			{
				tempInt[i] = new DataBlkInt();
				tempFloat[i] = new DataBlkFloat();
			}
		}
		
		/// <summary> Get the ICCProfile information JP2 ColorSpace</summary>
		/// <param name="csm">provides all necessary info about the colorspace
		/// </param>
		/// <returns> ICCMatrixBasedInputProfile for 3 component input and
		/// ICCMonochromeInputProfile for a 1 component source.  Returns
		/// null if exceptions were encountered.
		/// </returns>
		/// <exception cref="ColorSpaceException">
		/// </exception>
		/// <exception cref="ICCProfileException">
		/// </exception>
		/// <exception cref="IllegalArgumentException">
		/// </exception>
		private IccProfile getICCProfile(ColorSpace csm) =>
			Task.Run(async ()=> await 
				new IccParser(PipeReader.Create(new MemoryStream(csm.ICCProfile!))).ParseAsync()).Result;

		/// <summary> Returns, in the blk argument, a block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a copy of the internal data, therefore the returned data
		/// can be modified "in place".
		/// 
		/// <P>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' of
		/// the returned data is 0, and the 'scanw' is the same as the block's
		/// width. See the 'DataBlk' class.
		/// 
		/// <P>If the data array in 'blk' is 'null', then a new one is created. If
		/// the data array is not 'null' then it is reused, and it must be large
		/// enough to contain the block's data. Otherwise an 'ArrayStoreException'
		/// or an 'IndexOutOfBoundsException' is thrown by the Java system.
		/// 
		/// <P>The returned data has its 'progressive' attribute set to that of the
		/// input data.
		/// 
		/// </summary>
		/// <param name="out">Its coordinates and dimensions specify the area to
		/// return. If it contains a non-null data array, then it must have the
		/// correct dimensions. If it contains a null data array a new one is
		/// created. The fields in this object are modified to return the data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data. Only 0
		/// and 3 are valid.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="getInternCompData">
		/// 
		/// </seealso>
		public override DataBlk getCompData(DataBlk outblk, int c)
		{
	
			try
			{
				if (ncomps != 1 && ncomps != 3)
				{
					String msg = "ICCProfiler: icc profile _not_ applied to " + ncomps + " component image";
					FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.WARNING, msg);
					return src.getCompData(outblk, c);
				}

				TryDoTransform(outblk, c);
			}
			catch (MatrixBasedTransformException e)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.ERROR, "matrix transform problem:\n" + e.Message);
				if (pl.getParameter("debug").Equals("on"))
				{
					SupportClass.WriteStackTrace(e);
				}
				else
				{
					FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.ERROR, "Use '-debug' option for more details");
				}
				return null;
			}
			catch (MonochromeTransformException e)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.ERROR, "monochrome transform problem:\n" + e.Message);
				if (pl.getParameter("debug").Equals("on"))
				{
					SupportClass.WriteStackTrace(e);
				}
				else
				{
					FacilityManager.getMsgLogger().printmsg(MsgLogger_Fields.ERROR, "Use '-debug' option for more details");
				}
				return null;
			}
			
			return outblk;
		}

		private void TryDoTransform(DataBlk outblk, int c)
		{
			// Calculate all components:
			for (int i = 0; i < ncomps; ++i)
			{
				CalculateSingleComponent(outblk, i);
			}

			DoTransform(outblk, c);

			// Initialize the output block geometry and set the profiled
			// data into the output block.
			outblk.offset = 0;
			outblk.scanw = outblk.w;
		}

		private void DoTransform(DataBlk outblk, int c)
		{
			switch (outblk.DataType)
			{
				// Int and Float data only
				case DataBlk.TYPE_INT:

					if (ncomps == 1)
					{
						((MonochromeTransformTosRGB)xform).apply(workInt[c], tempInt[c]);
					}
					else
					{
						// ncomps == 3
						((MatrixBasedTransformTosRGB)xform).apply(workInt, tempInt);
					}

					outblk.progressive = inInt[c].progressive;
					outblk.Data = tempInt[c].Data;
					break;


				case DataBlk.TYPE_FLOAT:

					if (ncomps == 1)
					{
						((MonochromeTransformTosRGB)xform).apply(workFloat[c], tempFloat[c]);
					}
					else
					{
						// ncomps == 3
						((MatrixBasedTransformTosRGB)xform).apply(workFloat, tempFloat);
					}

					outblk.progressive = inFloat[c].progressive;
					outblk.Data = tempFloat[c].Data;
					break;


				case DataBlk.TYPE_SHORT:
				case DataBlk.TYPE_BYTE:
				default:
					// Unsupported output type. 
					throw new ArgumentException("invalid source datablock" + " type");
			}
		}

		private void CalculateSingleComponent(DataBlk outblk, int i)
		{
			// Initialize general input and output indexes
			int kOut = -1;
			int kIn = -1;

			switch (outblk.DataType)
			{
				// Int and Float data only
				case DataBlk.TYPE_INT:

					// Set up the DataBlk geometry
					copyGeometry(workInt[i], outblk);
					copyGeometry(tempInt[i], outblk);
					copyGeometry(inInt[i], outblk);
					InternalBuffer = outblk;

					// Reference the output array
					workDataInt[i] = (int[])workInt[i].Data;

					// Request data from the source.    
					inInt[i] = (DataBlkInt)src.getInternCompData(inInt[i], i);
					dataInt[i] = inInt[i].DataInt;
					
					for (int row = 0; row < outblk.h; ++row)
					{
						var leftedgeIn = inInt[i].offset + row * inInt[i].scanw;
						var rightedgeIn = leftedgeIn + inInt[i].w;
						var leftedgeOut = outblk.offset + row * outblk.scanw;
				
						dataInt[i].AsSpan(leftedgeIn, rightedgeIn-leftedgeIn).CopyTo(workDataInt[i].AsSpan(leftedgeOut..));
					}

					break;


				case DataBlk.TYPE_FLOAT:

					// Set up the DataBlk geometry
					copyGeometry(workFloat[i], outblk);
					copyGeometry(tempFloat[i], outblk);
					copyGeometry(inFloat[i], outblk);
					InternalBuffer = outblk;

					// Reference the output array
					workDataFloat[i] = (float[])workFloat[i].Data;

					// Request data from the source.    
					inFloat[i] = (DataBlkFloat)src.getInternCompData(inFloat[i], i);
					dataFloat[i] = inFloat[i].DataFloat;

					// The nitty-gritty.

					for (int row = 0; row < outblk.h; ++row)
					{
						var leftedgeIn = inFloat[i].offset + row * inFloat[i].scanw;
						var rightedgeIn = leftedgeIn + inFloat[i].w;
						var leftedgeOut = outblk.offset + row * outblk.scanw;
						
						workDataFloat[i].AsSpan(leftedgeIn, rightedgeIn-leftedgeIn).CopyTo(workDataFloat[i].AsSpan(leftedgeOut..));
					}

					break;


				case DataBlk.TYPE_SHORT:
				case DataBlk.TYPE_BYTE:
				default:
					// Unsupported output type. 
					throw new ArgumentException("Invalid source " + "datablock type");
			}
		}

		/// <summary> Returns, in the blk argument, a block of image data containing the
		/// specifed rectangular area, in the specified component. The data is
		/// returned, as a reference to the internal data, if any, instead of as a
		/// copy, therefore the returned data should not be modified.
		/// 
		/// <P>The rectangular area to return is specified by the 'ulx', 'uly', 'w'
		/// and 'h' members of the 'blk' argument, relative to the current
		/// tile. These members are not modified by this method. The 'offset' and
		/// 'scanw' of the returned data can be arbitrary. See the 'DataBlk' class.
		/// 
		/// <P>This method, in general, is more efficient than the 'getCompData()'
		/// method since it may not copy the data. However if the array of returned
		/// data is to be modified by the caller then the other method is probably
		/// preferable.
		/// 
		/// <P>If possible, the data in the returned 'DataBlk' should be the
		/// internal data itself, instead of a copy, in order to increase the data
		/// transfer efficiency. However, this depends on the particular
		/// implementation (it may be more convenient to just return a copy of the
		/// data). This is the reason why the returned data should not be modified.
		/// 
		/// <P>If the data array in <tt>blk</tt> is <tt>null</tt>, then a new one
		/// is created if necessary. The implementation of this interface may
		/// choose to return the same array or a new one, depending on what is more
		/// efficient. Therefore, the data array in <tt>blk</tt> prior to the
		/// method call should not be considered to contain the returned data, a
		/// new array may have been created. Instead, get the array from
		/// <tt>blk</tt> after the method has returned.
		/// 
		/// <P>The returned data may have its 'progressive' attribute set. In this
		/// case the returned data is only an approximation of the "final" data.
		/// 
		/// </summary>
		/// <param name="blk">Its coordinates and dimensions specify the area to return,
		/// relative to the current tile. Some fields in this object are modified
		/// to return the data.
		/// 
		/// </param>
		/// <param name="c">The index of the component from which to get the data.
		/// 
		/// </param>
		/// <returns> The requested DataBlk
		/// 
		/// </returns>
		/// <seealso cref="getCompData">
		/// 
		/// </seealso>
		public override DataBlk getInternCompData(DataBlk out_Renamed, int c)
		{
			return getCompData(out_Renamed, c);
		}
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override String ToString()
		{
			System.Text.StringBuilder rep = new System.Text.StringBuilder("[ICCProfiler:");
			System.Text.StringBuilder body = new System.Text.StringBuilder();
			if (icc != null)
			{
				body.Append(eol).Append(ColorSpace.indent("  ", icc.ToString()));
			}
			if (xform != null)
			{
				body.Append(eol).Append(ColorSpace.indent("  ", xform.ToString()));
			}
			rep.Append(ColorSpace.indent("  ", body));
			return rep.Append("]").ToString();
		}
		
	}
}