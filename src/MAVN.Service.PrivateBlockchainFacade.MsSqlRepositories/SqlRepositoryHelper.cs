using System;
using System.IO;
using System.Reflection;
using Common.Log;
using Lykke.Common.Cache;
using Lykke.Common.Log;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using Microsoft.Extensions.Caching.Memory;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories
{
    public class SqlRepositoryHelper : ISqlRepositoryHelper
    {
        private const string ScriptsFolder = "SQL";

        private readonly OnDemandDataCache<string> _resourcesCache;
        private readonly ILog _log;
        private readonly Assembly _assembly;

        public SqlRepositoryHelper(IMemoryCache memoryCache, ILogFactory logFactory)
        {
            _resourcesCache = new OnDemandDataCache<string>(memoryCache);
            _log = logFactory.CreateLog(this);
            _assembly = Assembly.GetExecutingAssembly();
        }

        public string LoadSqlFromResource(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException();

            return _resourcesCache.GetOrAdd(filename, key =>
            {
                try
                {
                    var resourceName = $"{_assembly.GetName().Name}.{ScriptsFolder}.{key}";

                    var resourceStream = _assembly.GetManifestResourceStream(resourceName);

                    using (var streamReader = new StreamReader(resourceStream))
                    {
                        var sqlText = streamReader.ReadToEnd();

                        return sqlText;
                    }
                }
                catch (Exception e)
                {
                    _log.Error(
                        e,
                        "Could not read sql from resource",
                        new {assembly = _assembly.FullName, ScriptsFolder, filename = key });

                    return null;
                }
                // todo: temporary solution before we move to business transactions.
            }, DateTime.UtcNow.AddDays(1));
        }
    }
}
