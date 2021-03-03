using System;
using Castle.Core.Logging;
using Glasswall.Common.Security;
using Glasswall.Common.Storage.FileSystem.Store;
using Glasswall.Common.Storage.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Polly;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Glasswall.Common.Storage.FileSystem.Tests
{
    [TestFixture]
    public class DependencyInjectionExtensionsTests
    {
        private Mock<IFileStoreOptions> _options;

        [SetUp]
        public void Setup()
        {
            _options = new Mock<IFileStoreOptions>();

            _options.Setup(s => s.RetryPolicy)
                .Returns(Policy.Handle<Exception>().RetryAsync());

            _options.Setup(s => s.RootPath)
                .Returns("/mnt/test");
        }

        [Test]
        public void Can_Resolve_When_Multiple_Registered()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<ILogger<FileStore>, Logger<FileStore>>();
            services.AddTransient<ILoggerFactory, NullLoggerFactory>();

            var secondOptions = new Mock<IFileStoreOptions>();

            secondOptions.Setup(s => s.RetryPolicy)
                .Returns(Policy.Handle<Exception>().RetryAsync());

            secondOptions.Setup(s => s.RootPath)
                .Returns("/mnt/test");

            services.AddFileStore(_options.Object);
            services.AddFileStore(secondOptions.Object);

            var sp = services.BuildServiceProvider();

            Assert.That(sp.GetServices<IFileStore>(), Has.Length.EqualTo(2));
        }

        [Test]
        public void Can_Resolve_When_Single_Registered()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<ILogger<FileStore>, Logger<FileStore>>();
            services.AddTransient<ILoggerFactory, NullLoggerFactory>();

            services.AddFileStore(_options.Object);

            var sp = services.BuildServiceProvider();

            Assert.That(sp.GetServices<IFileStore>(), Has.Length.EqualTo(1));
        }

        [Test]
        public void Throws_With_Null_Services()
        {
            IServiceCollection services = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.That(() => services.AddFileStore(_options.Object), Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("services"));
        }

        [Test]
        public void Throws_With_Null_Options()
        {
            IServiceCollection services = new ServiceCollection();

            Assert.That(() => services.AddFileStore(null), Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("fileStoreOptions"));
        }

        [Test]
        public void Throws_With_Null_Store_Root_Path()
        {
            IServiceCollection services = new ServiceCollection();

            _options.Setup(s => s.RootPath).Returns(null as string);

            Assert.That(() => services.AddFileStore(_options.Object), Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("fileStoreOptions"));
        }

        [Test]
        public void Throws_With_Null_Retry_Policy()
        {
            IServiceCollection services = new ServiceCollection();

            _options.Setup(s => s.RetryPolicy).Returns(null as AsyncPolicy);

            Assert.That(() => services.AddFileStore(_options.Object), Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("fileStoreOptions"));
        }

        [Test]
        public void Throws_With_Null_EncryptionSecret_And_NonNull_EncryptionHandler()
        {
            IServiceCollection services = new ServiceCollection();

            _options.Setup(s => s.EncryptionSecret).Returns(null as byte[]);
            _options.Setup(s => s.EncryptionHandler).Returns(Mock.Of<IEncryptionHandler>());

            Assert.That(() => services.AddFileStore(_options.Object), Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("fileStoreOptions"));
        }
    }
}
