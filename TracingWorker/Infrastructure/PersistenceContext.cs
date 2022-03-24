using Microsoft.EntityFrameworkCore;
using TracingWorker.Domain.Entities;

namespace TracingWorker.Infrastructure
{
    public class PersistenceContext : DbContext
    {

        public PersistenceContext(DbContextOptions<PersistenceContext> options) : base(options) { }

        public DbSet<Person> Person { get; set; } = default!;

        public async Task CommitAsync()
        {
            await SaveChangesAsync().ConfigureAwait(false);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                return;
            }

            //modelBuilder.Entity<Person>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
