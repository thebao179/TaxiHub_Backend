using Microsoft.EntityFrameworkCore;
using TripService.Models;

namespace TripService.DataAccess
{
    public class TripServiceContext: DbContext
    {
        private readonly string _connectionStrings;

        public TripServiceContext(string connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }

        public DbSet<Trip> Trip { get; set; }
        public DbSet<TripFeedback> TripFeedback { get; set; }
        public DbSet<TripRequest> TripRequest { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            optionsBuilder.UseNpgsql(_connectionStrings);
        }


    }
}
