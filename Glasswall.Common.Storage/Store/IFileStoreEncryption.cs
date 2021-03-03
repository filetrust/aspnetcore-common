using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Glasswall.Common.Storage.Store
{
    public interface IFileStoreEncryption
    {
        Task<MemoryStream> HandleReadAsync(Stream streamToRead, CancellationToken cancellationToken);
        Task HandleWriteAsync(Stream streamToWriteTo, byte[] content, CancellationToken cancellationToken);
    }
}