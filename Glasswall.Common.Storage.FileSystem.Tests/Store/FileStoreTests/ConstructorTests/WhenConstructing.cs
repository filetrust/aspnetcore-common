using Glasswall.Common.Storage.FileSystem.Store;
using Glasswall.Common.Test.NUnit;
using NUnit.Framework;

namespace Glasswall.Common.Storage.FileSystem.Tests.Store.FileStoreTests.ConstructorTests
{
    [TestFixture]
    public class WhenConstructing : UnitTestBase<FileStore>
    {
        [Test]
        public void Constructor_Is_Guarded_Against_Null()
        {
            ClassIsGuardedAgainstNull();
        }

        [Test]
        public void Constructor_Constructs_With_Mocked_Parameters()
        {
            ConstructsWithMockedParameters();
        }
    }
}