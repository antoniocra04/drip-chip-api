namespace drip_chip_api.Context
{
    using Microsoft.EntityFrameworkCore;
    using drip_chip_api.Models;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    public class DBContext
        :DbContext
    {
        public DBContext(DbContextOptions options)
            : base(options)
        {

        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<AnimalType> AnimalTypes { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Point> Points { get; set; }
    }
}
