using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql;
using Pgvector.EntityFrameworkCore;

namespace Raven.DB.PSQL
{
    public class AppDbContext : DbContext
    {
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Tags> Tags { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<CommentContent> CommentContent { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<TagsPosts> TagsPosts { get; set; }
        public DbSet<PostContent> PostContents { get; set; }
        public DbSet<Logs> Logs { get; set; }

        public AppDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql("Host=postgres;Port=5432;Database=TestDb;Username=admin;Password=p;Include Error Detail=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categories>()
                .HasMany(o => o.Posts)
                .WithOne(o => o.CategoryPost)
                .HasForeignKey(o => o.CategoryId);
            modelBuilder.Entity<Users>()
                .HasMany(o => o.Posts)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.AuthorId);
            modelBuilder.Entity<Users>()
                .HasMany(o => o.Comments)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.AuthorId);
            modelBuilder.Entity<Comments>()
                .HasOne(o => o.User)
                .WithMany(o => o.Comments)
                .HasForeignKey(o => o.AuthorId);
            modelBuilder.Entity<Comments>()
                .HasMany(o => o.CommentContents)
                .WithOne(o => o.Comment)
                .HasForeignKey(o => o.CommentId);
            modelBuilder.Entity<CommentContent>()
                .HasOne(o => o.Comment)
                .WithMany(o => o.CommentContents)
                .HasForeignKey(o => o.CommentId);
            modelBuilder.Entity<Posts>()
                .HasOne(o => o.User)
                .WithMany(o => o.Posts)
                .HasForeignKey(o => o.AuthorId);
            modelBuilder.Entity<Posts>()
                .HasMany(o => o.PostContents)
                .WithOne(o => o.Post)
                .HasForeignKey(o => o.PostId);
            modelBuilder.Entity<Posts>()
                .HasOne(o => o.CategoryPost)
                .WithMany(o => o.Posts)
                .HasForeignKey(o => o.CategoryId);
            modelBuilder.Entity<PostContent>()
                .HasOne(o => o.Post)
                .WithMany(o => o.PostContents)
                .HasForeignKey(o => o.PostId);

            modelBuilder.Entity<PostContent>().HasKey(o => new { o.PostId, o.ContentId });
            modelBuilder.Entity<CommentContent>().HasKey(o => new { o.CommentId, o.ContentId });
            modelBuilder.Entity<TagsPosts>().HasKey(o => new { o.TagId, o.PostId });
        }
    }
}
