using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.TrainingData.Entity;

namespace Raven.DB.PSQL.TrainingData
{
    public class AppDbContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTags> PostTags { get; set; }

        public AppDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql("Host=postgres-training-data;Port=5432;Database=TrainingDb;Username=admin;Password=p;Include Error Detail=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostTags>().HasKey(o => new {o.TagId, o.PostId});
        }
    }
}
