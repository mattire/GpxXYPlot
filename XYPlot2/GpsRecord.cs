using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYPlot2
{
    public class GpsRecord
    {
        public long ID { get; set; }
        public double Wgs84Latitude { get; set; }
        public double Wgs84Longitude { get; set; }
        public double Wgs84Altitude { get; set; }
        public float Accuracy { get; set; }
        // epoc or ticks
        public long Time { get; set; }
    }
}
