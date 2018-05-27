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

        public Dictionary<ulong, List<node>> wayPointsDictionary
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

        public Dictionary<ulong, List<node>> convertRoads()
        {
            int numberWaysInModel = this.OsmModel.way.Count;
            
            this.wayPointsDictionary = new Dictionary<ulong, List<node>>();
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
                this.wayPointsDictionary[OsmModel.way[i].id] = numberNodesInWay;
            }

            Console.WriteLine("Road corresponding points were successfuly exported, Performing reconstruction Algorithm");
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

    }
}
