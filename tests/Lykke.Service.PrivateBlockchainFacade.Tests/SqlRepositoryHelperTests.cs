using System;
using Lykke.Logs;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class SqlRepositoryHelperTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void LoadSqlFromResource_InvalidParameters_RaisesException(string filename)
        {
            var sut = new SqlRepositoryHelper(new MemoryCache(new MemoryCacheOptions()), EmptyLogFactory.Instance);

            Assert.Throws<ArgumentNullException>(() => sut.LoadSqlFromResource(filename));
        }

        [Theory]
        [InlineData("filename_does_not_exist")]
        public void LoadSqlFromResource_ResourcePathDoesNotExist_Returns_Null(string filename)
        {
            var sut = new SqlRepositoryHelper(new MemoryCache(new MemoryCacheOptions()), EmptyLogFactory.Instance);

            var sql = sut.LoadSqlFromResource(filename);

            Assert.Null(sql);
        }
    }
}
