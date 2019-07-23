using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYPlot2
{
    // gps point with time
    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
        public long T { get; set; }

        public void SD() {
            System.Diagnostics.Debug.WriteLine($"{X}, {Y}, {T}");
        }
    }

    public class LLSPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
    }


    public class Line
    {
        public float A { get; set; }
        public float B { get; set; }

        public float f(float x) { return A + B * x; }

        public List<Point> Errors(List<Point> points)
        {
            return points.Select(p => new Point() { X = p.X, Y = f(p.X) - p.Y }).ToList();
        }

        public List<LLSPoint> Errors(List<LLSPoint> points)
        {
            return points.Select(p => new LLSPoint() { X = p.X, Y = f(p.X) - p.Y }).ToList();
        }

        internal IEnumerable<Point> GetLinePointsTime2X(List<Point> points)
        {
            return points.Select(p => new Point() { T = p.T, X = p.T, Y = f(p.T) });
        }
    }

    class LatitudeLLS : LinearLeastSquares
    {
        //public LatitudeLLS(IEnumerable<Point> points) : base(points.Select(p => new LLSPoint() { X = (float)p.T / 1000000, Y = p.Y })) { }
        public LatitudeLLS(IEnumerable<Point> points) : base(points.Select(p => new LLSPoint() { X = (float)p.T , Y = p.Y })) { }

        public void Add(Point p)
        {
            var llsp = new LLSPoint() { X = p.T, Y = p.Y };
            Add(llsp);
        }
    }

    class LongitudeLLS : LinearLeastSquares
    {
        //public LongitudeLLS(IEnumerable<Point> points) : base(points.Select(p => new LLSPoint() { X = (float)p.T / 1000000, Y = p.X })) { }
        public LongitudeLLS(IEnumerable<Point> points) : base(points.Select(p => new LLSPoint() { X = (float)p.T , Y = p.X })) { }

        public void Add(Point p)
        {
            var llsp = new LLSPoint() { X = p.T, Y = p.X };
            Add(llsp);
        }
    }

    class LinearLeastSquares
    {
        // f(a,b) = a + b*x

        //      avg(y) * sum(x^2) - avg(x) * sum (xy)   (sum(y)/n) * sum(x^2) - (sum(x)/n) * sum (x*y)
        // a = -------------------------------------- = ----------------------------------------------
        //          n sum (x^2) - n (avg(x))^2                n * sum(x^2) - n (sum(x)/n)^2

        //      sum(x*y) - n avg(x) * avg(y)            sum(x*y) - n (sum(x)/n) * (sum(y)/n)
        // b = -------------------------------------- = -----------------------------
        //         sum (x^2) - n (avg(x))^2                sum (x^2) - n (sum(x)/n)^2

        public float B { get; set; }
        public float A { get; set; }

        private IEnumerable<LLSPoint> mPoints;

        // First level calcs (updated by adding)
        private float sumX, sumY, sumX2, sumXY;
        private int n;

        // Second level calcs (always calculated)
        private float avgX, avgY;

        public LinearLeastSquares(IEnumerable<LLSPoint> points)
        {
            mPoints = points;

            sumX = points.Select(p => p.X).Sum();
            sumY = points.Select(p => p.Y).Sum();
            sumX2 = points.Select(p => p.X * p.X).Sum();
            sumXY = points.Select(p => p.X * p.Y).Sum();
            n = points.Count();

            CalcAvgs();
            CalcAB();
        }

        private void CalcAvgs() { avgX = sumX / n; avgY = sumY / n; }

        private void CalcAB() {
            A = (avgY * sumX2 - avgX * sumXY) / (sumX2 - n * avgX * avgX);
            B = (sumXY - n * avgX * avgY) / (sumX2 - n * (avgX * avgX));
        }

        public float NextError(LLSPoint p) {
            return (A + B * p.X) - p.Y;
        }

        public float AverageError()
        {
            return mPoints.Select(p => Math.Abs(p.Y - (A + B * p.Y))).Sum() / mPoints.Count();
        }

        public void Add(LLSPoint p)
        {
            sumX += p.X;
            sumY += p.Y;
            sumX2 += p.X * p.X;
            sumXY += p.X * p.Y;
        }

        public Line Line {
            get {
                return new Line() { A = A, B = B };
            }
        }

        public List<float> LastPointsDiff(int count)
        {
            return null;
        }

    }
}
