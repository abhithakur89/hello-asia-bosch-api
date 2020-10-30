﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace bosch_api.SignalRHub
{
    public interface IBoschApiHub
    {
        Task BroadcastEntry(int cameraId);
        Task BroadcastExit(int cameraId);
        Task BroadcastCrowdDensityChanged(int cameraId, int density);
        Task BroadcastLatestCaptureChanged(FileStreamResult latestCapture)
    }
}
