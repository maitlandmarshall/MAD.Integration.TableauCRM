using Microsoft.EntityFrameworkCore;

namespace MAD.Integration.TableauCRM.Data
{
    public class ConfigurationDbContext : DbContext
    {
        public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : base(options)
        {
        }

        public DbSet<Configuration> Configuration { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Configuration>(cfg =>
            {
                cfg.HasKey(x => x.Id);
                cfg.Property(y => y.Id).ValueGeneratedOnAdd();

                cfg.Property(y => y.DestinationTableName).IsRequired();
                cfg.Property(y => y.TableName).IsRequired();                              
            });
        }

    }
}
