using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYPlot2;

namespace Extensions
{
    public static class GpsRecordExt
    {
        public static GpsRecord Substract(this XYPlot2.GpsRecord g1, XYPlot2.GpsRecord g2)
        {
            //List<string> propNames = new List<string>() { "Wgs84Altitude", "Wgs84Latitude", "Wgs84Longitude" };
            //foreach (var pn in propNames) {
            //    g1.
            //}
            var r = new GpsRecord()
            {
                Wgs84Altitude = g1.Wgs84Altitude - g2.Wgs84Altitude,
                Wgs84Longitude = g1.Wgs84Longitude - g2.Wgs84Longitude,
                Wgs84Latitude = g1.Wgs84Latitude - g2.Wgs84Latitude,
            };
            return r;
        }
    }
}
