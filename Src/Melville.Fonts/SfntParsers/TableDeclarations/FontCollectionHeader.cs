using System.Buffers;

namespace Melville.Fonts.SfntParsers.TableDeclarations;

internal readonly partial struct FontCollectionHeader
{
    [SFntField] private readonly UInt32 tag;
    [SFntField] private readonly UInt16 majorVersion;
    [SFntField] private readonly UInt16 minorVersion;
    [SFntField] private readonly UInt32 numFonts; 
    [SFntField("this.numFonts")] private readonly UInt32[] tableDirectoryOffsets;
    //I ignore and do not read the dsigTag. dsigLength, and dsigOffset fields in type 2 collections
    // because verifying font digital signatures is not a goal of this project.
}