using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Common.Storage.Store;
using Microsoft.Extensions.Logging;
using Polly;

namespace Glasswall.Common.Storage.FileSystem.Store
{
    public class FileStore : IFileStore
    {
        private readonly ILogger<FileStore> _logger;
        private readonly IFileStoreOptions _fileStoreOptions;
        private readonly IFileStoreEncryption _encryption;
        private readonly AsyncPolicy _retryer;

        public FileStore(
            ILogger<FileStore> logger,
            IFileStoreOptions fileStoreOptions,
            IFileStoreEncryption encryption)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileStoreOptions = fileStoreOptions ?? throw new ArgumentNullException(nameof(fileStoreOptions));
            _retryer = _fileStoreOptions.RetryPolicy;
            _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));
        }

        public IAsyncEnumerable<string> SearchAsync(string relativePath, IPathActions pathActions, CancellationToken cancellationToken)
        {
            if (pathActions == null) throw new ArgumentNullException(nameof(pathActions));
            return InternalSearchAsync(Path.Combine(_fileStoreOptions.RootPath, relativePath), pathActions, cancellationToken);
        }

        public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException("Value must not be null or whitespace", nameof(relativePath));
            var fullPath = Path.Combine(_fileStoreOptions.RootPath, relativePath);
            return _retryer.ExecuteAsync(async () => await Task.FromResult(Directory.Exists(fullPath) || File.Exists(fullPath)));
        }

        public Task<MemoryStream> ReadAsync(string relativePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException("Value must not be null or whitespace", nameof(relativePath));
            var fullPath = Path.Combine(_fileStoreOptions.RootPath, relativePath);
            return _retryer.ExecuteAsync(async() => await InternalReadAsync(fullPath, cancellationToken));
        }

        public Task WriteAsync(string relativePath, byte[] bytes, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException("Value must not be null or whitespace", nameof(relativePath));
            var fullPath = Path.Combine(_fileStoreOptions.RootPath, relativePath);
            return _retryer.ExecuteAsync(async() => await InternalWriteAsync(fullPath, bytes, cancellationToken));
        }

        public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException("Value must not be null or whitespace", nameof(relativePath));
            return _retryer.ExecuteAsync(async() => await InternalDeleteAsync(Path.Combine(_fileStoreOptions.RootPath, relativePath)));
        }

        private static Task InternalDeleteAsync(string fullPath)
        {
            if (Directory.Exists(fullPath)) Directory.Delete(fullPath, true);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            return Task.CompletedTask;
        }

        private async Task InternalWriteAsync(string fullPath, byte[] bytes, CancellationToken cancellationToken)
        {
            var dir = Path.GetDirectoryName(fullPath) ?? throw new ArgumentException("A directory was not specified", nameof(fullPath));
            
            Directory.CreateDirectory(dir);

            if (File.Exists(fullPath)) File.Delete(fullPath);

            await using var fw = File.OpenWrite(fullPath);
            await using var ms = new MemoryStream(bytes);
            await _encryption.HandleWriteAsync(fw, ms, cancellationToken);
        }

        private async Task<MemoryStream> InternalReadAsync(string path, CancellationToken cancellationToken)
        {
            if (!File.Exists(path)) return null;
            await using var fs = File.OpenRead(path);
            return await _encryption.HandleReadAsync(fs, cancellationToken);
        }
        

        private async IAsyncEnumerable<string> InternalSearchAsync(
            string directory,
            IPathActions pathActions,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            _logger.LogInformation("Searching relativePath '{0}'", directory);

            var (subFiles, subDirectories) = await _retryer.ExecuteAsync(() => Task.FromResult(GetFilesAndDirectories(directory)));

            foreach (var subDirectory in subDirectories)
            {
                var relativePath = Collect(subDirectory);
                var action = pathActions.DecideAction(relativePath);

                switch (action)
                {
                    case PathAction.Recurse:
                        await foreach (var subItem in InternalSearchAsync(subDirectory, pathActions, cancellationToken))
                            yield return subItem;
                        break;
                    case PathAction.Collect:
                        yield return relativePath;
                        break;
                    case PathAction.Break:
                        yield break;
                }
            }

            foreach (var subFile in subFiles)
            {
                var relativePath = Collect(subFile);
                var action = pathActions.DecideAction(relativePath);

                if (action == PathAction.Collect) yield return relativePath;
            }
        }

        private static (string[], string[]) GetFilesAndDirectories(string directory)
        {
            return (Directory.GetFiles(directory), Directory.GetDirectories(directory));
        }

        private string Collect(string path)
        {
            return path.Replace(_fileStoreOptions.RootPath, "").TrimStart(Path.DirectorySeparatorChar);
        }
    }
}