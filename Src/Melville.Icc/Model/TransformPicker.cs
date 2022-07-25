using System.Diagnostics.CodeAnalysis;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;


namespace Melville.Icc.Model;

public enum TransformationNames : uint
{
     AtoB0 = 0x41324230,
     AtoB1 = 0x41324231,
     AtoB2 = 0x41324232,
     BtoA0 = 0x42324130,
     BtoA1 = 0x42324131,
     BtoA2 = 0x42324132,
     DtoB0 = 0x44324230,
     DtoB1 = 0x44324231,
     DtoB2 = 0x44324232,
     BtoD0 = 0x42324430,
     BtoD1 = 0x42324431,
     BtoD2 = 0x42324432,
     
     rXYZ = 0x7258595a,
     gXYZ = 0x6758595a,
     bXYZ = 0x6258595a,
     wtpt = 0x77747074,
     
     rTRC = 0x72545243,
     gTRC = 0x67545243,
     bTRC = 0x62545243,
     kTRC = 0x6B545243,
}
public static class TransformPicker
{
     private static TransformationNames[] perceptualPcsToDevice =
          { TransformationNames.BtoD0, TransformationNames.BtoA0 };
     private static TransformationNames[] perceptualDeviceToPcs =
          { TransformationNames.DtoB0, TransformationNames.AtoB0 };
     private static TransformationNames[] colorimentricPcsToDevice =
          { TransformationNames.BtoD1, TransformationNames.BtoA1, TransformationNames.BtoA0 };
     private static TransformationNames[] colorimentricDeviceToPcs =
          { TransformationNames.DtoB1, TransformationNames.AtoB1, TransformationNames.AtoB0 };
     private static TransformationNames[] saturationPcsToDevice =
          { TransformationNames.BtoD2, TransformationNames.BtoA2, TransformationNames.BtoA0 };
     private static TransformationNames[] saturationDeviceToPcs =
          { TransformationNames.DtoB2, TransformationNames.AtoB2, TransformationNames.AtoB0 };
     public static IColorTransform? PcsToDeviceTransform(this IccProfile profile, RenderIntent intent) =>
          TryWrapFromLabConversion(profile.Header.ProfileConnectionColorSpace, 
               SelectTransform(profile.Tags, PcsToDevicePreferences(intent)));

     private static TransformationNames[] PcsToDevicePreferences(RenderIntent intent) =>
          intent switch
          {
               RenderIntent.Perceptual =>  perceptualPcsToDevice,
               RenderIntent.MediaColorimentric or RenderIntent.IccColorimentric => colorimentricPcsToDevice,
               RenderIntent.Saturation => saturationPcsToDevice,
               _ => throw new ArgumentOutOfRangeException(nameof(intent), intent, null)
          };
     public static IColorTransform? DeviceToPcsTransform(this IccProfile profile, RenderIntent intent) =>
          TryWrapToLabConversion(profile.Header.ProfileConnectionColorSpace, SelectIccProfileStyle(profile, intent));

     private static IColorTransform? SelectIccProfileStyle(IccProfile profile, RenderIntent intent) =>
          SelectTransform(profile.Tags, DeviceToPcsPreferences(intent)) ??
          new TrcTransformParser(profile).Create();
     
     private static IColorTransform? TryWrapToLabConversion(ColorSpace profile, IColorTransform? innerTransform) => 
          ShouldWrapLabTransform(profile, innerTransform) ? 
               new ToLabConversion(innerTransform): 
               innerTransform;
     private static IColorTransform? TryWrapFromLabConversion(ColorSpace profile, IColorTransform? innerTransform) => 
          ShouldWrapLabTransform(profile, innerTransform) ? 
               new FromLabConversion(innerTransform): 
               innerTransform;


     private static bool ShouldWrapLabTransform(
          ColorSpace profile, [NotNullWhen(true)] IColorTransform? innerTransform) =>
          profile == ColorSpace.Lab && innerTransform is not null;

     private static TransformationNames[] DeviceToPcsPreferences(RenderIntent intent) =>
          intent switch
          {
               RenderIntent.Perceptual =>  perceptualDeviceToPcs,
               RenderIntent.MediaColorimentric or RenderIntent.IccColorimentric => colorimentricDeviceToPcs,
               RenderIntent.Saturation => saturationDeviceToPcs,
               _ => throw new ArgumentOutOfRangeException(nameof(intent), intent, null)
          };

     private static IColorTransform? SelectTransform(
          IReadOnlyList<ProfileTag> tags, TransformationNames[] preferences)
     {
          // this is an N squared for-if search loop which is terribly inefficient, however I expect
          // color profiles to have on the order of 10 to 15 tags, so I really do not care about the
          // inefficiency
          foreach (var preference in preferences)
          {
               foreach (var tag in tags)
               {
                    if (tag.Tag == (uint)preference && tag.Data is IColorTransform ret) return ret;
               }
          }
          return tags
               .Select(i=>i.Data)
               .OfType<IColorTransform>()
               .FirstOrDefault(i => i is not NullColorTransform);
     }
     
}