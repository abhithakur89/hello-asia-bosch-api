using bosch_api.Helper;
using BoschApi.Entities.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using System.IO;
using bosch_api.SignalRHub;
using Microsoft.AspNetCore.SignalR.Client;

namespace bosch_api.Controllers
{
    //[Route("api")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<CameraController> _logger;
        private ApplicationDbContext Context { get; set; }
        private static object lockEntryObject = new object();
        private static object lockExitObject = new object();

        private enum ResponseCodes
        {
            [Display(Name = "Successful")]
            Successful = 1200,
            [Display(Name = "Error")]
            SystemError = 1201,
        }

        public CameraController(IConfiguration configuration, ApplicationDbContext applicationDbContext, ILogger<CameraController> logger)
        {
            Configuration = configuration;
            Context = applicationDbContext;
            _logger = logger;
        }

        /// <summary>
        /// GetAllSites API. Returns all cameras info.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /getallcameras?siteid=1
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200,
        ///         "description": "Successful",
        ///         "cameras": [
        ///             {
        ///                 "cameraId": 1,
        ///                 "cameraName": "Camera at Entrance",
        ///                 "cameraIP": "192.168.1.191",
        ///                 "model": "Bosch",
        ///                 "additionalDetail": "",
        ///                 "GateId": 1
        ///             },
        ///             {
        ///                 ...
        ///             }
        ///             ...
        ///         ]
        ///     }
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>
        [HttpGet]
        [Route("getallcameras")]
        public ActionResult GetAllCameras(int siteId)
        {
            try
            {
                _logger.LogInformation("GetAllCameras() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());

                //var received = new { SiteId = string.Empty };

                //received = JsonConvert.DeserializeAnonymousType(jsiteId.ToString(Formatting.None), received);

                //_logger.LogInformation($"Paramerters: {received.SiteId}");

                //if (!int.TryParse(received.SiteId, out int nSiteId)) throw new Exception("Invalid Site Id");

                var cameras = Context.Cameras
                    .Where(x => x.Gate.SiteId == siteId)
                    .Select(x => new
                    {
                        x.CameraId,
                        x.CameraName,
                        x.CameraIP,
                        x.Model,
                        x.AdditionalDetail,
                        x.GateId
                    });

                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                    description = ResponseCodes.Successful.DisplayName(),
                    cameras
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

        /// <summary>
        /// Entry API. Add new entry.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /addnewentry?cameraid=1
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200,
        ///     }
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>
        
        [HttpGet]
        [Route("addnewentry")]
        public ActionResult AddNewEntry(int cameraid)
        {
            try
            {
                _logger.LogInformation("AddNewEntry() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());
                DateTime dateTime = DateTime.UtcNow.ToTimezone(Configuration["Timezone"]);

                lock (lockEntryObject)
                {
                    EntryRecord entryRecord = new EntryRecord();
                    entryRecord.Timestamp = dateTime;
                    entryRecord.CameraId = cameraid;

                    Context.EntryRecords.Add(entryRecord);

                    var entryCount = Context.EntryCounts
                        .Where(x => x.CameraId == cameraid && x.Date == dateTime.Date)
                        ?.Select(x=>x)
                        ?.FirstOrDefault();

                    if (entryCount == null)
                    {
                        EntryCount newEntryCount = new EntryCount();
                        newEntryCount.Date = dateTime.Date;
                        newEntryCount.CameraId = cameraid;
                        newEntryCount.Count = 1;

                        Context.EntryCounts.Add(newEntryCount);
                        Context.SaveChangesAsync();
                    }
                    else
                    {
                        entryCount.Count = entryCount.Count + 1;
                        Context.SaveChangesAsync();
                    }

                    SignalRHubConnection.GetInstance(Configuration["SignalRHubUrl"])
                        .SendAsync("BroadcastEntry", cameraid);

                }

                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

        /// <summary>
        /// Entry API. Add new exit.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /addnewexit?cameraid=1
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200
        ///     }
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>

        [HttpGet]
        [Route("addnewexit")]
        public ActionResult AddNewExit(int cameraid)
        {
            try
            {
                _logger.LogInformation("AddNewExit() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());
                DateTime dateTime = DateTime.UtcNow.ToTimezone(Configuration["Timezone"]);

                lock (lockExitObject)
                {
                    ExitRecord exitRecord = new ExitRecord();
                    exitRecord.Timestamp = dateTime;
                    exitRecord.CameraId = cameraid;

                    Context.ExitRecords.Add(exitRecord);

                    var exitCount = Context.ExitCounts
                        .Where(x => x.CameraId == cameraid && x.Date == dateTime.Date)
                        ?.Select(x => x)
                        ?.FirstOrDefault();

                    if (exitCount == null)
                    {
                        ExitCount newExitCount = new ExitCount();
                        newExitCount.Date = dateTime.Date;
                        newExitCount.CameraId = cameraid;
                        newExitCount.Count = 1;

                        Context.ExitCounts.Add(newExitCount);
                        Context.SaveChangesAsync();
                    }
                    else
                    {
                        exitCount.Count = exitCount.Count + 1;
                        Context.SaveChangesAsync();
                    }
                }

                //BoschApiHub.BroadcastExit(cameraid);
                SignalRHubConnection.GetInstance(Configuration["SignalRHubUrl"])
                    .SendAsync("BroadcastExit", cameraid);

                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

        /// <summary>
        /// Read today's entries.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /gettodayentries?cameraid=1&recordcount=2
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200,
        ///         "count": 3,
        ///         "records": [
        ///             {
        ///                 "timestamp": "2020-09-13T14:47:25",
        ///                 "count": 2
        ///             },
        ///             {
        ///                 "timestamp": "2020-09-13T14:46:15",
        ///                 "count": 1
        ///             }
        ///         ]
        ///     }
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>

        [HttpGet]
        [Route("gettodayentries")]
        public ActionResult GetTodayEntries(int cameraid, int recordcount)
        {
            try
            {
                _logger.LogInformation("GetTodayEntries() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());
                DateTime dateTime = DateTime.UtcNow.ToTimezone(Configuration["Timezone"]);

                var count = Context.EntryCounts
                    .Where(x => x.Date == dateTime.Date && x.CameraId == cameraid)
                    ?.Select(x => x.Count)
                    ?.FirstOrDefault() ?? 0;

                var records = Context.EntryRecords
                    .Where(x => x.CameraId == cameraid && x.Timestamp.Date == dateTime.Date)
                    ?.GroupBy(x => x.Timestamp)
                    ?.Select(x => new { Timestamp = x.Key, Count = x.Count() })
                    ?.OrderByDescending(x => x.Timestamp)
                    ?.Take(recordcount)
                    ?.Select(x => new { Timestamp = x.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), x.Count });

                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                    count,
                    records
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

        /// <summary>
        /// Read today's exits.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /gettodayexits?cameraid=1&recordcount=10
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200,
        ///         "count": 3,
        ///         "records": [
        ///             {
        ///                 "timestamp": "2020-09-13T14:47:25",
        ///                 "count": 2
        ///             },
        ///             {
        ///                 "timestamp": "2020-09-13T14:46:15",
        ///                 "count": 1
        ///             }
        ///         ]
        ///     }
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>

        [HttpGet]
        [Route("gettodayexits")]
        public ActionResult GetTodayExits(int cameraid, int recordcount)
        {
            try
            {
                _logger.LogInformation("GetTodayExits() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());
                DateTime dateTime = DateTime.UtcNow.ToTimezone(Configuration["Timezone"]);

                var count = Context.ExitCounts
                    .Where(x => x.Date == dateTime.Date && x.CameraId == cameraid)
                    ?.Select(x => x.Count)
                    ?.FirstOrDefault() ?? 0;

                var records = Context.ExitRecords
                    .Where(x => x.CameraId == cameraid && x.Timestamp.Date == dateTime.Date)
                    ?.GroupBy(x => x.Timestamp)
                    ?.Select(x => new { Timestamp = x.Key, Count = x.Count() })
                    ?.OrderByDescending(x => x.Timestamp)
                    ?.Take(recordcount)
                    ?.Select(x => new { Timestamp = x.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), x.Count  });

                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                    count,
                    records
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

        /// <summary>
        /// Crowd API. Add crowd level.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /crowddensity?cameraid=1&crowdLevel=3
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200
        ///     }
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>

        [HttpGet]
        [Route("crowddensity")]
        public ActionResult CrowdDensity(int cameraid, int crowdLevel)
        {
            try
            {
                _logger.LogInformation("CrowdDensity() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());
                DateTime dateTime = DateTime.UtcNow.ToTimezone(Configuration["Timezone"]);


                {
                    CrowdDensityLevel crowdDensityLevel = new CrowdDensityLevel();
                    crowdDensityLevel.Timestamp = dateTime;
                    crowdDensityLevel.CameraId = cameraid;
                    crowdDensityLevel.Level = crowdLevel;

                    Context.CrowdDensityLevels.Add(crowdDensityLevel);
                    Context.SaveChangesAsync();
                }

                //BoschApiHub.BroadcastCrowdDensityChanged(cameraid, crowdLevel);
                SignalRHubConnection.GetInstance(Configuration["SignalRHubUrl"])
                    .SendAsync("BroadcastCrowdDensityChanged", cameraid, crowdLevel);

                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

        /// <summary>
        /// GetLatestCapture API. Get the latest screen capture.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /getlatestcapture?cameraid=1
        /// 
        /// Sample response:
        ///     Latest Captured picture
        ///     
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>

        [HttpGet]
        [Route("getlatestcapture")]
        public ActionResult GetLatestCapture()
        {
            try
            {
                _logger.LogInformation("GetLatestCapture() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());
                DateTime dateTime = DateTime.UtcNow.ToTimezone(Configuration["Timezone"]);

                DropboxClient dropboxClient = new DropboxClient(Configuration["DropboxToken"]);

                var filePath = DownloadLatestFile(dropboxClient).Result;

                var image = System.IO.File.OpenRead(filePath);
                return File(image, "image/jpeg");

            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

        private async Task<string> DownloadLatestFile(DropboxClient dbx)
        {
            bool hasMore = true;
            ListFolderResult list = null;

            while (hasMore)
            {
                if (list == null)
                    list = await dbx.Files.ListFolderAsync(Configuration["DropboxFolder"]);
                else
                    list = await dbx.Files.ListFolderContinueAsync(list.Cursor);

                hasMore = list.HasMore;
            }

            var v = list.Entries
                ?.Where(x => x.IsFile)
                ?.OrderByDescending(x => x.AsFile.ServerModified)
                ?.FirstOrDefault();

            string localFilePath = Path.Combine(System.IO.Path.GetTempPath(), v.Name);

            using (var response = await dbx.Files.DownloadAsync(v.PathLower))
            {
                using (var fileStream = System.IO.File.Create(localFilePath))
                {
                    (await response.GetContentAsStreamAsync()).CopyTo(fileStream);
                }
            }

            return localFilePath;
        }

        /// <summary>
        /// Get today's alarm level.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /gettodayalarmlevel?cameraid=1&recordcount=1
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200,
        ///         "records": [
        ///             {
        ///                 "timestamp": "2020-09-13T14:47:25",
        ///                 "level": 1
        ///             }
        ///         ]
        ///     }
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>

        [HttpGet]
        [Route("gettodayalarmlevel")]
        public ActionResult GetTodayAlarmLevel(int cameraid, int recordcount)
        {
            try
            {
                _logger.LogInformation("GetTodayAlarmLevel() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());
                DateTime dateTime = DateTime.UtcNow.ToTimezone(Configuration["Timezone"]);

                var records = Context.CrowdDensityLevels
                    .Where(x => x.CameraId == cameraid && x.Timestamp.Date == dateTime.Date)
                    ?.OrderByDescending(x => x.Timestamp)
                    ?.Take(recordcount)
                    ?.Select(x => new { Timestamp = x.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), x.Level });

                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                    records
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

        /// <summary>
        /// Get latest alarm level.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /getlatestalarmlevel?cameraid=1
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200,
        ///         "timestamp": "2020-09-13T14:47:25",
        ///         "level": "2"
        ///     }
        ///     
        /// Response codes:
        ///     1200 = "Successful"
        ///     1201 = "Error"
        /// </remarks>
        /// <returns>
        /// </returns>

        [HttpGet]
        [Route("getlatestalarmlevel")]
        public ActionResult GetLatestAlarmLevel(int cameraid)
        {
            try
            {
                _logger.LogInformation("GetLatestAlarmLevel() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());

                var record = Context.CrowdDensityLevels
                    .Where(x => x.CameraId == cameraid)
                    ?.OrderByDescending(x => x.Timestamp)
                    ?.Select(x => new { Timestamp = x.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"), x.Level })
                    ?.FirstOrDefault();

                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                    record.Timestamp,
                    record.Level
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Generic exception handler invoked. {e.Message}: {e.StackTrace}");

                return new JsonResult(new
                {
                    respcode = ResponseCodes.SystemError,
                    description = ResponseCodes.SystemError.DisplayName(),
                    Error = e.Message
                });
            }
        }

    }
}
