/// <summary>**************************************************************************
/// 
/// $Id: ICCXYZType.java,v 1.1 2002/07/25 14:56:37 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;

namespace CoreJ2K.Icc.Tags
{
	
	/// <summary> A tag containing a triplet.
	/// 
	/// </summary>
	/// <seealso cref="j2k.icc.tags.ICCXYZTypeReverse" />
	/// <seealso cref="j2k.icc.types.XYZNumber" />
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCXYZType:ICCTag
	{
		
		/// <summary>x component </summary>
		public long x;
		/// <summary>y component </summary>
		public long y;
		/// <summary>z component </summary>
		public long z;
		
		/// <summary>Normalization utility </summary>
		public static long DoubleToXYZ(double x)
		{
			//UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
			return (long) Math.Floor(x * 65536.0 + 0.5);
		}
		
		/// <summary>Normalization utility </summary>
		public static double XYZToDouble(long x)
		{
			return x / 65536.0;
		}
		
		/// <summary> Construct this tag from its constituant parts</summary>
		/// <param name="signature">tag id
		/// </param>
		/// <param name="data">array of bytes
		/// </param>
		/// <param name="offset">to data in the data array
		/// </param>
		/// <param name="length">of data in the data array
		/// </param>
		protected internal ICCXYZType(int signature, byte[] data, int offset, int length):base(signature, data, offset, length)
		{
            x = ICCProfile.getInt(data, offset + 2 * ICCProfile.int_size);
            y = ICCProfile.getInt(data, offset + 3 * ICCProfile.int_size);
            z = ICCProfile.getInt(data, offset + 4 * ICCProfile.int_size);
		}
		
		
		/// <summary>Return the string rep of this tag. </summary>
		public override string ToString()
		{
			return $"[{base.ToString()}({x}, {y}, {z})]";
		}
		
		
		/// <summary>Write to a file. </summary>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		public virtual void  write(System.IO.Stream raf)
		{
            var xb = ICCProfile.setLong(x);
            var yb = ICCProfile.setLong(y);
            var zb = ICCProfile.setLong(z);
			
            // CONVERSION PROBLEM?
			raf.Write(xb, ICCProfile.int_size, 0);
			raf.Write(yb, ICCProfile.int_size, 0);
			raf.Write(zb, ICCProfile.int_size, 0);
		}
		
		
		/* end class ICCXYZType */
	}
}