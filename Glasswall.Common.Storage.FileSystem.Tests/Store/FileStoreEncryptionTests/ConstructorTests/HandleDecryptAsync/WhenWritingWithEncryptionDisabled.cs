using System.IO;
using System.Text;
using System.Threading.Tasks;
using Glasswall.Common.Security;
using Glasswall.Common.Storage.FileSystem.Store;
using NUnit.Framework;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreEncryptionTests.ConstructorTests.HandleDecryptAsync
{
    [TestFixture]
    public class WhenWritingWithEncryptionDisabled : FileStoreEncryptionTestBase
    {
        private Stream _inputStream;
        private byte[] _expectedBytes;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            _expectedBytes = Encoding.UTF8.GetBytes("thensomedatawoopwoop");

            _inputStream = File.Open("test.txt", FileMode.Create);

            Options.Setup(s => s.EncryptionHandler).Returns(null as IEncryptionHandler);

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
            CollectionAssert.AreEqual(ms.ToArray(), _expectedBytes);
        }

        [Test]
        public void Encrypt_Is_Not_Called()
        {
            Encryption.VerifyNoOtherCalls();
        }
    }
}