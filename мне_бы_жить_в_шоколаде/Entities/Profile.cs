using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace мне_бы_жить_в_шоколаде.Entities
{
    [Table("profiles")]

    public class Profile: BaseModel 
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }
        [Column("full_name")]
        public string Name { get; set; }
        [Column("role")]
        public string Role { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}

