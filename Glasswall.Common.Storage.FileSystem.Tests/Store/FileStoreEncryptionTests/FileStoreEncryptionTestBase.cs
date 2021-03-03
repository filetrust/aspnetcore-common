using System;
using System.IO;
using System.Threading;
using Glasswall.Common.Security;
using Glasswall.Common.Storage.FileSystem.Store;
using Glasswall.Common.Storage.Store;
using Glasswall.Common.Test.NUnit;
using Moq;
using Polly;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreEncryptionTests
{
    public class FileStoreEncryptionTestBase : UnitTestBase<FileStoreEncryption>
    {
        protected string RootPath;
        protected Mock<IFileStoreOptions> Options;
        protected Mock<IEncryptionHandler> Encryption;
        protected CancellationToken CancellationToken;

        protected void SharedSetup(string rootPath = null)
        {
            rootPath ??= $".{Path.DirectorySeparatorChar}{Guid.NewGuid()}";
            RootPath = rootPath;
            
            Options = new Mock<IFileStoreOptions>();
            Encryption = new Mock<IEncryptionHandler>();

            CancellationToken = new CancellationToken(false);

            Options.Setup(s => s.RootPath).Returns(RootPath);
            Options.Setup(s => s.RetryPolicy).Returns(Policy.Handle<Exception>().RetryAsync());
            Options.Setup(s => s.EncryptionHandler).Returns(Encryption.Object);

            ClassInTest = new FileStoreEncryption(Options.Object);
        }
    }
}