using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GuildfordBoroughCouncil.ActiveDirectory.Api.Lookup;
using GuildfordBoroughCouncil.Linq;
using System.Web.Http.Description;

namespace GuildfordBoroughCouncil.ActiveDirectory.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("directory/v1/services")]
    public class ServicesController : ApiController
    {
        [HttpGet]
        [Route]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult ServiceUnitNames(bool? IncludeXmt = false)
        {
            return Ok(Lookup.Data.ServiceUnitNames(IncludeXmt));
        }
    }
}