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
using Visitora.Common;
using Visitora.Model.DTOs;

namespace Visitora.Controllers
{
    public class StatesController : ApiController
    {
        private Entities db = new Entities();

        [Route("api/states/{state}/cities")]
        public IQueryable<CityDTO> GetStateCities(string state)
        {
            var cities = from city in db.Cities
                         where city.State.Name == state
                         select new CityDTO
                         {
                             Name = city.name,
                             StateName = city.State.Name
                         };

            return cities.OrderBy(c => c.Name).AsQueryable();
        }

        [Route("api/states/{state}/cities/{city}")]
        public IQueryable<CityDTO> GetCitiesInRadius(string state, string city, [FromUri] double radius)
        {
            City initialCity = db.Cities.Single(c=>c.State.Name == state && c.name == city);
            
            List<CityDTO> citiesInRanges = new List<CityDTO>();
            List<City> cities = db.Cities.ToList();
            foreach(City cityItem in cities)
            {
                if(GeoCodeCalc.CalcDistance(cityItem.latitude,cityItem.longitude,initialCity.latitude, initialCity.longitude) <= radius)
                {
                    citiesInRanges.Add(new CityDTO
                    {
                        Name = cityItem.name,
                        StateName = cityItem.State.Name
                    });
                }
            }
            return citiesInRanges.AsQueryable();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StateExists(int id)
        {
            return db.States.Count(e => e.Id == id) > 0;
        }
    }
}