using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Glasswall.Common.Security
{
    public interface IEncryptionHandler
    {
        Task<IEnumerable<byte>> EncryptAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken);
        Task<IEnumerable<byte>> DecryptAsync(byte[] data, byte[] key, byte[] iv, CancellationToken cancellationToken);
    }
}