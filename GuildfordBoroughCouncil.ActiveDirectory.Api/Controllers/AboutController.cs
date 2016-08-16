using System;
using System.Web.Http;
using System.Web.Http.Description;

namespace GuildfordBoroughCouncil.ActiveDirectory.Api.Controllers
{
    /// <summary>
    /// About controller
    /// </summary>
    [RoutePrefix("directory")]
    public class DirectoryAboutController : ApiController
    {
        [HttpGet]
        [Route("_about")]
        [ResponseType(typeof(string))]
        public IHttpActionResult About()
        {
            var Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return Ok(String.Format("{0}.{1}.{2}", Ver.Major, Ver.Minor, Ver.Build));
        }
    }
}