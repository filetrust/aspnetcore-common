using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Common.Security;
using Glasswall.Common.Storage.Store;

namespace Glasswall.Common.Storage.FileSystem.Store
{
    public class FileStoreEncryption : IFileStoreEncryption
    {
        public const string EncryptedMarker = "ThisFileIsEncrypted";
        private const int InitializationVectorLength = 16;
        private readonly byte[] _encryptedMarkerBytes = Encoding.UTF8.GetBytes(EncryptedMarker);

        private readonly IFileStoreOptions _fileStoreOptions;
        private readonly IEncryptionHandler _encryptionHandler;

        public FileStoreEncryption(IFileStoreOptions fileStoreOptions, IEncryptionHandler encryptionHandler)
        {
            _fileStoreOptions = fileStoreOptions ?? throw new ArgumentNullException(nameof(fileStoreOptions));
            _encryptionHandler = encryptionHandler;
        }

        public async Task<MemoryStream> HandleReadAsync(Stream streamToRead, CancellationToken cancellationToken)
        {
            await using (var ms = new MemoryStream())
            {
                await streamToRead.CopyToAsync(ms, (int) streamToRead.Length, cancellationToken);

                if (_encryptionHandler == null) return ms;

                return await ReadEncrypted(ms, cancellationToken);
            }
        }

        public async Task HandleWriteAsync(Stream streamToWriteTo, MemoryStream content, CancellationToken cancellationToken)
        {
            byte[] bytes;

            if (_encryptionHandler != null) bytes = await GetEncryptedBytes(content.GetBuffer(), cancellationToken);
            else bytes = content.GetBuffer();

            await streamToWriteTo.WriteAsync(bytes, cancellationToken);
        }

        private async Task<MemoryStream> ReadEncrypted(MemoryStream origContent, CancellationToken cancellationToken)
        {
            var fileContents = origContent.ToArray();

            if (_encryptedMarkerBytes.Where((t, i) => fileContents[i] != t).Any()) return origContent;

            var saltBytes = fileContents.Skip(_encryptedMarkerBytes.Length).Take(InitializationVectorLength);

            var data = await _encryptionHandler.DecryptAsync(
                fileContents.Skip(_encryptedMarkerBytes.Length).Skip(InitializationVectorLength).ToArray(),
                _fileStoreOptions.EncryptionSecret,
                saltBytes.ToArray(),
                cancellationToken);

            return new MemoryStream(data.ToArray());
        }

        private async Task<byte[]> GetEncryptedBytes(byte[] bytes, CancellationToken cancellationToken)
        {
            var saltBytes = EncryptionUtils.GenerateSalt(InitializationVectorLength);

            var data = await _encryptionHandler.EncryptAsync(
                bytes,
                _fileStoreOptions.EncryptionSecret,
                saltBytes,
                cancellationToken);

            return _encryptedMarkerBytes.Concat(saltBytes).Concat(data).ToArray();
        }
    }
}