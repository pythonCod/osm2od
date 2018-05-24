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


        public Tuple<Double, Double> calculateBounds()
        {
            double minLat = OsmModel.bounds.minlat;
            double maxLat = OsmModel.bounds.maxlat;
            double minLon = OsmModel.bounds.minlon;
            double maxLon = OsmModel.bounds.maxlon;

            var leftBot = new GeoCoordinate(minLat,minLon);
            var rightTop = new GeoCoordinate(maxLat, maxLon);
            var leftTop = new GeoCoordinate(maxLat, minLon);
            var rightBot = new GeoCoordinate(minLat, maxLon);

            var mapWidth = rightBot.GetDistanceTo(leftBot);
            var mapHeight = leftBot.GetDistanceTo(leftTop);


            Console.WriteLine("Map width : {0}m, Map Height : {1}m ", mapWidth,mapHeight);
            Tuple<Double, Double> originPoint = new Tuple<double, double>(minLat,minLon);
            return originPoint;
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

        public string LatToY()
        {
            NotImplementedException error = new NotImplementedException();
            return error.Message;
        }
        public string LonToX()
        {
            NotImplementedException error = new NotImplementedException();
            return error.Message;
        }

    }
}
