using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Application.DTOs;
using OrderSystem.Application.Interfaces;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.Services
{
    public interface IOrderService
    {
        Task<(IEnumerable<OrderDto> Items, int TotalCount)> GetAllOrdersAsync(string? searchTerm = null, int page = 1, int pageSize = 10);
        Task<OrderDto> CreateOrderAsync(OrderCreateDto dto, string userId);
        Task<(bool Success, string Message)> ApproveOrderAsync(ApprovalDto dto, string performedBy, string userRole);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<OrderDto?> UpdateOrderAsync(int id, OrderUpdateDto dto);
        Task<bool> DeleteOrderAsync(int id);
        Task<IEnumerable<StatsDto>> GetOrderStatsAsync();
    }

    public class OrderService : IOrderService
    {
        private readonly IApplicationDbContext _context;

        public OrderService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<OrderDto> Items, int TotalCount)> GetAllOrdersAsync(string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => o.Description.Contains(searchTerm) || o.CreatedBy.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    Description = o.Description,
                    Amount = o.Amount,
                    Status = o.Status,
                    CreatedBy = o.CreatedBy,
                    CreatedDate = o.CreatedDate
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<OrderDto> CreateOrderAsync(OrderCreateDto dto, string userId)
        {
            var order = new Order
            {
                Description = dto.Description,
                Amount = dto.Amount,
                CreatedBy = userId,
                Status = OrderStatus.Pending
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return new OrderDto
            {
                Id = order.Id,
                Description = order.Description,
                Amount = order.Amount,
                Status = order.Status,
                CreatedBy = order.CreatedBy,
                CreatedDate = order.CreatedDate
            };
        }

        public async Task<(bool Success, string Message)> ApproveOrderAsync(ApprovalDto dto, string performedBy, string userRole)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) return (false, "Order not found.");

            // Business Rule: Orders > $10,000 require Admin approval
            if (order.Amount > 10000 && userRole != "Admin")
            {
                return (false, "Orders above $10,000 require Admin approval.");
            }

            // Call Stored Procedure for atomic update + audit log
            return await _context.ApproveOrderAsync(
                dto.OrderId, 
                performedBy, 
                dto.IsApproved ? (int)OrderStatus.Approved : (int)OrderStatus.Rejected, 
                dto.Comments);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                Description = order.Description,
                Amount = order.Amount,
                Status = order.Status,
                CreatedBy = order.CreatedBy,
                CreatedDate = order.CreatedDate
            };
        }

        public async Task<OrderDto?> UpdateOrderAsync(int id, OrderUpdateDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return null;

            order.Description = dto.Description;
            order.Amount = dto.Amount;

            await _context.SaveChangesAsync();

            return new OrderDto
            {
                Id = order.Id,
                Description = order.Description,
                Amount = order.Amount,
                Status = order.Status,
                CreatedBy = order.CreatedBy,
                CreatedDate = order.CreatedDate
            };
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<StatsDto>> GetOrderStatsAsync()
        {
            // LINQ for approval statistics and grouping
            var stats = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new StatsDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(o => o.Amount)
                })
                .ToListAsync();

            return stats;
        }
    }
}
