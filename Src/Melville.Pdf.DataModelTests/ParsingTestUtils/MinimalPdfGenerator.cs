﻿namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public class MinimalPdfGenerator
    {
        public static string MinimalPdf(int major, int minor) =>
            $@"%PDF-{major}.{minor}
1 0 obj
<< /Type /Catalog /Outlines 2 0 R /Pages 3 0 R >>
endobj
2 0 obj
<< /Type Outlines /Count 0 >>
endobj
3 0 obj
<< /Type /Pages /Kids [4 0 R] /Count 1 >>
endobj
4 0 obj
<< /Type /Page /Parent 3 0 R /MediaBox [0 0 612 792] /Contents 5 0 R /Resources << /ProcSet 6 0 R >> >>
endobj
5 0 obj
<< /Length 35 >>
stream
… Page-marking operators …
endstream 
endobj
6 0 obj
[/PDF]
endobj
xref
0 7
0000000000 65535 f 
0000000009 00000 n 
0000000074 00000 n 
0000000119 00000 n 
0000000176 00000 n 
0000000295 00000 n 
0000000376 00000 n 
trailer 
<< /Size 7 /Root 1 0 R >>
startxref
416
%%EOF";
    }
}