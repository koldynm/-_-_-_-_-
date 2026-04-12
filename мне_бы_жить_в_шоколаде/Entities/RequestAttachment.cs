using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace мне_бы_жить_в_шоколаде.Entities
{
    // Модель для вложений (фотографий)
    [Table("request_attachments")]
    public class RequestAttachment : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("request_id")]
        public Guid RequestId { get; set; }

        [Column("file_path")]
        public string FilePath { get; set; }
    }
}
