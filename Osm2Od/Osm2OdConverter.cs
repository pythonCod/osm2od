using System;
using System.Collections.Generic;
using System.Linq;
using Osm2Od;
using System.Device.Location;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;
using System.Drawing;

namespace ConsoleApp1
{

    public class Osm2OdConverter
    {

        public osm OsmModel { get; set; }

        public OpenDrive OdModel;

        public List<node> numberNodesInWay
        {
            get;set;
        }



        public Dictionary<way, List<node>> wayPointsDictionary
        {
            get;set;
        }


        public Tuple<double,double> calculateBounds()
        {
            double minLat = OsmModel.bounds.minlat;
            double maxLat = OsmModel.bounds.maxlat;
            double minLon = OsmModel.bounds.minlon;
            double maxLon = OsmModel.bounds.maxlon;

            double mapWidth = Distance(minLat,minLon, minLat,maxLon);
            double mapHeight = Distance(minLat,minLon,maxLat,minLon);

            //double xO = LonToX(minLon);
            //double yO = LatToY(minLat);
            Tuple<double,double> originPoint = new Tuple<double, double>(minLon, minLat);



            Console.WriteLine("Map width : {0}m, Map Height : {1}m ", mapWidth,mapHeight);
            Console.WriteLine("origin x at {0}, origin y at {1}", originPoint.Item1, originPoint.Item2);
            return originPoint;
        }

        /// <summary>
        /// 
        /// Measure the destance between two lat,lon points and return the distance in meters
        /// 
        ///    https://github.com/googollee/eviltransform/blob/master/csharp/EvilTransform.cs
        ///    License : https://github.com/googollee/eviltransform/blob/master/LICENSE
        /// 
        /// </summary>
        /// <param name="latA"> first point lat </param>
        /// <param name="lngA"> first point lng </param>
        /// <param name="latB"> second point lat </param>
        /// <param name="lngB"> second point lon</param>
        /// <returns></returns>
        public double Distance(double latA, double lngA, double latB, double lngB)
        {
            double earthR = 6371000;
            double x = Math.Cos(latA * Math.PI / 180) * Math.Cos(latB * Math.PI / 180) * Math.Cos((lngA - lngB) * Math.PI / 180);
            double y = Math.Sin(latA * Math.PI / 180) * Math.Sin(latB * Math.PI / 180);
            double s = x + y;
            if (s > 1)
                s = 1;
            if (s < -1)
                s = -1;
            double alpha = Math.Acos(s);
            var distance = alpha * earthR;
            return distance;
        }

        public Dictionary<way, List<node>> convertRoads()
        {
            int numberWaysInModel = this.OsmModel.way.Count;
            
            this.wayPointsDictionary = new Dictionary<way, List<node>>();
            for (int i = 0; i < numberWaysInModel; i++)
            {
                numberNodesInWay = new List<node>();
                for (int y = 0; y < OsmModel.way[i].nd.Count; y++)
                {
                    
                    ulong refValue = OsmModel.way[i].nd[y].@ref;

                    node nodeId = (from nod in OsmModel.node
                                  where nod.id == refValue
                                  select nod).First();
                    numberNodesInWay.Add(nodeId);
                }
                this.wayPointsDictionary[OsmModel.way[i]] = numberNodesInWay;
            }

            Console.WriteLine("Road corresponding points were successfuly exported, Performing reconstruction Algorithm");
            this.SegmentOsmRoads(this.wayPointsDictionary);
            return this.wayPointsDictionary;
            
        }

        /// <summary>
        /// input = internal OsmModel object
        /// 
        /// iterate over ways in the OsmModel, grab their Latitude, Longitude
        /// convert them to x, y Coordinates [meter as a base unit], calculate 
        /// the hdg values based on calculate Heading function 
        /// </summary>
        /// <returns>OdModel : an Open drive object contain the converted information</returns>
        public OpenDrive convertRoadsFromOsmToOd()
        {

            OpenDrive OdModel = new OpenDrive();
            return this.OdModel;
        }

        public bool calculateHeading()
        {
            return true;
        }

        public double LatToY(double lat)
        {
            return Math.Log( Math.Tan( (lat+90) / 360*Math.PI) ) / Math.PI*180;
        }
        public double LonToX(double lon)
        {
            return (lon) / (180 * Math.PI);
        }

        /// <summary>
        /// find if point is elligable to be a junction point based on the area of the triangle
        /// formed by threee consecutive points
        /// </summary>
        /// <param name="node1"> </param>
        /// <param name="node2"></param>
        /// <param name="node3"></param>
        /// <returns></returns>
        public void SegmentOsmRoads(Dictionary<way, List<node>> wayNodesDict)
        {
            foreach (KeyValuePair<way,List<node>> road in wayNodesDict)
            {
                // segmentation works on a set of points, which are 3 or more than 3 consecutive points, if otherwise found
                // it will be considered as line.
                if (road.Value.Count >= 3 && road.Key.tag.Find(highWayTag => highWayTag.k == "highway") != null)
                {
                    node nextNode = GetNext<node>(road.Value, road.Value[0]);
                    Queue<double> junctionPointsPositions = ApplyLSG(road.Value);

                }
                else
                {
                    Console.WriteLine("[Segmentation] object {0} is a simple line or has no highway tag", road.Key.id);
                }
            }
            //double a = Distance(node1.lat,node1.lon,node2.lat,node2.lon);
            //double b = Distance(node2.lat,node2.lon,node3.lat,node3.lon);
            //double c = Distance(node2.lat, node2.lon, node3.lat, node3.lon);
            //double s = (a + b + c) / 2;
            //double triangleArea =  Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        private Queue<double> ApplyLSG(List<node> wayNodes)
        {
            Queue<double> junctionPointsPositions = new Queue<double>();
            Queue<double> junctionTriangles = new Queue<double>();

            for (int i = 0; i < wayNodes.Count-1; i++)
            {
                if (i != 0)
                {
                    node p_i = wayNodes[i];
                    node p_p = wayNodes[i - 1];
                    node p_n = wayNodes[i + 1];

                    double a = Distance(p_p.lat, p_p.lon, p_i.lat, p_i.lon);
                    double b = Distance(p_i.lat, p_i.lon, p_n.lat, p_n.lon);
                    double c = Distance(p_p.lat, p_p.lon, p_n.lat, p_n.lon);
                    double s = (a + b + c) / 2.0;
                    double triangleArea = Math.Sqrt(s * (s - a) * (s - b) * (s - c));

                    junctionTriangles.Enqueue(triangleArea);
                    if (junctionTriangles.Max() == triangleArea && triangleArea != 0)
                    {
                        Console.WriteLine("junction point at point {0} found", i);
                        junctionPointsPositions.Enqueue(i);
                    }
                }
                else
                {
                    Console.WriteLine("first point");
                }

            }

            return junctionPointsPositions;

        }

        private static T GetNext<T>(IEnumerable<T> list, T current)
        {
            try
            {
                return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
            }
            catch
            {
                return default(T);
            }
        }

        private static T GetPrevious<T>(IEnumerable<T> list, T current)
        {
            try
            {
                return list.TakeWhile(x => !x.Equals(current)).Last();
            }
            catch
            {
                return default(T);
            }
        }

    }
}
