﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace bosch_api.SignalRHub
{
    public interface IBoschApiHubClient
    {
        Task NewIn(int cameraId);
        Task NewOut(int cameraId);
        Task CrowdDensityChanged(int cameraId, int density);
        Task LatestCaptureChanged(FileStreamResult latestCapture);
    }
}
