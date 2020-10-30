using BoschApi.Entities.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bosch_api.SignalRHub
{
    public class BoschApiHub: Hub<IBoschApiHubClient>
    {
        private IConfiguration Configuration;
        private readonly ILogger<BoschApiHub> _logger;

        private ApplicationDbContext DbContext { get; set; }

        public BoschApiHub(IConfiguration configuration, ApplicationDbContext applicationDbContext, ILogger<BoschApiHub> logger)
        {
            Configuration = configuration;
            DbContext = applicationDbContext;
            _logger = logger;
        }

        public async Task BroadcastEntry(int cameraId)
        {
            await Clients.All.NewIn(cameraId);
        }

        public async Task BroadcastExit(int cameraId)
        {
            await Clients.All.NewOut(cameraId);
        }

        public async Task BroadcastCrowdDensityChanged(int cameraId, int density)
        {
            await Clients.All.CrowdDensityChanged(cameraId, density);
        }
    }
}
