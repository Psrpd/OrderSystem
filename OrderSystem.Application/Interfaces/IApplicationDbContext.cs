using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Order> Orders { get; set; }
        DbSet<AuditLog> AuditLogs { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        // Method for Stored Procedure
        Task<(bool Success, string Message)> ApproveOrderAsync(int orderId, string performedBy, int newStatus, string comments);
    }
}
