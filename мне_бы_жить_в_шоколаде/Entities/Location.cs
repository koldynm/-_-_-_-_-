using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace мне_бы_жить_в_шоколаде.Entities
{
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
}
