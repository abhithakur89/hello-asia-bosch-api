using bosch_api.Helper;
using BoschApi.Entities.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace bosch_api.Controllers
{
    //[Route("api")]
    [ApiController]
    public class SiteController: ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<SiteController> _logger;
        private ApplicationDbContext Context { get; set; }

        private enum ResponseCodes
        {
            [Display(Name = "Successful")]
            Successful = 1200,
            [Display(Name = "Error")]
            SystemError = 1201,
        }

        public SiteController(IConfiguration configuration, ApplicationDbContext applicationDbContext, ILogger<SiteController> logger)
        {
            Configuration = configuration;
            Context = applicationDbContext;
            _logger = logger;
        }

        /// <summary>
        /// GetAllSites API. Returns all sites info.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /getallsites
        ///      
        /// Sample response:
        /// 
        ///     {
        ///         "respcode": 1200,
        ///         "description": "Successful",
        ///         "sites": [
        ///             {
        ///                 "siteId": 1,
        ///                 "siteName": "Novade Office",
        ///                 "siteDescription": "Novade Pte. Ltd. main office"
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
        [Route("getallsites")]
        public ActionResult GetAllSites()
        {
            try
            {
                _logger.LogInformation("GetAllSites() called from: " + HttpContext.Connection.RemoteIpAddress.ToString());

                var sites = Context.Sites
                    .Select(x => new { x.SiteId, x.SiteName, x.SiteDescription });

                _logger.LogInformation($"Returning sites: {JsonConvert.SerializeObject(sites)}");
                return new JsonResult(new
                {
                    respcode = ResponseCodes.Successful,
                    description = ResponseCodes.Successful.DisplayName(),
                    sites
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
