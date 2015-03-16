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
using Visitora.Model;
using Visitora.Model.DTOs;

namespace Visitora.Controllers
{
    public class VisitsController : ApiController
    {
        private Entities db = new Entities();

        // GET: api/Visits
        public IQueryable<Visit> GetVisits()
        {
            return db.Visits;
        }

        [Route("v1/users/{user}/visits")]
        public IQueryable<VisitDTO> GetUserVisits(string user)
        {
            var visits = from visit in db.Visits
                         where String.Concat(visit.User.First_Name, " ", visit.User.Last_Name) == user
                         select new VisitDTO
                         {
                             UserName = String.Concat(visit.User.First_Name, " ", visit.User.Last_Name),
                             CityName = visit.City.name,
                             StateName = visit.City.State.Name,
                         };

            return visits.OrderByDescending(v=>v.CityName).AsQueryable();
        }

        [Route("v1/users/{user}/visits")]
        public IHttpActionResult PostVisit(string user, CityDTO city)
        {

            Visit visitObj = ConvertToVisitObject(user, city);
            int changes = 0;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (visitObj != null && !VisitExists(visitObj.User_Id, visitObj.City_Id))
                {
                    db.Visits.Add(visitObj);
                    changes = db.SaveChanges();
                }
                else
                {
                    return Conflict();
                }

            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Json((changes>0)? true:false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool VisitExists(int userId, int cityId)
        {
            return db.Visits.Count(e => e.User_Id == userId && e.City_Id == cityId) > 0;
        }

        private Visit ConvertToVisitObject(string user, CityDTO city)
        {
            Visit visitObj;
            try
            {
                visitObj = new Visit();
                visitObj.Id = db.Visits.Count() + 1;
                visitObj.City_Id = db.Cities.Single(c => c.State.Name == city.StateName && c.name == city.Name).Id;
                String userFirstName = user.Split(' ')[0];
                String userLastName = user.Split(' ')[1];
                visitObj.User_Id = db.Users.Where(u => u.First_Name == userFirstName && u.Last_Name == userLastName).First().id;
            }
            catch(Exception)
            {
                throw;
            }

            return visitObj;
        }
    }
}