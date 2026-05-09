using System;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class OrderCreateDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class ApprovalDto
    {
        public int OrderId { get; set; }
        public bool IsApproved { get; set; }
        public string Comments { get; set; } = string.Empty;
    }

    public class StatsDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class OrderUpdateDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
