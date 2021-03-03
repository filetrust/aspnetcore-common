using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Common.Storage.FileSystem.Store;
using Moq;
using NUnit.Framework;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreEncryptionTests.ConstructorTests.HandleReadAsync
{
    [TestFixture]
    public class WhenReadingWithEncryptionEnabledButFileIsNotEncrypted : FileStoreEncryptionTestBase
    {
        private MemoryStream _inputStream;
        private MemoryStream _outputStream;
        private byte[] _expectedBytes;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();
            var encryptedStreamContent = Encoding.UTF8.GetBytes("thensomedatawoopwoop");
            _expectedBytes = Encoding.UTF8.GetBytes("thensomedatawoopwoop");

            _inputStream = new MemoryStream(encryptedStreamContent);

            Encryption.Setup(s => s.DecryptAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_expectedBytes);

            ClassInTest = new FileStoreEncryption(Options.Object);

            _outputStream = await ClassInTest.HandleReadAsync(_inputStream, CancellationToken);
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            _inputStream.Dispose();
        }

        [Test]
        public void Output_Stream_Is_Correct()
        {
            CollectionAssert.AreEqual(_expectedBytes, _outputStream.ToArray());
        }

        [Test]
        public void Decryption_Is_Not_Called()
        {
            Encryption.VerifyNoOtherCalls();
        }
    }
}