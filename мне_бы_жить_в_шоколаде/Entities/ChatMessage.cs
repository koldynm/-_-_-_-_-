using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace мне_бы_жить_в_шоколаде.Entities
{
    [Table("request_messages")]
    public class ChatMessage : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("request_id")]
        public Guid RequestId { get; set; }

        [Column("sender_id")]
        public Guid SenderId { get; set; }

        [Column("message_text")]
        public string MessageText { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}