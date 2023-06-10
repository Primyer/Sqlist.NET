using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sqlist.NET.Migration.Data
{
    public class SchemaPhase
    {
        [Column("version")]
        public string? Version { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("summary")]
        public string? Summary { get; set; }

        [Column("applied")]
        public DateTime Applied { get; set; } = DateTime.Now;
    }
}
