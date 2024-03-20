using drip_chip_api.Context;
using drip_chip_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using System.Globalization;
using System.Net.Mime;
using static drip_chip_api.Models.Animal;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace drip_chip_api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class animalsController : ControllerBase
    {
        private readonly Context.DBContext dbContext;
        public Authorization authorization = new Authorization();

        public animalsController(Context.DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        static Boolean ValidateDateTimeString(string datetime)
        {
            if(datetime == null)
            {
                return true;
            }

            DateTime result = new DateTime();
            if (DateTime.TryParseExact(datetime, "yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return true;
            else
                return false;
        }

        // GET api/<animalsController>/search
        [HttpGet("search")]
        public ActionResult<Animal[]> GetSearchAnimal(int? chipperId, long? chippingLocationId, string? lifeStatus, string? gender, int size = 10, int from = 0)
        {
            if (Request.Headers.Authorization.ToString() != "")
            {
                if (!authorization.isAuthorized(Request.Headers.Authorization))
                {
                    return Unauthorized();
                }
            }

            if (size <= 0 || from < 0)
            {
                return BadRequest();
            }

            var animals = dbContext.Animals.ToList();

            if (chipperId != null)
            {
                animals = animals.Where(animal => animal.chipperId == chipperId).ToList();
            }
            if (chippingLocationId != null)
            {
                animals = animals.Where(animal => animal.chippingLocationId == chippingLocationId).ToList();
            }
            if (lifeStatus != null)
            {
                animals = animals.Where(animal => animal.lifeStatus == lifeStatus).ToList();
            }
            if (gender != null)
            {
                animals = animals.Where(animal => animal.gender == gender).ToList();
            }
            if (size > animals.Count)
            {
                return animals.ToArray();
            }

            return animals.ToArray()[from..^(size - 1)];
        }

        // GET api/<animalsController>/5
        [HttpGet("{id}")]
        public ActionResult<Animal> GetAnimalId(long id)
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

            var animal = dbContext.Animals.Where(animal => animal.id == id).ToArray();
            if (animal.Length == 0)
            {
                return NotFound();
            }

            return animal[0];
        }

        [HttpPost]
        public ActionResult<Animal> CreateAnimal(Animal animal) 
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if(animal.animalTypes.Count() == 0 || animal.weight <= 0 || animal.length <= 0 || animal.height <= 0 || animal.chipperId <= 0 || animal.chippingLocationId <= 0 || animal.animalTypes.Count() <= 0 || (animal.gender != "MALE" && animal.gender != "FEMALE" && animal.gender != "OTHER") )
            {
                return BadRequest();
            }

            for(int i = 0; i < animal.animalTypes.Count();i++)
            {
                if (dbContext.AnimalTypes.Where(animalType => animalType.id == animal.animalTypes.ToArray()[i]).Count() == 0)
                {
                    return NotFound();
                }
            }

            if(dbContext.Accounts.Where(account => account.id == animal.chipperId).Count() == 0 || dbContext.Locations.Where(location => location.id == animal.chippingLocationId).Count() == 0)
            {
                return NotFound();
            }

            if(animal.animalTypes.Count() != animal.animalTypes.Distinct().Count())
            {
                return Conflict();
            }

            animal.chippingDateTime = DateTime.Now;
            animal.lifeStatus = "ALIVE";

            dbContext.Animals.Add(animal);
            dbContext.SaveChanges();

            return StatusCode(201, animal);
        }

        [HttpPut("{id}")]
        public ActionResult<Animal> UpdateAnimal([FromBody] Animal animal, int id) 
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (id <= 0 || (animal.lifeStatus != "ALIVE" && animal.lifeStatus != "DEAD") || animal.weight <= 0 || animal.length <= 0 || animal.height <= 0 || animal.chipperId <= 0 || animal.chippingLocationId <= 0 || (animal.gender != "MALE" && animal.gender != "FEMALE" && animal.gender != "OTHER"))
            {
                return BadRequest();
            }

            var updateAnimal = dbContext.Animals.Where(updateAnimal => updateAnimal.id == id).ToList();
            if (dbContext.Accounts.Where(account => account.id == animal.chipperId).Count() == 0 || dbContext.Locations.Where(location => location.id == animal.chippingLocationId).Count() == 0 || updateAnimal.Count() == 0)
            {
                return NotFound();
            }

            if (updateAnimal[0].lifeStatus != animal.lifeStatus)
            {
                updateAnimal[0].deathDateTime= DateTime.Now;
            }

            updateAnimal[0].weight = animal.weight;
            updateAnimal[0].height = animal.height;
            updateAnimal[0].gender = animal.gender;
            updateAnimal[0].lifeStatus= animal.lifeStatus;
            updateAnimal[0].chippingLocationId= animal.chippingLocationId;
            updateAnimal[0].chipperId = animal.chipperId;

            dbContext.SaveChanges();

            return updateAnimal[0];
        }

        [HttpDelete("{id}")]
        public ActionResult RemoveAnimal(int id)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return BadRequest();
            }

            var animal = dbContext.Animals.Where(animal => animal.id == id).ToList();

            if (animal.Count() == 0)
            {
                return NotFound();
            }

            if (animal[0].visitedLocations != null)
            {
                return BadRequest();
            }

            dbContext.Animals.Remove(animal[0]);
            dbContext.SaveChanges();
               
            return Ok();
        }

        [HttpGet("types/{id}")]
        public ActionResult<AnimalType> GetAnimalTypeById(long id)
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

            var animalType = dbContext.AnimalTypes.Where(animalType => animalType.id == id).ToArray();
            if (animalType.Length == 0)
            {
                return NotFound();
            }

            return animalType[0];
        }

        [HttpPost("types")]
        public ActionResult<AnimalType> CreateAnimalType([FromBody] CreateAnimalTypeValues createAnimalTypeValues)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (createAnimalTypeValues.type == null || createAnimalTypeValues.type.Trim().Length == 0)
            {
                return BadRequest();
            }

            if(dbContext.AnimalTypes.Where(animalType => animalType.type == createAnimalTypeValues.type).ToArray().Length > 0)
            {
                return Conflict();
            }

            var newType = new AnimalType{ type = createAnimalTypeValues.type };
            dbContext.AnimalTypes.Add(newType);
            dbContext.SaveChanges();

            return StatusCode(201, newType);
        }

        [HttpPut("types/{id}")]
        public ActionResult<AnimalType> UpdateAnimalType([FromBody] UpdateAnimalTypeValues updateAnimalTypeValues, int id)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (updateAnimalTypeValues.type == null || updateAnimalTypeValues.type.Trim().Length == 0 || id <= 0)
            {
                return BadRequest();
            }

            if(dbContext.AnimalTypes.Where(animalType => animalType.id == id ).ToArray().Length == 0)
            {
                return NotFound();
            }

            if(dbContext.AnimalTypes.Where(animalType => animalType.type == updateAnimalTypeValues.type).ToArray().Length != 0)
            {
                return Conflict();
            }

            var animalType = dbContext.AnimalTypes.Where(animalType => animalType.id == id).ToList();
            animalType[0].type = updateAnimalTypeValues.type;

            dbContext.SaveChanges();

            return animalType[0];
        }

        [HttpDelete("types/{id}")]
        public ActionResult DeleteAnimalType(int id)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return BadRequest();
            }

            if (dbContext.AnimalTypes.Where(animalType => animalType.id == id).ToArray().Length == 0)
            {
                return NotFound();
            }

            var animals = dbContext.Animals.ToArray();

            for(int i = 0; i < animals.Length; i++)
            {
                if (animals[i].animalTypes != null)
                {
                    for (int j = 0; j < animals[i].animalTypes.Count(); j++)
                    {
                        if (animals[i].animalTypes.ToArray()[j] == id)
                        {
                            return BadRequest();
                        }
                    }
                }
            }


            var animalType = dbContext.AnimalTypes.Where(animalType => animalType.id == id).ToList();
            dbContext.AnimalTypes.Remove(animalType[0]);
            dbContext.SaveChanges();

            return Ok();
        }

        [HttpGet("{id}/locations")]
        public ActionResult<Animal> GetAnimalLocationById(long id, string? startDateTime, string? endDateTime, int size = 10, int from = 0)
        {
            if (Request.Headers.Authorization.ToString() != "")
            {
                if (!authorization.isAuthorized(Request.Headers.Authorization))
                {
                    return Unauthorized();
                }
            }

            if (id <= 0 || ValidateDateTimeString(startDateTime) != true || ValidateDateTimeString(endDateTime) != true || size <= 0 || from < 0)
            {
                return BadRequest();
            }

            var animalType = dbContext.Animals.Where(animalType => animalType.id == id).ToArray();
            if (animalType.Length == 0)
            {
                return NotFound();
            }

            return animalType[0];
        }

        [HttpPost("{animalId}/locations/{pointId}")]
        public ActionResult<Point> AddLocation(int animalId, int pointId)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if(animalId <= 0 || pointId <= 0)
            {
                return BadRequest();
            }

            var animal = dbContext.Animals.Where(animal => animal.id == animalId).ToList();
            if (animal.Count() == 0 || dbContext.Locations.Where(location => location.id == pointId).ToArray().Length == 0)
            {
                return NotFound();
            }

            if (animal[0].lifeStatus == "DEAD" || animal[0].chippingLocationId == pointId)
            {
                return BadRequest();
            }

            if (animal[0].visitedLocations == null)
            {
                animal[0].visitedLocations = new int[] { pointId };
            }
            else
            {
                if (animal[0].visitedLocations.Last() == pointId)
                {
                    return BadRequest();
                }
                else
                {
                    animal[0].visitedLocations.ToList().Add(pointId);
                }
            }

            var point = new Point { dateTimeOfVisitLocationPoint = DateTime.Now, locationPointId = pointId };
            dbContext.Points.Add(point);

            dbContext.SaveChanges();

            return StatusCode(201, point);
        }

        [HttpPut("{animalId}/locations")]
        public ActionResult<Location> ChangeVisitedLocation(int animalId, [FromBody] ChangeVisitedLocationValues changeVisitedLocationValues) 
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if(animalId <= 0 || changeVisitedLocationValues.visitedLocationPointId <= 0 || changeVisitedLocationValues.locationPointId <= 0)
            {
                return BadRequest();
            }

            var animal = dbContext.Animals.Where(animal => animal.id == animalId).ToList();
            if (animal.Count() == 0 || animal[0].animalTypes == null || dbContext.Locations.Where(location => location.id == changeVisitedLocationValues.visitedLocationPointId).ToArray().Length == 0 || dbContext.Locations.Where(location => location.id == changeVisitedLocationValues.locationPointId).ToArray().Length == 0 || !Array.Exists(animal.ToArray()[0].visitedLocations, e => e == changeVisitedLocationValues.visitedLocationPointId))
            {
                return NotFound();
            }

            if ( changeVisitedLocationValues.visitedLocationPointId == changeVisitedLocationValues.locationPointId) 
            {
                return BadRequest();
            }

            for(int i = 0; i < animal[0].visitedLocations.Length; i++)
            {
                if (animal[0].visitedLocations[i] == changeVisitedLocationValues.visitedLocationPointId)
                {
                    if(i+1 < animal[0].visitedLocations.Length && animal[0].visitedLocations[i + 1] == changeVisitedLocationValues.locationPointId)
                    {
                        return BadRequest();
                    }

                    if (i - 1 >= 0 && animal[0].visitedLocations[i - 1] == changeVisitedLocationValues.locationPointId)
                    {
                        return BadRequest();
                    }
                }
            }

            for (int i = 0; i < animal[0].visitedLocations.Length; i++)
            {
                if (animal[0].visitedLocations[i] == changeVisitedLocationValues.visitedLocationPointId)
                {
                    animal[0].visitedLocations[i] = changeVisitedLocationValues.locationPointId;
                }
            }

            dbContext.SaveChanges();

            return Ok();
        }

        [HttpDelete("{animalId}/locations/{visitedPointId}")]
        public ActionResult DeleteAnimalVisitedLocation(int animalId, int visitedPointId) 
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (animalId <= 0 || visitedPointId <= 0)
            {
                return BadRequest();
            }

            var animal = dbContext.Animals.Where(animal => animal.id == animalId).ToList();
            if (animal.Count() == 0 || animal[0].animalTypes == null ||  dbContext.Locations.Where(location => location.id == visitedPointId).ToArray().Length == 0 || !Array.Exists(animal.ToArray()[0].visitedLocations, e => e == visitedPointId))
            {
                return NotFound();
            }

            animal[0].visitedLocations = animal[0].visitedLocations.Where(location => location != visitedPointId).ToArray();

            if (animal[0].visitedLocations.Length == 1 && animal[0].visitedLocations[0] == animal[0].chippingLocationId)
            {
                animal[0].visitedLocations = Array.Empty<int>();
            }

            dbContext.SaveChanges();

            return Ok();
        }

        [HttpPost("{animalId}/types/{typeId}")]
        public ActionResult<Animal> AddTypeToAnimal(long animalId, int typeId)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (animalId <= 0 || typeId <= 0)
            {
                return BadRequest();
            }

            var animal = dbContext.Animals.Where(animal => animal.id == animalId).ToList();
            if(animal.Count() == 0 || animal[0].animalTypes == null || dbContext.AnimalTypes.Where(type => type.id == typeId).ToList().Count() == 0)
            {
                return NotFound();
            }

            if (Array.Exists(animal.ToArray()[0].animalTypes.ToArray(), e => e == typeId))
            {
                return Conflict();
            }

            animal[0].animalTypes.ToList().Add(typeId);
            dbContext.SaveChanges();

            return StatusCode(201, animal);
        }

        [HttpPut("{id}/types")]
        public ActionResult<Animal> ChangeAnimalType(int id, [FromBody] ChangeAnimalTypeValues changeAnimalTypeValues)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if(id <= 0 || changeAnimalTypeValues.oldTypeId <= 0 || changeAnimalTypeValues.newTypeId <= 0) 
            { 
                return BadRequest(); 
            }

            var animal = dbContext.Animals.Where(animal => animal.id == id).ToList();
            if(animal.Count() == 0 || animal[0].animalTypes == null || dbContext.AnimalTypes.Where(type => type.id == changeAnimalTypeValues.oldTypeId).ToList().Count() == 0 || dbContext.AnimalTypes.Where(type => type.id == changeAnimalTypeValues.newTypeId).ToList().Count() == 0 || !Array.Exists(animal.ToArray()[0].animalTypes.ToArray(), e => e == changeAnimalTypeValues.oldTypeId))
            {
                return NotFound();
            }

            if(Array.Exists(animal.ToArray()[0].animalTypes.ToArray(), e => e == changeAnimalTypeValues.newTypeId))
            {
                return Conflict();
            }

            for(int i = 0; i < animal[0].animalTypes.Count(); i++)
            {
                if (animal[0].animalTypes.ToArray()[i] == changeAnimalTypeValues.oldTypeId)
                {
                    animal[0].animalTypes.ToArray()[i] = changeAnimalTypeValues.newTypeId;
                }
            }

            dbContext.SaveChanges();

            return animal[0];
        }

        [HttpDelete("{animaId}/types/{typeId}")]
        public ActionResult<Animal> DeleteAnimalType(int animalId, int typeId)
        {
            if (!authorization.isAuthorized(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            if (animalId <= 0 || typeId <= 0)
            {
                return BadRequest();
            }

            var animal = dbContext.Animals.Where(animal => animal.id == animalId).ToList();
            if (animal.Count() == 0 || dbContext.AnimalTypes.Where(type => type.id == typeId).ToList().Count() == 0 || Array.Exists(animal.ToArray()[0].animalTypes.ToArray(), e => e == typeId))
            {
                return NotFound();
            }

            animal[0].animalTypes = animal[0].animalTypes.Where(type => type != typeId).ToArray();
            dbContext.SaveChanges();

            return Ok();
        }

    }
}
