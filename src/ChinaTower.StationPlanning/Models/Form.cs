﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ChinaTower.StationPlanning.Models
{
    public enum FormType
    {
        储备库,
        存量资源,
        新建站址,
        潜在难点库,
        在建难点库
    }

    public class Form
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        [MaxLength(32)]
        public string City { get; set; }

        public DateTime Time { get; set; }

        public FormType Type { get; set; }

        [NotMapped]
        public string[] Objects
        {
            get { return JsonConvert.DeserializeObject<string[]>(Content); }
        }
    }
}
