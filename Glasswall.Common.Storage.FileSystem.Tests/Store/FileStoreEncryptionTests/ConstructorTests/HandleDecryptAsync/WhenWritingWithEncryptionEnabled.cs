using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Common.Security;
using Glasswall.Common.Storage.FileSystem.Store;
using Moq;
using NUnit.Framework;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreEncryptionTests.ConstructorTests.HandleDecryptAsync
{
    [TestFixture]
    public class WhenWritingWithEncryptionEnabled : FileStoreEncryptionTestBase
    {
        private Stream _inputStream;
        private byte[] _expectedBytes;
        private Mock<IEncryptionHandler> _encrypter;
        private byte[] _encryptedBytes;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            _expectedBytes = Encoding.UTF8.GetBytes("thensomedatawoopwoop");
            _encryptedBytes = Encoding.UTF8.GetBytes("ThisFileIsEncryptedthensomedatawoopwoop");

            _inputStream = File.Open("test.txt", FileMode.Create);
            _encrypter = new Mock<IEncryptionHandler>();
            
            _encrypter.Setup(s => s.EncryptAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_encryptedBytes);

            Options.Setup(s => s.EncryptionHandler).Returns(_encrypter.Object);

            ClassInTest = new FileStoreEncryption(Options.Object);

            await ClassInTest.HandleWriteAsync(_inputStream, _expectedBytes, CancellationToken);
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            _inputStream.Dispose();
        }

        [Test]
        public void Stream_Writes_Correct_Data()
        {
            using var ms = new MemoryStream();
            _inputStream.Position = 0;
            _inputStream.CopyTo(ms);
            Assert.That(Encoding.UTF8.GetString(ms.ToArray()).StartsWith("ThisFileIsEncrypted"));
        }

        [Test]
        public void Encrypt_Is_Called()
        {
            _encrypter.Verify(s => s.EncryptAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(),
                It.IsAny<CancellationToken>()), Times.Once);
            Encryption.VerifyNoOtherCalls();
        }
    }
}