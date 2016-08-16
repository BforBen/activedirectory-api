using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

namespace GuildfordBoroughCouncil.ActiveDirectory.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("v1/services")]
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