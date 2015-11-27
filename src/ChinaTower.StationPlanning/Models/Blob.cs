using System;
using System.ComponentModel.DataAnnotations;

namespace ChinaTower.StationPlanning.Models
{
    public class Blob
    {
        public Guid Id { get; set; }

        [MaxLength(128)]
        public string FileName { get; set; }

        [MaxLength(128)]
        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public DateTime Time { get; set; }

        public byte[] File { get; set; }
    }
}
