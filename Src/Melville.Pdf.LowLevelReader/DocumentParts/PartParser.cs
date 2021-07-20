using System.Threading.Tasks;
using Melville.FileSystem;

namespace Melville.Pdf.LowLevelReader.DocumentParts
{
    public interface IPartParser
    {
        Task<DocumentPart[]> Parse(IFile source);
    }
    public class PartParser: IPartParser
    {
        public Task<DocumentPart[]> Parse(IFile source)
        {
            throw new System.NotImplementedException();
        }
    }
}