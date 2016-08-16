using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using GuildfordBoroughCouncil.ActiveDirectory.Api.Lookup;
using System.Web.Http.Description;

namespace GuildfordBoroughCouncil.ActiveDirectory.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("v1/groups")]
    public class GroupController : ApiController
    {
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(Group))]
        public IHttpActionResult Details(string id)
        {
            var group = Lookup.Data.AllGroups().Where(g => g.UserNameX == id).SingleOrDefault();

            if (group != null)
            {
                return Ok(group);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("{id}/members")]
        [ResponseType(typeof(IEnumerable<User>))]
        public IHttpActionResult GroupMembers(string id)
        {
            return Ok(Lookup.Data.GroupMembers(id));
        }

        [HttpGet]
        [Route]
        [ResponseType(typeof(IEnumerable<Group>))]
        public IHttpActionResult FindGroups(string q = null)
        {
            IEnumerable<Group> Groups = null;

            if (q == null)
            {
                Groups = Lookup.Data.AllGroups();
            }
            else
            {
                Groups = Lookup.Data.FindGroups(q);
            }
            
            return Ok(Groups);
        }
    }
}