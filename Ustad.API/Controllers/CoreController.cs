using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using Ustad.API.Variables;

namespace Ustad.API.Controllers
{
    [ApiController]
    [Route("api/core")]
    public class CoreController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly tVariables v = new tVariables();

        public CoreController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ResolveTenant moved to TenantController (api/core/tenant/resolve) to avoid duplicate route.
    }
}