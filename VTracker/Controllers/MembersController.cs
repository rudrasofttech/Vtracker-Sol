using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using VTracker.Models;

namespace VTracker.Controllers
{
    [Authorize]
    public class MembersController : ApiController
    {
        private VisitTrackerContext db = new VisitTrackerContext();

        // GET: api/Members
        public MemberListDTO GetMembers([FromUri]int page = 1, [FromUri] int psize = 20)
        {
            int count = db.Members.Count();
            MemberListDTO result = new MemberListDTO();
            result.TotalPages = count > psize ? (db.Members.Count() / psize) : 1;
            if (page > result.TotalPages)
            {
                page = result.TotalPages;
            }
            else if (page < 1)
            {
                page = 1;
            }
            var query = db.Members.OrderBy(t => t.ID).Skip((page - 1) * psize).Take(psize);

            foreach (Member m in query.ToList())
            {
                m.Password = "";
                result.Members.Add(m);
            }
            result.Page = page;

            return result;
        }

        // GET: api/Members/5
        [ResponseType(typeof(Member))]
        public IHttpActionResult GetMember(int id)
        {
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return NotFound();
            }

            return Ok(member);
        }

        // PUT: api/Members/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutMember(int id, Member member)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != member.ID)
            {
                return BadRequest();
            }

            db.Entry(member).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Members
        [ResponseType(typeof(Member))]
        public IHttpActionResult PostMember(Member member)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Members.Add(member);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = member.ID }, member);
        }

        // DELETE: api/Members/5
        [ResponseType(typeof(Member))]
        public IHttpActionResult DeleteMember(int id)
        {
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return NotFound();
            }

            db.Members.Remove(member);
            db.SaveChanges();

            return Ok(member);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MemberExists(int id)
        {
            return db.Members.Count(e => e.ID == id) > 0;
        }
    }
}