using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;

namespace ChinaTower.StationPlanning.Models
{
    public class ChinaTowerContext : IdentityDbContext<User>
    {
        public DbSet<Blob> Blobs { get; set; }
        public DbSet<RxLevLine> RxLevLines { get; set; }
        public DbSet<Tower> Towers { get; set; }
        public DbSet<Form> Forms { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Blob>(e =>
            {
                e.HasIndex(x => x.Time);
                e.HasIndex(x => x.FileName);
            });

            builder.Entity<RxLevLine>(e =>
            {
                e.HasIndex(x => x.Time);
                e.HasIndex(x => x.City);
                e.HasIndex(x => x.BeginLat);
                e.HasIndex(x => x.BeginLon);
                e.HasIndex(x => x.EndLat);
                e.HasIndex(x => x.EndLon);
            });

            builder.Entity<Tower>(e =>
            {
                e.HasIndex(x => x.City);
                e.HasIndex(x => x.District);
                e.HasIndex(x => x.Lat);
                e.HasIndex(x => x.Lon);
                e.HasIndex(x => x.Type);
                e.HasIndex(x => x.Scene);
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.Name);
                e.HasIndex(x => x.Address);
                e.HasIndex(x => x.Provider);
                e.HasIndex(x => x.Time);
            });

            builder.Entity<Form>(e =>
            {
                e.HasIndex(x => x.Type);
                e.HasIndex(x => x.Time);
            });
        }
    }
}
