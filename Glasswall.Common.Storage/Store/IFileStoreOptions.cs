using Polly;

namespace Glasswall.Common.Storage.Store
{
    public interface IFileStoreOptions
    {
        string RootPath { get; }
        AsyncPolicy RetryPolicy { get; }
        byte[] EncryptionSecret { get; }
    }
}