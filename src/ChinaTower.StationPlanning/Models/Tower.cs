using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ChinaTower.StationPlanning.Models
{
    public class Tower
    {
        public Guid Id { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        [MaxLength(64)]
        public string District { get; set; }

        [MaxLength(64)]
        public string City { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TowerType Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TowerScene Scene { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TowerStatus Status { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Provider Provider { get; set; }

        public double Lon { get; set; }

        public double Lat { get; set; }

        [MaxLength(512)]
        public string Address { get; set; }

        [MaxLength(512)]
        public string Url { get; set; }

        [ForeignKey("Image")]
        public Guid? BlobId { get; set; }

        public Blob Image { get; set; }

        public DateTime Time { get; set; }

        public double Height { get; set; }
    }
}
