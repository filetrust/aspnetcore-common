using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreTests.ReadAsync
{
    [TestFixture]
    public class GivenValidFile : FileStoreTestBase
    {
        private MemoryStream _output;
        private MemoryStream _ms;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            if (!Directory.Exists(RootPath)) Directory.CreateDirectory(RootPath);

            var relativePath = $"{Guid.NewGuid()}.txt";

            if (!File.Exists(relativePath))
                await File.WriteAllTextAsync($"{RootPath}{Path.DirectorySeparatorChar}{relativePath}", "some text",
                    CancellationToken);

            Encryption.Setup(f => f.HandleReadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_ms = new MemoryStream(await File.ReadAllBytesAsync($"{RootPath}{Path.DirectorySeparatorChar}{relativePath}")));

            _output = await ClassInTest.ReadAsync(relativePath, CancellationToken);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            Directory.Delete(RootPath, true);
            _ms.Dispose();
        }

        [Test]
        public void Output_Is_File_Contents()
        {
            using (_output)
            {
                var str = Encoding.UTF8.GetString(_output.ToArray());

                Assert.That(str, Is.EqualTo("some text"));
            }
        }
    }
}