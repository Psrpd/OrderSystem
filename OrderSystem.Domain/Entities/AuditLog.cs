using System;

namespace OrderSystem.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Action { get; set; } = string.Empty; // e.g., "Approved", "Rejected"
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Comments { get; set; } = string.Empty;

        public virtual Order Order { get; set; } = null!;
    }
}
