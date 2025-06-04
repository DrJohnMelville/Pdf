/// <summary>**************************************************************************
/// 
/// $Id: ChannelDefinitionBox.java,v 1.1 2002/07/25 14:50:46 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using System.Collections.Generic;
using ICCProfile = CoreJ2K.Icc.ICCProfile;
using io_RandomAccessIO = CoreJ2K.j2k.io.RandomAccessIO;

namespace CoreJ2K.Color.Boxes
{
	
	/// <summary> This class maps the components in the codestream
	/// to channels in the image.  It models the Component
	/// Mapping box in the JP2 header.
	/// 
	/// </summary>
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public sealed class ChannelDefinitionBox:JP2Box
	{
		public int NDefs { get; private set; }

		private Dictionary<int, int[]> definitions = new Dictionary<int, int[]>();
		
		/// <summary> Construct a ChannelDefinitionBox from an input image.</summary>
		/// <param name="in">RandomAccessIO jp2 image
		/// </param>
		/// <param name="boxStart">offset to the start of the box in the image
		/// </param>
		/// <exception cref="IOException,">ColorSpaceException 
		/// </exception>
		public ChannelDefinitionBox(io_RandomAccessIO in_Renamed, int boxStart):base(in_Renamed, boxStart)
		{
			readBox();
		}
		
		/// <summary>Analyze the box content. </summary>
		private void  readBox()
		{
			
			var bfr = new byte[8];
			
			in_Renamed.seek(dataStart);
			in_Renamed.readFully(bfr, 0, 2);
            NDefs = ICCProfile.getShort(bfr, 0) & 0x0000ffff;
			
			var offset = dataStart + 2;
			in_Renamed.seek(offset);
			for (var i = 0; i < NDefs; ++i)
			{
				in_Renamed.readFully(bfr, 0, 6);
                int channel = ICCProfile.getShort(bfr, 0);
				var channel_def = new int[3];
				channel_def[0] = getCn(bfr);
				channel_def[1] = getTyp(bfr);
				channel_def[2] = getAsoc(bfr);
				definitions[channel_def[0]] = channel_def;
			}
		}
		
		/* Return the channel association. */
		public int getCn(int asoc)
		{
			IEnumerator<int> keys = definitions.Keys.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (keys.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				var bfr = definitions[keys.Current];
				if (asoc == getAsoc(bfr))
					return getCn(bfr);
			}
			return asoc;
		}
		
		/* Return the channel type. */
		public int getTyp(int channel)
		{
			var bfr = definitions[channel];
			return getTyp(bfr);
		}
		
		/* Return the associated channel of the association. */
		public int getAsoc(int channel)
		{
			var bfr = definitions[channel];
			return getAsoc(bfr);
		}
		
		
		/// <summary>Return a suitable String representation of the class instance. </summary>
		public override string ToString()
		{
			var rep = new System.Text.StringBuilder("[ChannelDefinitionBox ").Append(Environment.NewLine).Append("  ");
			rep.Append("ndefs= ").Append(Convert.ToString(NDefs));
			
			IEnumerator<int> keys = definitions.Keys.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (keys.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				var bfr = definitions[keys.Current];
				rep.Append(Environment.NewLine).Append("  ").Append("Cn= ").Append(Convert.ToString(getCn(bfr))).Append(", ").Append("Typ= ").Append(Convert.ToString(getTyp(bfr))).Append(", ").Append("Asoc= ").Append(Convert.ToString(getAsoc(bfr)));
			}
			
			rep.Append("]");
			return rep.ToString();
		}
		
		/// <summary>Return the channel from the record.</summary>
		private int getCn(byte[] bfr)
		{
            return ICCProfile.getShort(bfr, 0);
		}
		
		/// <summary>Return the channel type from the record.</summary>
		private int getTyp(byte[] bfr)
		{
            return ICCProfile.getShort(bfr, 2);
		}
		
		/// <summary>Return the associated channel from the record.</summary>
		private int getAsoc(byte[] bfr)
		{
            return ICCProfile.getShort(bfr, 4);
		}
		
		private int getCn(int[] bfr)
		{
			return bfr[0];
		}
		
		private int getTyp(int[] bfr)
		{
			return bfr[1];
		}
		
		private int getAsoc(int[] bfr)
		{
			return bfr[2];
		}
		
		/* end class ChannelDefinitionBox */
		static ChannelDefinitionBox()
		{
			{
				type = 0x63646566;
			}
		}
	}
}