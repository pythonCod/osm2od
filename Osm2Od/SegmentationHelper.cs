using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace Osm2Od
{
    public struct Point
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
    public  class SegmentationHelper
    {

        public List<Point> curvaturDistanceDomain { get; set; }

        public  SegmentationHelper (double[] distanceVector, double[] curvatureVector)
        {
            var mergeDistanceCurvature = distanceVector.Zip(curvatureVector, (distance, curvature) => (distance, curvature));
            this.curvaturDistanceDomain = new List<Point>(curvatureVector.Count());
            for (int i = 0; i < curvatureVector.Count(); i++)
            {
                Point curvatureDistancePoint = new Point(distanceVector[i], curvatureVector[i]);
                curvaturDistanceDomain.Add(curvatureDistancePoint);
            }
        }
        /// <summary>
        /// Douglases the peucker reduction.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="firstPoint">The first point.</param>
        /// <param name="lastPoint">The last point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="pointIndexsToKeep">The point index to keep.</param>

        public static Tuple<List<Point>,List<int>> DouglasPeuckerReduction(List<Point> Points, Double Tolerance)
        {
            double Tolerancesqrd = Tolerance * Tolerance;
            List<int> juncIndxs = new List<int>();
            if (Points == null || Points.Count < 3)
            {
                juncIndxs.Add(0);
                juncIndxs.Add(Points.Count() - 1);
                return Tuple.Create(Points, juncIndxs);

            }
            Int32 firstPoint = 0;
            Int32 lastPoint = Points.Count - 1;
            List<Int32> pointIndexsToKeep = new List<Int32>();

            //Add the first and last index to the keepers
            pointIndexsToKeep.Add(firstPoint);
            pointIndexsToKeep.Add(lastPoint);


            //The first and the last point can not be the same
            while (lastPoint >= 0 && Points[firstPoint].Equals(Points[lastPoint]))
            {
                lastPoint--;
            }
            if (lastPoint == 0) { return Tuple.Create(Points, juncIndxs); }

            SortedDictionary<Int32, Int32> PairsIndexesToCheck = new SortedDictionary<Int32, Int32>();
            PairsIndexesToCheck.Add(firstPoint, lastPoint);
            Int32 checkcnt = 0;
            while (PairsIndexesToCheck.Count > 0 && checkcnt < lastPoint)
            {
                Double maxDistancesqrd = 0;
                Int32 indexFarthest = 0, currentFirstPoint = PairsIndexesToCheck.First().Key, currentLastPoint = PairsIndexesToCheck.First().Value;
                double deltax = Points[currentFirstPoint].X - Points[currentLastPoint].X,
                deltay = Points[currentFirstPoint].Y - Points[currentLastPoint].Y;
                Double oneoverbottomsqrd = 1 / (deltax * deltax + deltay * deltay),
                x1y2 = Points[currentFirstPoint].X * Points[currentLastPoint].Y,
                x2y1 = Points[currentLastPoint].X * Points[currentFirstPoint].Y,
                x1y2_diff_x2y1 = x1y2 - x2y1;
                ;
                for (Int32 index = currentFirstPoint + 1; index < currentLastPoint; index++)
                {
                    Double distancesqrd = PerpendicularDistance(Points[currentFirstPoint], Points[currentLastPoint], Points[index], oneoverbottomsqrd, x1y2_diff_x2y1, deltax, deltay);
                    if (distancesqrd > maxDistancesqrd)
                    {
                        maxDistancesqrd = distancesqrd;
                        indexFarthest = index;
                    }
                }

                if (maxDistancesqrd > Tolerancesqrd)
                {
                    //Add the largest point that exceeds the tolerance
                    pointIndexsToKeep.Add(indexFarthest);
                    //split current pair
                    PairsIndexesToCheck[currentFirstPoint] = indexFarthest;
                    PairsIndexesToCheck.Add(indexFarthest, currentLastPoint);

                }
                else
                {
                    //pair is checked
                    PairsIndexesToCheck.Remove(currentFirstPoint);
                }
                checkcnt++;
            }
            if (checkcnt == lastPoint)
            { Console.WriteLine("Last Point"); }

            List<Point> returnPoints = new List<Point>();
            pointIndexsToKeep.Sort();

            foreach (Int32 index in pointIndexsToKeep)
            {
                returnPoints.Add(Points[index]);
                juncIndxs.Add(index);
            }

            return Tuple.Create(returnPoints, juncIndxs);
            }
        


        public static Double PerpendicularDistance(Point Point1, Point Point2, Point Point, Double oneoverbottomsqrd, Double x1y2_diff_x2y1, Double deltax, Double deltay)
        {
            //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle вект произв 
            //Base = √((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            //Area = .5*Base*H                                          *Solve for height
            //Height = Area/.5/Base

            Double area = Math.Abs(x1y2_diff_x2y1 - Point.Y * deltax + Point.X * deltay);
            Double areasqrd = area * area;

            Double heightsqrd = areasqrd * oneoverbottomsqrd;

            return heightsqrd;



        }
    }
}

