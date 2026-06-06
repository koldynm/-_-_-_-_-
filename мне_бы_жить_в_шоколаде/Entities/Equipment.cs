using Postgrest.Attributes;
using Postgrest.Models;

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

        [Reference(typeof(Location), true, false)]
        public Location? Location { get; set; }

        [Reference(typeof(EquipmentType), true, false)]
        public EquipmentType? EquipmentType { get; set; }

        // Удобное свойство для вывода в UI (например, "Проектор - Epson EB-530")
        public string FullDisplayName => $"{EquipmentType?.Name} {Model}".Trim();
    }
}