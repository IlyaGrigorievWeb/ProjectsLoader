using Contracts.Entities;
using Microsoft.EntityFrameworkCore;

namespace Storages.EntitiesStorage;
public class PostgresContext : DbContext
{
    public DbSet<GitHubProject> GitHubProjects { get; set; }
    public DbSet<User> Users { get; set; }

    public PostgresContext(DbContextOptions<PostgresContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

}