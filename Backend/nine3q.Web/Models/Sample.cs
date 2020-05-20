﻿using System;

namespace nine3q.Web.Models
{
    public class Sample
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF
        {
            get {
                //throw new Exception("Hallo Exception");
                return 32 + (int)(TemperatureC / 0.5556);
            }
        }

        public string Summary { get; set; }
    }
}
