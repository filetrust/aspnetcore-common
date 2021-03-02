using System;
using System.IO;
using System.Threading;
using Glasswall.Common.Storage.FileSystem.Store;
using Glasswall.Common.Storage.Store;
using Glasswall.Common.Test.NUnit;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreTests
{
    public class FileStoreTestBase : UnitTestBase<FileStore>
    {
        protected string RootPath;
        protected Mock<ILogger<FileStore>> Logger;
        protected Mock<IFileStoreOptions> Options;
        protected Mock<IFileStoreEncryption> Encryption;
        protected CancellationToken CancellationToken;

        protected void SharedSetup(string rootPath = null)
        {
            rootPath ??= $".{Path.DirectorySeparatorChar}{Guid.NewGuid()}";
            RootPath = rootPath;
            
            Logger = new Mock<ILogger<FileStore>>();
            Options = new Mock<IFileStoreOptions>();
            Encryption = new Mock<IFileStoreEncryption>();

            CancellationToken = new CancellationToken(false);

            Options.Setup(s => s.RootPath).Returns(RootPath);
            Options.Setup(s => s.RetryPolicy).Returns(Policy.Handle<Exception>().RetryAsync());
            
            ClassInTest = new FileStore(
                Logger.Object, 
                Options.Object, 
                Encryption.Object);
        }
    }
}