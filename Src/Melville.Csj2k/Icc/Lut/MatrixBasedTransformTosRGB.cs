/// <summary>**************************************************************************
/// 
/// $Id: MatrixBasedTransformTosRGB.java,v 1.1 2002/07/25 14:56:49 grosbois Exp $
/// 
/// Copyright Eastman Kodak Company, 343 State Street, Rochester, NY 14650
/// $Date $
/// ***************************************************************************
/// </summary>

using Melville.CSJ2K.j2k.image;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model;

namespace Melville.CSJ2K.Icc.Lut
{
	public class MatrixBasedTransformTosRGB
	{
		private readonly IColorTransform transform;

		public MatrixBasedTransformTosRGB(IccProfile profile)
		{
			transform = profile.DeviceToSrgb();
		}


		public void apply(DataBlkInt[] inb, DataBlkInt[] outb)
		{
			int[][] in_Renamed = new int[3][]; // data references.
			int[]?[] out_Renamed = new int[3][]; // data references.

			// for each component (rgb)
			for (int c = 0; c < 3; ++c)
			{

				// Reference the input and output samples.
				in_Renamed[c] = (int[])inb[c].Data!;
				out_Renamed[c] = (int[]?)outb[c].Data;

				// Assure a properly sized output buffer.
				if (out_Renamed[c] == null || out_Renamed[c]!.Length < in_Renamed[c].Length)
				{
					out_Renamed[c] = new int[in_Renamed[c].Length];
					outb[c].Data = out_Renamed[c];
				}

//				in_Renamed[c].AsSpan().CopyTo(out_Renamed[c].AsSpan());
			}

			Span<float> color = stackalloc float[3];
			for ( int i = 0; i < in_Renamed[0].Length; i++)
			{
				color[0] = InTransform(in_Renamed[ICCProfile.RED][i]);
				color[1] = InTransform(in_Renamed[ICCProfile.GREEN][i]);
				color[2] = InTransform(in_Renamed[ICCProfile.BLUE][i]);
		//		transform.Transform(color, color);
				out_Renamed[ICCProfile.RED]![i] = OutTransform(color[0]);
				out_Renamed[ICCProfile.GREEN]![i] = OutTransform(color[1]);
				out_Renamed[ICCProfile.BLUE]![i] = OutTransform(color[2]);
				// out_Renamed[0]![i] = in_Renamed[0][i];
				// out_Renamed[1]![i] = in_Renamed[1][i];
				// out_Renamed[2]![i] = in_Renamed[2][i];
			}

		}

		private float InTransform(int x) => x + 128;
		private int OutTransform(float x) => (int)(x - 128f);

		/// <summary> Performs the transform.  Pass the input throught the LookUpTableFP, apply the
		/// matrix to the output and finally pass the intermediate buffer through the
		/// LookUpTable8.  This operation is designated (LFP*M*L8) * Data are already 
		/// constructed.  Although the data for only one component is returned, the
		/// transformation must be done for all components, because the matrix application
		/// involves a linear combination of component input to produce the output.
		/// </summary>
		public virtual void  apply(DataBlkFloat[] inb, DataBlkFloat[] outb)
		{
			float[][] in_Renamed = new float[3][]; // data references.
			float[]?[] out_Renamed = new float[3][]; // data references.

			// for each component (rgb)
			for (int c = 0; c < 3; ++c)
			{
				
				// Reference the input and output samples.
				in_Renamed[c] = (float[]) inb[c].Data!;
				out_Renamed[c] = (float[]?) outb[c].Data;
				
				// Assure a properly sized output buffer.
				if (out_Renamed[c] == null || out_Renamed[c]!.Length < in_Renamed[c].Length)
				{
					out_Renamed[c] = new float[in_Renamed[c].Length];
					outb[c].Data = out_Renamed[c];
				}

				in_Renamed[c].AsSpan().CopyTo(out_Renamed[c].AsSpan());
			}
		}
	}
}