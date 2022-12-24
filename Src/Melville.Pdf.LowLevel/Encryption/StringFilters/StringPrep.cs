using System;
using System.Data;
using System.Text;

namespace Melville.Pdf.LowLevel.Encryption.StringFilters;

internal static class StringPrep
{
    public static string SaslPrep(this string input)
    {
        return SaslCharacterMapping.MapChars(input).Normalize(NormalizationForm.FormKC);
    }

    public static string SaslPrepIfValid(this string input)
    {
        var ret = input.SaslPrep();
        if (!SaslValidator.IsValid(ret))
            throw new ArgumentException("Password is not SaslPrep Compliant");

        return ret;
    }
}