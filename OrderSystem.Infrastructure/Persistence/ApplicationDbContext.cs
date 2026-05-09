using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Identity;
using OrderSystem.Application.Interfaces;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace OrderSystem.Infrastructure.Persistence
{
    public class SpResult
    {
        public int Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SpResult> SpResults { get; set; } // Keyless entity

        public async Task<(bool Success, string Message)> ApproveOrderAsync(int orderId, string performedBy, int newStatus, string comments)
        {
            var orderIdParam = new Microsoft.Data.SqlClient.SqlParameter("@OrderId", orderId);
            var performedByParam = new Microsoft.Data.SqlClient.SqlParameter("@PerformedBy", performedBy);
            var newStatusParam = new Microsoft.Data.SqlClient.SqlParameter("@NewStatus", newStatus);
            var commentsParam = new Microsoft.Data.SqlClient.SqlParameter("@Comments", comments ?? (object)DBNull.Value);

            var result = await this.Set<SpResult>().FromSqlRaw("EXEC sp_ApproveOrder @OrderId, @PerformedBy, @NewStatus, @Comments", 
                orderIdParam, performedByParam, newStatusParam, commentsParam).ToListAsync();

            var first = result.FirstOrDefault();
            return (first?.Success == 1, first?.Message ?? "Unknown error");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SpResult>().HasNoKey();

            builder.Entity<Order>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.RowVersion).IsRowVersion();
            });

            builder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(d => d.Order)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.OrderId);
            });
        }
    }
}
