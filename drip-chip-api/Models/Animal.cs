namespace drip_chip_api.Models
{
    using Newtonsoft.Json.Linq;
    using NuGet.Protocol;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;
    using System.Text.Json;

    [Table("animals")]
    public class Animal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [NotMapped]
        public int[]? animalTypes { get; set; }
        public float? weight { get; set; }
        public float? length { get; set; }
        public float? height { get; set; }
        public string? gender { get; set; }
        public string? lifeStatus { get; set; }
        public DateTime chippingDateTime { get; set; }
        public int chipperId { get; set; }
        public long chippingLocationId { get; set; }
        [NotMapped]
        public int[]? visitedLocations { get; set; }
        public DateTime? deathDateTime { get; set; }
    }

    public class ChangeAnimalTypeValues
    {
        public int oldTypeId { get; set; }
        public int newTypeId { get; set; }
    }

    public class ChangeVisitedLocationValues
    {
        public int visitedLocationPointId { get; set; }
        public int locationPointId { get; set; }
    }

    public class CreateAnimalTypeValues
    {
        public string? type { get; set; }
    }

    public class UpdateAnimalTypeValues
    {
        public string? type { get; set;}
    }
}
