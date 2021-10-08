using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public static class PdfArrayOperations
    {
        public static async ValueTask<T> GetAsync<T>(this PdfArray array, int index) where T: PdfObject =>
            (T) await array[index];

        public static ValueTask<PdfObject> GetOrNullAsync(this PdfArray array, int index) =>
            index < 0 || index >= array.Count ? new(PdfTokenValues.Null) : array[index];

    }
}