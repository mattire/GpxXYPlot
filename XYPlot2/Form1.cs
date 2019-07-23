using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace XYPlot2
{



    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
            //ReadGpx.ReadGpxErrors(null);
            GpxDataPlot();
            //LinearRegressionTest(null, null);
        }

        private void GpxDataPlot() {
            // lat lon alt
            Tuple<List<Point>, List<Point>, List<Point>> t = ReadGpx.ReadGpxFile2PointSeries("C:\\Users\\Matti\\Documents\\Visual Studio 2017\\Projects\\XYPlot\\TestX7.gpx");


            //DrawTimeSeries(t.Item1.Take(100).ToList(), false);
            //DrawTimeSeries(t.Item1, false);

            var Chart1points = DrawTimeSeries(t.Item1, false, "ts1", chart1, SeriesChartType.Point);
            var Chart2points = DrawTimeSeries(t.Item2, false, "ts2", chart2, SeriesChartType.Point);

            //DrawTimeSeries(t.Item1.Take(4), false, "ts1_l1", chart1, SeriesChartType.Line, false);

            GplsLLStest(t, Chart1points, Chart2points);
            //DrawTimeSeries(t.Item3, false, "ts3");
        }

        public IEnumerable<Point> LineToPoints(Line l, int start, int end, List<Point> points)
        {
            var len = end - start;
            return points.Skip(start).Take(len).Select(p => new Point() { T = p.T, Y = p.T * l.B + l.A });
        }

        private void GplsLLStest(Tuple<List<Point>, List<Point>, List<Point>> t, IEnumerable<Point> ch1p, IEnumerable<Point> ch2p)
        {
            //var lats = new LatitudeLLS(t.Item1);
            var lats = new LatitudeLLS(ch1p.Take(4));
            var lons = new LatitudeLLS(ch2p);
            var ps = LineToPoints(lats.Line, 0, 10, ch1p.ToList());
            foreach (var p in ps) { p.SD(); }

            System.Diagnostics.Debug.WriteLine("aa");

            DrawTimeSeries(ps, false, "p1", chart1, SeriesChartType.Line, false);

            //var lls = new LinearLeastSquares(ch1p);

            ////var gpslls = new StaticGpsLLSPath(t);
            //var gpslls = new StaticGpsLLSPath(ch1p, ch2p);
            ////var ps1 = gpslls.FstLatLine.Line.GetLinePointsTime2X(t.Item1.Take(9).ToList());
            //var ps1 = gpslls.FstLatLine.Line.GetLinePointsTime2X(ch1p.Take(9).ToList());
            //var ps2 = gpslls.FstLonLine.Line.GetLinePointsTime2X(ch2p.Take(9).ToList());
            //
            ////foreach (var item in ps1) { item.SD(); }
            //
            //DrawTimeSeries(ps1, false, "ps1", chart1, SeriesChartType.Line, false);
            ////DrawTimeSeries(ps2, false, "ps2", chart1, SeriesChartType.Line, true);
            ////gpslls.mLatPoints.Take(2);
            ////gpslls.FstLatLine
        }

        private void LinearRegressionTest(object sender, EventArgs e)
        {
            Random rdn = new Random();
            int rr = 10; //rndRange
            List<Tuple<int, int>> xys = new List<Tuple<int, int>>();

            /*/
            var points = Enumerable.Range(120, 170).Select(x => new Point() { X = x, Y = x + rdn.Next(-rr, rr) });
            Line line = LinearRegression(points);
            /*/
            var points = Enumerable.Range(120, 170).Select(x => new LLSPoint() { X = x, Y = x + rdn.Next(-rr, rr) });
            var lls = new LinearLeastSquares(points);
            Line line = lls.Line;
            //*/
            //var linePoints = points.Select(p => new Point() { X = p.X, Y = line.f(p.X) });

            foreach (var p in points)
            {
                chart1.Series["test1"].Points.AddXY
                                (p.X, p.Y);
                chart1.Series["test2"].Points.AddXY
                                (p.X, line.f(p.X));
            }

            //for (int i = 0; i < 50; i++)
            //{
            //    chart1.Series["test1"].Points.AddXY
            //                    (i, i + rdn.Next(0, 4));
            //    //chart1.Series["test2"].Points.AddXY
            //    //                (rdn.Next(0, 10), rdn.Next(0, 10));
            //}
            chart1.Series["test1"].ChartType = SeriesChartType.Point;
            chart1.Series["test1"].Color     = Color.Black;
            chart1.Series["test2"].ChartType = SeriesChartType.FastLine;
            chart1.Series["test2"].Color     = Color.Blue;

            var yMax = points.Select(p => p.Y).Max();
            var yMin = points.Select(p => p.Y).Min();
            chart1.ChartAreas[0].AxisY.Maximum = yMax;
            chart1.ChartAreas[0].AxisY.Minimum = yMin;

            var errs = line.Errors(points.ToList());

            foreach (var err in errs) {
                chart2.Series["Series1"].Points.AddXY(err.X, err.Y);
            }
            chart2.Series["Series1"].ChartType = SeriesChartType.Point;
            chart2.Series["Series1"].Color = Color.Red;

            //chart1.ChartAreas[0].AxisY.Maximum = Double.NaN; // sets the Maximum to NaN
            //chart1.ChartAreas[0].AxisY.Minimum = Double.NaN; // sets the Minimum to NaN
            //chart1.ChartAreas[0].RecalculateAxesScale();
        }

        Dictionary<Chart, long> xBases = new Dictionary<Chart, long>();
        // return drawing area points
        private IEnumerable<Point> DrawTimeSeries(IEnumerable<Point> points, bool lls, string seriesName, Chart chart, SeriesChartType chartType, bool setRange = true)
        {
            long xBase = points.Select(p => p.T).Min();

            #region SetChartRange
            if (setRange) {
                xBase = points.Select(p => p.T).Min();
                xBases[chart] = xBase;
                points = points.Select(p => new Point() { T = p.T - xBase, Y = p.Y }).ToList();

                float MaxY = points.Select(p => p.Y).Max();
                float MinY = points.Select(p => p.Y).Min();
                long MaxT = points.Select(p => p.T).Max();
                long MinT = points.Select(p => p.T).Min();
                chart.ChartAreas[0].AxisY.Maximum = MaxY;
                chart.ChartAreas[0].AxisY.Minimum = MinY;
                chart.ChartAreas[0].AxisX.Maximum = MaxT;
                chart.ChartAreas[0].AxisX.Minimum = MinT;
            } else {
                //points = points.Select(p => new Point() { T = p.T - xBases[chart], Y = p.Y }).ToList();
                foreach (var item in points) { item.SD(); }
            }
            #endregion

            #region DrawPoints
            chart.Series.Add(seriesName);
            //foreach (var p in points) { chart.Series["test1"].Points.AddXY(p.T, p.Y); }
            //chart.Series["test1"].ChartType = SeriesChartType.Point;
            //chart.Series["test1"].Color = Color.Red;
            foreach (var p in points) { chart.Series[seriesName].Points.AddXY(p.T, p.Y); }
            chart.Series[seriesName].ChartType = chartType;
            chart.Series[seriesName].Color = Color.Red;

            #endregion

            if (lls) { }
            return points;
        }

        


        // http://mathworld.wolfram.com/LeastSquaresFitting.html
        private Line LinearRegression(IEnumerable<Point> points)
        {
            // f(a,b) = a + b*x

            //      avg(y) * sum(x^2) - avg(x) * sum (xy)
            // a = --------------------------------------
            //          sum (x^2) - n (avg(x))^2

            //      sum(x*y) - n avg(x) * avg(y)
            // b = --------------------------------------
            //         sum (x^2) - n (avg(x))^2

            var n = points.Count();
            var avgY = points.Select(p => p.Y).Average();
            var sum_x2 = points.Select(p => p.X * p.X).Sum();
            var avgX = points.Select(p => p.X).Average();
            var sum_xy = points.Select(p => p.X * p.Y).Sum();

            var a = (avgY * sum_x2 - avgX * sum_xy) / (sum_x2 - n * avgX * avgX);

            var b = (sum_xy - n * avgX * avgY) / (sum_x2 - n * (avgX * avgX));

            return new Line() { A = a, B = b };
        }

    }

    //public class Point
    //{
    //    public float X { get; set; }
    //    public float Y { get; set; }
    //    public long T { get; set; }
    //}

    //public class Line
    //{
    //    public float A { get; set; }
    //    public float B { get; set; }

    //    public float f(float x) { return A + B * x; }

    //    public List<Point> Errors(List<Point> points)
    //    {
    //        return points.Select(p => new Point() { X = p.X, Y = f(p.X) - p.Y }).ToList();
    //    }
    //}

}
