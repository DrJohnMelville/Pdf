/// <summary>**************************************************************************
/// 
/// $Id: ICCTagTable.java,v 1.1 2002/07/25 14:56:37 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>
using System;
using System.IO;
using ColorSpace = CoreJ2K.Color.ColorSpace;
using ICCProfileHeader = CoreJ2K.Icc.Types.ICCProfileHeader;
namespace CoreJ2K.Icc.Tags
{
	
	/// <summary> This class models an ICCTagTable as a HashTable which maps 
	/// ICCTag signatures (as Integers) to ICCTags.
	/// 
	/// On disk the tag table exists as a byte array conventionally aggragted into a
	/// structured sequence of types (bytes, shorts, ints, and floats.  The first four bytes
	/// are the integer count of tags in the table.  This is followed by an array of triplets,
	/// one for each tag. The triplets each contain three integers, which are the tag signature,
	/// the offset of the tag in the byte array and the length of the tag in bytes.
	/// The tag data follows.  Each tag consists of an integer (4 bytes) tag type, a reserved integer
	/// and the tag data, which varies depending on the tag.
	/// 
	/// </summary>
	/// <seealso cref="j2k.icc.tags.ICCTag" />
	/// <version> 	1.0
	/// </version>
	/// <author> 	Bruce A. Kern
	/// </author>
	public class ICCTagTable:System.Collections.Generic.Dictionary<int, ICCTag>
	{
		private static readonly int offTagCount;
		private static readonly int offTags;
		
		private System.Collections.Generic.List<Triplet> trios = new System.Collections.Generic.List<Triplet>(10);
		
		private int tagCount;
		
		
		private class Triplet
		{
			/// <summary>Tag identifier              </summary>
			internal int signature;
			/// <summary>absolute offset of tag data </summary>
			internal int offset;
			/// <summary>length of tag data          </summary>
			internal int count;
			/// <summary>size of an entry            </summary>
			public static readonly int size;
			
			
			internal Triplet(int signature, int offset, int count)
			{
				this.signature = signature;
				this.offset = offset;
				this.count = count;
			}
			static Triplet()
			{
				size = 3 * ICCProfile.int_size;
			}
		}
		
		/// <summary> Representation of a tag table</summary>
		public override string ToString()
		{
			var rep = new System.Text.StringBuilder($"[ICCTagTable containing {tagCount} tags:");
			var body = new System.Text.StringBuilder("  ");
			System.Collections.IEnumerator keys = Keys.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (keys.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				var key = (int) keys.Current;
				var tag = this[key];
				body.Append(Environment.NewLine).Append(tag);
			}
			rep.Append(ColorSpace.indent("  ", body));
			return rep.Append("]").ToString();
		}
		
		
		/// <summary> Factory method for creating a tag table from raw input.</summary>
		/// <param name="byte">array of unstructured data representing a tag
		/// </param>
		/// <returns> ICCTagTable
		/// </returns>
		public static ICCTagTable createInstance(byte[] data)
		{
			var tags = new ICCTagTable(data);
			return tags;
		}
		
		
		/// <summary> Ctor used by factory method.</summary>
		/// <param name="byte">raw tag data
		/// </param>
		protected internal ICCTagTable(byte[] data)
		{
            tagCount = ICCProfile.getInt(data, offTagCount);
			
			var offset = offTags;
			for (var i = 0; i < tagCount; ++i)
			{
                var signature = ICCProfile.getInt(data, offset);
                var tagOffset = ICCProfile.getInt(data, offset + ICCProfile.int_size);
                var length = ICCProfile.getInt(data, offset + 2 * ICCProfile.int_size);
				trios.Add(new Triplet(signature, tagOffset, length));
				offset += 3 * ICCProfile.int_size;
			}
			
			
			System.Collections.Generic.IEnumerator<Triplet> Enum = trios.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (Enum.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				var trio = Enum.Current;
				var tag = ICCTag.createInstance(trio.signature, data, trio.offset, trio.count);
				this[tag.signature] = tag;
			}
		}
		
		
		/// <summary> Output the table to a disk</summary>
		/// <param name="raf">RandomAccessFile which receives the table.
		/// </param>
		/// <exception cref="IOException">
		/// </exception>
		//UPGRADE_TODO: Class 'java.io.RandomAccessFile' was converted to 'System.IO.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioRandomAccessFile'"
		public virtual void  write(Stream raf)
		{
			
			var ntags = trios.Count;
			
			var countOff = ICCProfileHeader.size;
			var tagOff = countOff + ICCProfile.int_size;
			var dataOff = tagOff + 3 * ntags * ICCProfile.int_size;
			
			raf.Seek(countOff, SeekOrigin.Begin);
			BinaryWriter temp_BinaryWriter;
			temp_BinaryWriter = new BinaryWriter(raf);
			temp_BinaryWriter.Write(ntags);
			
			var currentTagOff = tagOff;
			var currentDataOff = dataOff;
			
			System.Collections.IEnumerator enum_Renamed = trios.GetEnumerator();
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationhasMoreElements'"
			while (enum_Renamed.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilEnumerationnextElement'"
				var trio = (Triplet) enum_Renamed.Current;
				var tag = this[trio.signature];
				
				raf.Seek(currentTagOff, SeekOrigin.Begin);
				BinaryWriter temp_BinaryWriter2;
				temp_BinaryWriter2 = new BinaryWriter(raf);
				temp_BinaryWriter2.Write(tag.signature);
				BinaryWriter temp_BinaryWriter3;
				temp_BinaryWriter3 = new BinaryWriter(raf);
				temp_BinaryWriter3.Write(currentDataOff);
				BinaryWriter temp_BinaryWriter4;
				temp_BinaryWriter4 = new BinaryWriter(raf);
				temp_BinaryWriter4.Write(tag.count);
				currentTagOff += 3 * Triplet.size;
				
				raf.Seek(currentDataOff, SeekOrigin.Begin);
				raf.Write(tag.data, tag.offset, tag.count);
				currentDataOff += tag.count;
			}
		}
		
		/* end class ICCTagTable */
		static ICCTagTable()
		{
			offTagCount = ICCProfileHeader.size;
			offTags = offTagCount + ICCProfile.int_size;
		}
	}
}