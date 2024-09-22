using System;
using System.ComponentModel.DataAnnotations.Schema;
using Sqlist.NET.Migration.Properties;

namespace Sqlist.NET.Migration
{
    public class SchemaPhase
    {
        [Column(Consts.Id)]
        public int Id { get; set; }

        [Column(Consts.Version)]
        public string Version { get; set; } = null!;

        [Column(Consts.Package)]
        public string? Package { get; set; }

        [Column(Consts.Parent)]
        public int Parent { get; set; }

        [Column(Consts.Title)]
        public string Title { get; set; } = null!;

        [Column(Consts.Description)]
        public string? Description { get; set; }

        [Column(Consts.Summary)]
        public string? Summary { get; set; }

        [Column(Consts.Applied)]
        public DateTime Applied { get; set; } = DateTime.UtcNow;
    }
}
