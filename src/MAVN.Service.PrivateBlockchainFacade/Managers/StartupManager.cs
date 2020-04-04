using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Sdk;

namespace MAVN.Service.PrivateBlockchainFacade.Managers
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private readonly IEnumerable<IStartStop> _startables;

        public StartupManager(IEnumerable<IStartStop> startables)
        {
            _startables = startables;
        }

        public Task StartAsync()
        {
            foreach (var startable in _startables)
            {
                startable.Start();
            }

            return Task.CompletedTask;
        }
    }
}
