using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
