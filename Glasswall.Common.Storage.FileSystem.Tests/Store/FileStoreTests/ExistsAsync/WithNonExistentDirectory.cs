﻿using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreTests.ExistsAsync
{
    [TestFixture]
    public class WithNonExistentDirectory : FileStoreTestBase
    {
        private bool _output;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            if (!Directory.Exists(RootPath)) Directory.CreateDirectory(RootPath);

            _output = await ClassInTest.ExistsAsync("SOME OTHER PATH", CancellationToken);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            Directory.Delete(RootPath, true);
        }

        [Test]
        public void True_Is_Returned()
        {
            Assert.That(_output, Is.False);
        }
    }
}