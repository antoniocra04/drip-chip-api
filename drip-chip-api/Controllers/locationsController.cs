using drip_chip_api.Context;
using drip_chip_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using System.Net.Mime;
using System.Security.Principal;
using static drip_chip_api.Models.Location;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace drip_chip_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class locationsController : ControllerBase
    {
        private readonly Context.DBContext dbContext;
        public Authorization authorization = new Authorization();

        public locationsController(Context.DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // GET api/<locationsController>/5
        [HttpGet("{id}")]
        public ActionResult<Location> GetLocationById(long id)
        {

            if (Request.Headers.Authorization.ToString() != "")
            {
                if (!authorization.isAuthorized(Request.Headers.Authorization))
                {
                    return Unauthorized();
                }
            }

            if (id <= 0)
            {
                return BadRequest();
            }

            var location = dbContext.Locations.Where(location => location.id == id).ToArray();
            if (location.Length == 0)
            {
                return NotFound();
            }

            return location[0];
        }

        [HttpPost]
        public ActionResult<Location> CreateLocation(Location location) {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (location.latitude == null || location.longitude == null || location.longitude < -180 || location.longitude > 180 || location.latitude < -90 || location.latitude > 90) {
                return BadRequest();
            }

            if (dbContext.Locations.Where(loc => loc.latitude == location.latitude && loc.longitude == location.longitude).ToArray().Length > 0)
            {
                return Conflict();
            }

            dbContext.Locations.Add(location);
            dbContext.SaveChanges();

            return StatusCode(201, location);
        }

        [HttpPut("{id}")]
        public ActionResult<Location> UpdateLocation(Location location, int id)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (id <= 0 || location.latitude == null || location.longitude == null || location.longitude < -180 || location.longitude > 180 || location.latitude < -90 || location.latitude > 90)
            {
                return BadRequest();
            }

            if (dbContext.Locations.Where(loc => loc.latitude == location.latitude && loc.longitude == location.longitude).ToArray().Length > 0)
            {
                return Conflict();
            }

            var updateLocation = dbContext.Locations.Where(loc => loc.id == id).ToList();
            if (updateLocation.Count == 0)
            {
                return NotFound();
            }

            updateLocation[0].latitude = location.latitude;
            updateLocation[0].longitude = location.longitude;

            dbContext.SaveChanges();

            return StatusCode(200, updateLocation[0]);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteLocation(int id)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return BadRequest();
            }

            var location = dbContext.Locations.Where(loc => loc.id == id).ToList();
            if (location.Count == 0)
            {
                return NotFound();
            }

            dbContext.Locations.Remove(location[0]);
            dbContext.SaveChanges();

            return Ok();
        }
    }
}
