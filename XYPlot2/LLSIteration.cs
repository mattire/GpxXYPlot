using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//using LLS = XYPlot2.LinearLeastSquares;
using LLS = XYPlot2.LinearLeastSquares;
using Lons = XYPlot2.LongitudeLLS;
using Lats = XYPlot2.LatitudeLLS;

using LLLine = System.Tuple<long, long, XYPlot2.Line>;

namespace XYPlot2
{
    class LLSIteration
    {
        public List<Tuple<long, long, Line>> IndLatLines = new List<Tuple<long, long, Line>>();
        public List<Tuple<long, long, Line>> IndLonLines = new List<Tuple<long, long, Line>>();

        //Dictionary<long, Line> indLatLines = new Dictionary<long, Line>();
        //Dictionary<long, Line> indLonLines = new Dictionary<long, Line>();

        public LLSIteration(IEnumerable<Point> lats, IEnumerable<Point> lons)
        {
            //float latErrLim = 0.000005f;
            //float latErrLim = 0.00001f;
            //float latErrLim = 0.00002f;
            //float latErrLim = 0.00004f;
            float latErrLim = 0.00008f;
            //float latErrLim = 0.00016f;

            float lonErrLim = 0.001f;
            //float lonErrLim = 0.00005f;


            Latsp = lats;
            Lonsp = lons;

            //IteratePoints(Latsp, latErrLim, IndLatLines, (p) => { return new Lats(p); });
            IteratePoints(Latsp, latErrLim, IndLatLines, (ps) => { return new CustomLLS(ps, (p) => new LLSPoint() { X = p.T, Y = p.Y }); });
            IteratePoints(Lonsp, lonErrLim, IndLonLines, (ps) => { return new CustomLLS(ps, (p)=> new LLSPoint() {  X= p.T, Y=p.Y } ); });
            /*/
            long lastStart = 0;
            long ind = 1;
            var currentLSqs = new Lats(Latsp.Take(2));
            Point lastPoint = Latsp.ElementAt(0);
            foreach (var point in lats.Skip(2))
            {
                ind++;
                var err = currentLSqs.NextError(point);
                System.Diagnostics.Debug.WriteLine("err");
                System.Diagnostics.Debug.WriteLine(err);
                //if (currentLSqs.NextError(point) < latErrLim) {
                if (err < latErrLim)
                {
                    currentLSqs.Update(point);
                    lastPoint = point;
                }
                else
                {
                    //indLatLines.Add(lastStart, currentLSqs.Line);
                    IndLatLines.Add(new LLLine(lastStart, ind, currentLSqs.Line));
                    lastStart = ind;
                    IEnumerable<Point> points = new List<Point>() { lastPoint, point };
                    currentLSqs = new Lats(points);
                }
            }
            IndLatLines.Add(new LLLine(lastStart, ind, currentLSqs.Line));

            System.Diagnostics.Debug.WriteLine(IndLatLines.Count);
            //*/
        }

        public void IteratePoints(
            IEnumerable<Point> pPoints,
            float errLim,
            List<LLLine> resultIndLines,
            Func<IEnumerable<Point>, LinearLeastSquares> GetLLS 
            )
        {
            long lastStart = 0;
            long ind = 1;
            //var currentLSqs = new Lats(pPoints.Take(2));
            var currentLSqs = GetLLS(pPoints.Take(2));
            Point lastPoint = pPoints.ElementAt(0);
            foreach (var point in pPoints.Skip(2))
            {
                ind++;
                var err = currentLSqs.NextError(point);
                System.Diagnostics.Debug.WriteLine("err");
                System.Diagnostics.Debug.WriteLine(err);
                //if (currentLSqs.NextError(point) < latErrLim) {
                if (err < errLim)
                {
                    currentLSqs.Update(point);
                    lastPoint = point;
                }
                else
                {
                    //indLatLines.Add(lastStart, currentLSqs.Line);
                    resultIndLines.Add(new LLLine(lastStart, ind, currentLSqs.Line));
                    lastStart = ind;
                    IEnumerable<Point> points = new List<Point>() { lastPoint, point };
                    currentLSqs = GetLLS(points);
                }
            }
            resultIndLines.Add(new LLLine(lastStart, ind, currentLSqs.Line));

            System.Diagnostics.Debug.WriteLine(resultIndLines.Count);
        }


        public List<Point> GetLatPointSeries() {
            List<Point> latLinePoints = new List<Point>();
            foreach (var ill in IndLatLines)
            {
                System.Diagnostics.Debug.WriteLine($"{ill.Item1}, {ill.Item2}" );
                var p = Latsp.ElementAt((int)ill.Item1);
                latLinePoints.Add(p);
            }
            latLinePoints.Add(Latsp.ElementAt(Latsp.Count() - 1));
            return latLinePoints;
        }

        public List<Point> GetLonPointSeries()
        {
            List<Point> lonLinePoints = new List<Point>();
            foreach (var ill in IndLonLines)
            {
                System.Diagnostics.Debug.WriteLine($"{ill.Item1}, {ill.Item2}");
                var p = Lonsp.ElementAt((int)ill.Item1);
                lonLinePoints.Add(p);
            }
            lonLinePoints.Add(Lonsp.ElementAt(Lonsp.Count() - 1));
            return lonLinePoints;
        }


        public IEnumerable<Point> Latsp { get; private set; }
        public IEnumerable<Point> Lonsp { get; private set; }
    }
}
