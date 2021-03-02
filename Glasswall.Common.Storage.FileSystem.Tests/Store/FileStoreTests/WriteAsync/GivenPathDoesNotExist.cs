using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreTests.WriteAsync
{
    [TestFixture]
    public class GivenPathDoesNotExist : FileStoreTestBase
    {
        private byte[] _expectedBytes;
        private string _fullPath;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            var relativePath = $"{Guid.NewGuid()}/{Guid.NewGuid()}.txt";
            _fullPath = $"{RootPath}{Path.DirectorySeparatorChar}{relativePath}";

            Encryption.Setup(s =>
                    s.HandleWriteAsync(It.IsAny<Stream>(), It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()))
                .Callback((Stream fs, MemoryStream ms, CancellationToken ct) =>
                {
                    fs.WriteAsync(ms.ToArray(), ct).GetAwaiter().GetResult();
                });

            await ClassInTest.WriteAsync(relativePath, _expectedBytes = new byte[] { 0x00, 0x11 }, CancellationToken);
        }

        [Test]
        public void Encryption_Is_Invoked()
        {
            Encryption.Verify(s => s.HandleWriteAsync(It.IsAny<Stream>(), It.IsAny<MemoryStream>(), It.IsAny<CancellationToken>()));
            Encryption.VerifyNoOtherCalls();
        }

        [Test]
        public void File_Is_Written()
        {
            Assert.That(File.ReadAllBytes(_fullPath), Has.Exactly(2).Items);
        }
    }
}