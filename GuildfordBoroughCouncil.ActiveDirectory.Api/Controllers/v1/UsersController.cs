using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using GuildfordBoroughCouncil.ActiveDirectory.Api.Lookup;
using System.Web.Http.Description;

namespace GuildfordBoroughCouncil.ActiveDirectory.Api.Controllers.v1
{
    [AllowAnonymous]
    [RoutePrefix("v1/users")]
    public class UsersController : ApiController
    {
        [HttpGet]
        [Route("councillors")]
        [ResponseType(typeof(IEnumerable<User>))]
        public IHttpActionResult Councillors(string q = null)
        {
            return Ok(Lookup.Data.Councillors(q));
        }

        [HttpGet]
        [Route("heads-of-service")]
        [ResponseType(typeof(IEnumerable<User>))]
        public IHttpActionResult HeadsOfService(string term = null)
        {
            return Ok(Lookup.Data.FindHeadsOfService(term));
        }

        [HttpGet]
        [Route]
        [ResponseType(typeof(IEnumerable<User>))]
        public IHttpActionResult FindUsers(string q = null, bool rq = false)
        {
            IEnumerable<User> Users = null;

            if (q == null)
            {
                Users = Lookup.Data.AllUsers();
            }
            else
            {
                Users = Lookup.Data.FindUsers(q);
            }

            if (rq)
            {
                var User = new User();
                User.Name = q;
                User.Title = "Click here if the user is not listed below.";
                User.Office = string.Empty;
                User.PhotoUrl = Properties.Settings.Default.NoPhotoImageUrl;
                Users = Users.Concat(new User[] { User });
            }

            return Ok(Users);
        }

        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(User))]
        public IHttpActionResult Users(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                return Ok(Lookup.Data.Users(new string[] { id }).SingleOrDefault());
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("{id}/groups")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult UserGroups(string id, bool follow = true)
        {
            return Ok(Lookup.Data.GroupSamAccountNamesForUser(id, follow));
        }

        [HttpGet]
        [Route("{id}/reports")]
        [ResponseType(typeof(IEnumerable<User>))]
        public IHttpActionResult DirectReports(string id)
        {
            return Ok(Lookup.Data.DirectReports(id));
        }
    }
}