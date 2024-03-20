namespace drip_chip_api.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("points")]
    public class Point
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public DateTime dateTimeOfVisitLocationPoint { get; set; }
        public long locationPointId { get; set; }

    }
}
