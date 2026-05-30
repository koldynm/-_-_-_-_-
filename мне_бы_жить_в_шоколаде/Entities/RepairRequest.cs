using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;

namespace мне_бы_жить_в_шоколаде.Entities
{
    

    // Модель для заявки на ремонт
    [Table("repair_requests")]
    public class RepairRequest : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("equipment_id")]
        public Guid EquipmentId { get; set; }

        [Column("requester_id")]
        public Guid RequesterId { get; set; }

        [Column("technician_id")]
        public Guid? TechnicianId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("status")]
        public string Status { get; set; } // Можно использовать string или Enum

        [Column("priority")]
        public string Priority { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("closed_at")]
        public DateTime ClosedAt { get; set; }

        [Reference(typeof(Equipment))]
        public Equipment Equipment { get; set; }

        [Column("deadline")]
        public DateTime? Deadline { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }
    }

    
}
