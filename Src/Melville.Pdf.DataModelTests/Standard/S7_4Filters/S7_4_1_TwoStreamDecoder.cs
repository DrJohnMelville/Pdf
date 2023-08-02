using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

public class S7_4_1_TwoStreamDecoder
{
    [Fact]
    public async Task Example3FromStandardAsync()
    {
        var stream = new DictionaryBuilder()
            .WithItem(KnownNames.Length, 562)
            .WithFilter(FilterName.ASCII85Decode, FilterName.FlateDecode)
            .AsStream(compressedData, StreamFormat.DiskRepresentation);

        var str = await stream.StreamContentAsync();
        var output = await new StreamReader(str).ReadToEndAsync();
        Assert.Equal(expandedData.Replace("\r\n","\r"), output);
    }

    private const string expandedData = """
        2 J
        BT
        /F1 12 Tf
        0 Tc
        0 Tw
        72.5 712 TD
        [ (Unfiltered streams can be read easily) 65 (, ) ] TJ
        0 -14 TD
        [ (b) 20 (ut generally tak) 10 (e more space than \311 ) ] TJ
        T* (compressed streams.) Tj
        0 -28 TD
        [ (Se) 25 (v) 15 (eral encoding methods are a) 20 (v) 25 (ailable in PDF) 80 (. ) ] TJ
        0 -14 TD
        (Some are used for compression and others simply ) Tj
        T* [ (to represent binary data in an) 55 ( ASCII format. ) ] TJ
        T* (Some of the compression filters are \
        suitable) Tj
        T* (for both data and images, while others are \
        suitable only) Tj
        T* (for continuous-tone images. ) Tj
        ET
        
        
        """;

    private const string compressedData = """
        GhQ\j_/=ii'LfceJ%/-bPR=PCD-LeMX!Z7pmG\.BSJGfG1MH^`^(F$rh*;h>h/i!XMIl:?" o%Tk8W%<tN0"eRE==1.4N>=Cc@SiF+*s\3`
        snB>Y$n=^$I[!5nkk"\Wp1;WBoI"]#[bZ=Ou#c QqtW?@3^VI[CftgH<HWngT*U:T12>8lm%+Q=gI^i0+\=:@fc@rHeGG!&rPQYfZ>YdpV`
        eM._@ Z]46KR;P?sT-tkq-Xn4&]=EE2[h<FPBAePZJ9inNNF4R*)W-?\NH1C-GG&?=IC_>b%&++C2jo*B+a7eU&`hee<0<F%-Y]De8<t.o"
        7p,rtNedLjK.J`kSL@c;BDEq+;F1Y?O)>keNVh+-(1*S= CQ?fQ_I]jWNc*Qe2XR^R#;3:<$\m;6d>@C3egX,+W+VfZ]X4*3r5*tiejhQe:
        !'jq$^jfSIj%13FL
        """;
}