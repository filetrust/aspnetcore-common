using System.IO;
using System.Text;
using System.Threading.Tasks;
using Glasswall.Common.Security;
using Glasswall.Common.Storage.FileSystem.Store;
using NUnit.Framework;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreEncryptionTests.ConstructorTests.HandleReadAsync
{
    [TestFixture]
    public class WhenReadingWithEncryptionDisabled : FileStoreEncryptionTestBase
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

            Options.Setup(s => s.EncryptionHandler).Returns(null as IEncryptionHandler);

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