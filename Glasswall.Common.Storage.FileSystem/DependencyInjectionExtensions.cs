using Glasswall.Common.Storage.FileSystem.Store;
using Glasswall.Common.Storage.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Glasswall.Common.Storage.FileSystem
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds an instance of a file store to the service collection, call multiple times to add multiple stores.
        /// Each store is specified by its <see cref="fileStoreOptions"/>.
        /// </summary>
        /// <param name="services">Service collection to add services too</param>
        /// <param name="fileStoreOptions">The options describing the store</param>
        /// <returns>The Service Collection to chain calls</returns>
        public static IServiceCollection AddFileStore(this IServiceCollection services, IFileStoreOptions fileStoreOptions)
        {
            return services.AddTransient<IFileStore>(sp => new FileStore(sp.GetRequiredService<ILogger<FileStore>>(), fileStoreOptions, new FileStoreEncryption(fileStoreOptions)));
        }
    }
}
