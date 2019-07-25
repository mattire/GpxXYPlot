using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Extensions;

namespace XYPlot2
{

    public class GpsNode
    {
        public float Lon { get; set; }
        public float Lat { get; set; }
        public float Alt { get; set; }

        public GpsNode Substract(GpsNode gpsNode)
        {
            return new GpsNode()
            {
                Lon = this.Lon - gpsNode.Lon,
                Lat = this.Lat - gpsNode.Lat,
                Alt = this.Alt - gpsNode.Alt,
            };
        }
    }


    public class ReadGpx
    {
        static Func<XmlNode, string, string> getAttr = (node, name) => {
            string res = "";
            foreach (XmlAttribute a in node.Attributes)
            {
                if (a.Name == name) { res = a.Value; }
            }
            return res;
        };

        static GpsRecord XmlNode2Gpsr(XmlNode n) {

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            GpsRecord gr = new GpsRecord();
            gr.Wgs84Latitude = float.Parse(getAttr(n, "lat"), System.Globalization.NumberStyles.Any, ci);
            gr.Wgs84Longitude = float.Parse(getAttr(n, "lon"), System.Globalization.NumberStyles.Any, ci);
            gr.Wgs84Altitude = float.Parse(n.ChildNodes[0].InnerText, System.Globalization.NumberStyles.Any, ci);

            // <time>2019-07-11T05:47:18Z</time>
            if (n.ChildNodes.Count > 1) {
                try { gr.Time = DateTime.Parse(n.ChildNodes[1].InnerText).Ticks; }
                catch (Exception e) { System.Diagnostics.Debug.WriteLine(e.ToString()); }
            }
            
            return gr;
        }

        static GpsNode XmlNode2Gps(XmlNode n)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            GpsNode gn = new GpsNode();
            gn.Lat = float.Parse(getAttr(n, "lat"), System.Globalization.NumberStyles.Any, ci);
            gn.Lon = float.Parse(getAttr(n, "lon"), System.Globalization.NumberStyles.Any, ci);
            gn.Alt = float.Parse(n.ChildNodes[0].InnerText, System.Globalization.NumberStyles.Any, ci);
            return gn;
        }

        static void Print(List<GpsNode> gpsNodes)
        {
            foreach (var g in gpsNodes) { System.Diagnostics.Debug.WriteLine($"{g.Lat},{g.Lon}, {g.Alt}"); }
        }

        static void Print(List<GpsRecord> gpsNodes)
        {
            foreach (var g in gpsNodes) {
                System.Diagnostics.Debug.WriteLine(
                $"{g.Wgs84Latitude.ToString(CultureInfo.InvariantCulture)}," +
                $"{g.Wgs84Longitude.ToString(CultureInfo.InvariantCulture)}, " +
                $"{g.Wgs84Altitude.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        public static void ReadGpxErrors(string path)
        {
            List<GpsRecord> grl = ReadGpxFile(path);
            GpsRecord gAve = new GpsRecord()
            {
                Wgs84Altitude = grl.Select(g => g.Wgs84Altitude).Average(),
                Wgs84Longitude = grl.Select(g => g.Wgs84Longitude).Average(),
                Wgs84Latitude = grl.Select(g => g.Wgs84Latitude).Average(),
            };
            List<GpsRecord> errs = grl.Select(g => g.Substract(gAve)).ToList();
            Print(errs);
            //return grl.Select(g => g.Substract(gAve)).ToList();
        }

        public static Tuple<List<Point>, List<Point>, List<Point>> ReadGpxFile2PointSeries(string path) {
            var grl = ReadGpxFile(path);
            
            return new Tuple<List<Point>, List<Point>, List<Point>>(
                    grl.Select(r => TimeToTenthSecs(new Point() { T = r.Time, Y = (float)r.Wgs84Latitude  })).ToList(),
                    grl.Select(r => TimeToTenthSecs(new Point() { T = r.Time, Y = (float)r.Wgs84Longitude })).ToList(),
                    grl.Select(r => TimeToTenthSecs(new Point() { T = r.Time, Y = (float)r.Wgs84Altitude  })).ToList()
                );
        }

        static int divider = 1000000;
        public static IEnumerable<Point> TimeToTenthSecs(IEnumerable<Point> points)
        {
            return points.Select(p => new Point() { T = p.T / divider, Y = p.Y });
        }

        public static Point TimeToTenthSecs(Point p)
        {
            return new Point() { T = p.T / divider, Y = p.Y };
        }


        public static List<GpsRecord> ReadGpxFile(string path)
        {
            XmlDocument xdoc = new XmlDocument();

            path = path ?? "C:\\Users\\Matti\\Documents\\Visual Studio 2017\\gps\\gpx\\TestX6.gpx";
            //path = path ?? "C:\\Users\\mreijonen\\Desktop\\TestX6.gpx";
            xdoc.Load(path);
            //var count = xdoc.DocumentElement.ChildNodes[0].ChildNodes[2].ChildNodes.Count;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("ns", "http://www.topografix.com/GPX/1/1");
            var gpsPoints = xdoc.SelectNodes("//ns:trkpt", nsmgr);

            //List<GpsNode> gsl = new List<GpsNode>();
            List<GpsRecord> grl = new List<GpsRecord>();
            foreach (XmlNode p in gpsPoints)
            {
                //var g = XmlNode2Gps(p);
                //gsl.Add(g);
                var gr = XmlNode2Gpsr(p);
                grl.Add(gr );
                System.Diagnostics.Debug.WriteLine($"{gr.Wgs84Latitude},{gr.Wgs84Longitude}, {gr.Wgs84Altitude}, {gr.Time}");
            }
            return grl;
        }

    }
}
