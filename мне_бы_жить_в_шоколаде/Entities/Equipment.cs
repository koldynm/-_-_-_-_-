using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace мне_бы_жить_в_шоколаде.Entities
{
    [Table("equipment")]
    public class Equipment : BaseModel
    {
        [PrimaryKey("id", false)] // false, так как ID генерируется на стороне БД (gen_random_uuid)
        public Guid Id { get; set; }

        [Column("inventory_number")]
        public string InventoryNumber { get; set; }

        [Column("type_id")]
        public Guid? TypeId { get; set; }

        [Column("model")]
        public string Model { get; set; }

        [Column("serial_number")]
        public string SerialNumber { get; set; }

        [Column("location_id")]
        public Guid? LocationId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("photo_url")]
        public string PhotoUrl { get; set; }

        // --- СВЯЗИ (Для Join запросов) ---

        [Reference(typeof(Location))]
        public Location Location { get; set; }

        [Reference(typeof(EquipmentType))]
        public EquipmentType EquipmentType { get; set; }

        // Удобное свойство для вывода в UI (например, "Проектор - Epson EB-530")
        public string FullDisplayName => $"{EquipmentType?.Name} {Model}".Trim();
    }

    [Table("locations")]
    public class Location : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("room_number")]
        public string RoomNumber { get; set; }

        [Column("building")]
        public string Building { get; set; }
    }

    [Table("equipment_types")]
    public class EquipmentType : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}