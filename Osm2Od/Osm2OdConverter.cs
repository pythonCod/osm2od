using System;
using System.Collections.Generic;
using System.Linq;
using Osm2Od;
using System.Device.Location;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;
using System.Drawing;
using System.Xml.Serialization;

namespace ConsoleApp1
{

    public  class Osm2OdConverter
    {

        public osm OsmModel { get; set; }

        public OpenDRIVE OdModel { get; set; }


        public List<node> numberNodesInWay
        {
            get;set;
        }



        public Dictionary<way, List<node>> wayPointsDictionary
        {
            get;set;
        }

        public double  minLat { get; set; }
        public double maxLat { get; set; }
        public double minLon { get; set; }
        public double maxLon { get; set; }
        public Tuple<double,double> calculateBounds()
        {
            this.minLat = OsmModel.bounds.minlat;
            this.maxLat = OsmModel.bounds.maxlat;
            this.minLon = OsmModel.bounds.minlon;
            this.maxLon = OsmModel.bounds.maxlon;

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
            double earthR = 6378137;
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
            
            IList<double> filteredValues = new List<double>();
            OpenDRIVE od = new OpenDRIVE();
            int p = wayNodesDict.Count();
            od.road = new OpenDRIVERoad[p];
            int c = 0;
            foreach (KeyValuePair<way,List<node>> road in wayNodesDict)
            {
                // segmentation works on a set of points, which are 3 or more than 3 consecutive points, if otherwise found
                // it will be considered as line.
                if (road.Value.Count > 3 && road.Key.tag.Find(highWayTag => highWayTag.k == "highway") != null)
                {
                    node nextNode = GetNext<node>(road.Value, road.Value[0]);
                    var headingsAndDistances = ApplyLSG(road.Value);
                    double[] curvatureVector = new double[road.Value.Count];
                    double[] distanceVector = new double[road.Value.Count];
                    double[] headingVector = new double[road.Value.Count];
                    double[] emaValues = new double[road.Value.Count];
                    double numberOfnodes = road.Value.Count;

                    
                    //od.road[c] = odRoad;
                    

                    for (int x = 0; x < headingsAndDistances.Count(); x++)
                    {
                        //TODO: replace distances with headings, it is reversed.
                        curvatureVector[x] = (headingsAndDistances[x].distance);
                        Console.WriteLine("Radius of curvature for point {0} is {1}", x, curvatureVector[x]);
                    }
                    //curvature/ distance curve will suffer from some data inhereted inaccuricies
                    //we need to apply smoothing filter to enhance the curve accuracy. Exponential 
                    //moving avarage filter will be used. [SMA : Simple moving avarage for first point only]
                    //[EMA: Exponential moving avarage], filter periods will be 2. 

                    //EMA Multiplier
                    double k = 2 / curvatureVector.Count();
                    for (int i = 0; i < curvatureVector.Count(); i++)
                    {
                        distanceVector[i] = headingsAndDistances[i].heading;
                    }

                    SegmentationHelper segmentationHandler = new SegmentationHelper(distanceVector, curvatureVector);
                   Tuple<List<Osm2Od.Point>,List<int>> junctionPoints = SegmentationHelper.DouglasPeuckerReduction(segmentationHandler.curvaturDistanceDomain,Math.Pow(.07,2));
                    double[] junction_Points_x = new double[junctionPoints.Item2.Count()];
                    double[] junction_Points_y = new double[junctionPoints.Item2.Count()];
                    for (int i = 0; i< junctionPoints.Item1.Count(); i++)
                    {
                        junction_Points_y[i] = junctionPoints.Item1[i].Y;
                        junction_Points_x[i] = junctionPoints.Item1[i].X;
                    }
                    Console.WriteLine("Road {0} junction points calculated, {1} found", road.Key.id, junctionPoints.Item1.Count());
                    //segment the sections according to line deviataion, if line stills withing our given tolerance, with curvature value 
                    //==0 or near zero, it will be considered a line, if curvature!=0, it is a curve, if != constant value it is a spiral
                    List<double> slopes = getRoadPrimativeGeometries(junction_Points_x, junction_Points_y);
                    OpenDRIVERoad odRoad = convertRoadsFromOsmToOd(road, distanceVector, curvatureVector, junction_Points_x, junction_Points_y);
                    odRoad.planView = new OpenDRIVERoadGeometry[slopes.Count()];
                    //create the geometry attribute of the road
                    for (int i = 0; i< slopes.Count(); i++)
                    {
                        OpenDRIVERoadGeometry roadGeometry = new OpenDRIVERoadGeometry();
                        roadGeometry.Items = new object[1];
                        roadGeometry.s = distanceVector[junctionPoints.Item2[i]];
                        roadGeometry.x = MercatorProjection.lonToX(road.Value[junctionPoints.Item2[i]].lon) - MercatorProjection.lonToX(minLon);
                        roadGeometry.y = MercatorProjection.latToY(road.Value[junctionPoints.Item2[i]].lat) - MercatorProjection.latToY(minLat);
                        roadGeometry.hdg = headingVector[junctionPoints.Item2[i]];
                        roadGeometry.length = distanceVector[junctionPoints.Item2[i + 1]];
                        //TODO: SET CURVATURE TOLERANCE HERE
                        if (slopes[i] <= .005 && curvatureVector[junctionPoints.Item2[i]] <=.005)
                        {
                            OpenDRIVERoadGeometryLine line = new OpenDRIVERoadGeometryLine();
                            roadGeometry.Items[0] = line;
                        }
                        if (slopes[i] <= .005 && curvatureVector[junctionPoints.Item2[i]] > .005)
                        {
                            OpenDRIVERoadGeometryArc arc = new OpenDRIVERoadGeometryArc();
                            arc.curvature = curvatureVector[junctionPoints.Item2[i]];
                            roadGeometry.Items[0] = arc;
                        }
                        if (slopes[i] > .005)
                        {
                            OpenDRIVERoadGeometrySpiral spiral = new OpenDRIVERoadGeometrySpiral();
                            spiral.curvStart = curvatureVector[junctionPoints.Item2[i]];
                            spiral.curvEnd = curvatureVector[junctionPoints.Item2[i+1]];
                            roadGeometry.Items[0] = spiral;
                        }
                        odRoad.planView[i] = roadGeometry;
                    }
                    od.road[c] = odRoad;
                    CurvatureDistanceChart curvatureDistanceChart = new CurvatureDistanceChart(curvatureVector, distanceVector, junction_Points_x, junction_Points_y);

                }
                else
                {
                    Console.WriteLine("[Segmentation] object {0} is a simple line or has no highway tag", road.Key.id);
                }
                Console.WriteLine("Heading/distance calculated, transforming to curvature/distance domain");
                c++;
            }
            //deserilize to OpenDrive.xodr.
            XmlSerializer odDeserilizer = new XmlSerializer(typeof(OpenDRIVE));
            var xml = "";

            using (var sww = new System.IO.StringWriter())
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(sww))
                {
                    odDeserilizer.Serialize(writer, od);
                    xml = sww.ToString(); 
                }
            }

        }
        private List<double> getRoadPrimativeGeometries(double[] junction_Points_x, double[] junction_Points_y)
        {
            double[] xData = new double[2];
            double[] yData = new double[2];
            List<double> segmentsSlopeList = new List<double>();

              int vectorLength = junction_Points_x.Count();
            for (int i = 0; i < vectorLength-1; i++)
            {
                double nextXPoint = GetNext<double>(junction_Points_x, junction_Points_x[i]);
                double nextYPoint = GetNext<double>(junction_Points_y, junction_Points_y[i]);
                xData[0] = junction_Points_x[i];
                xData[1] = nextXPoint;
                yData[0] = junction_Points_y[i];
                yData[1] = nextYPoint;

                Tuple<double, double> slopeInterceptData = MathNet.Numerics.Fit.Line(xData, yData);
                segmentsSlopeList.Add(slopeInterceptData.Item2);

            }
            return segmentsSlopeList;
        }

        private OpenDRIVERoad convertRoadsFromOsmToOd(KeyValuePair<way, List<node>> road, double[] distanceVector, double[] curvatureVector, double[] junction_Points_x, double[] junction_Points_y)
        {
            OpenDRIVERoad odRoad = new OpenDRIVERoad();
            odRoad= getRoadAttributes(road, odRoad, distanceVector);
            return odRoad;

        }

        private (double heading,double distance,double hdg)[] ApplyLSG(List<node> wayNodes)
        {
            Queue<double> junctionPointsPositions = new Queue<double>();
            var headingAndDistance = new(double distance, double curvature, double heading)[wayNodes.Count];
            for (int i = 0; i < wayNodes.Count-1; i++)
            {
                if (i != 0 )
                {
                    node p_i = wayNodes[i];
                    node p_p = wayNodes[i - 1];
                    node p_n = wayNodes[i + 1];

                    //find each point heading change in radians, calculate the distance traveled along the road
                    // by using simple heading change (theta)/ distance traveled on the imaginary arc s == 1/R
                    // which is the curvature by the definition of clothoids. 
                    double traveledDistance = Distance(p_p.lat, p_p.lon,p_i.lat,p_i.lon) + (1/2)* Distance(p_i.lat,p_i.lon,p_n.lat,p_n.lon);
                    double x1 = MercatorProjection.lonToX(p_p.lon) - MercatorProjection.lonToX(minLon);
                    double y1 = MercatorProjection.latToY(p_p.lat) - MercatorProjection.latToY(minLat);
                    double x2 = MercatorProjection.lonToX(p_i.lon) - MercatorProjection.lonToX(minLon);
                    double y2 = MercatorProjection.latToY(p_i.lat) - MercatorProjection.latToY(minLat);
                    double x3 = MercatorProjection.lonToX(p_n.lon) - MercatorProjection.lonToX(minLon);
                    double y3 = MercatorProjection.latToY(p_n.lat) - MercatorProjection.latToY(minLat);
                    double dy_i = y2 - y1;
                    double dx_i = x2 - x1;
                    double dy_n = y3 - y2;
                    double dx_n = x3 - x2;
                    //double dy = Math.Sin(p_p.lon - p_i.lon) * Math.Cos(p_i.lat);
                    //double dx = Math.Cos(p_p.lat) * Math.Sin(p_i.lat) - Math.Sin(p_p.lat) * Math.Cos(p_i.lat) * Math.Cos(p_i.lon-p_p.lon); ;
                    var heading_i = Math.Atan2(dy_i, dx_i);
                    var heading_n = Math.Atan2(dy_n, dx_n);
                    var headingChange = heading_n - heading_i;
                    //assign the same values to the first point
                    if (i == 1)
                    {
                        var distanceTraveled = Distance(p_p.lat, p_p.lon, p_i.lat, p_i.lon) + Distance(p_i.lat, p_i.lon, p_n.lat, p_n.lon)/2;
                        headingAndDistance[i].distance = Distance(p_p.lat, p_p.lon, p_i.lat, p_i.lon);
                        headingAndDistance[i].curvature = headingChange / distanceTraveled;
                        headingAndDistance[i].heading = headingChange;
                        headingAndDistance[0].curvature = headingAndDistance[i].curvature;
                        headingAndDistance[0].heading = headingChange;
                        headingAndDistance[0].distance = 0;
                    }
                    else if (i == wayNodes.Count - 2)
                    {
                        double traveledDistanceLast = Distance(p_p.lat, p_p.lon, p_i.lat, p_i.lon)/2 + Distance(p_i.lat, p_i.lon, p_n.lat, p_n.lon)/2;
                        double dxLast = MercatorProjection.lonToX(p_i.lon) - MercatorProjection.lonToX(p_p.lon);
                        double dyLast = MercatorProjection.latToY(p_i.lat) - MercatorProjection.latToY(p_p.lat);
                        double dxLast_n = MercatorProjection.lonToX(p_n.lon) - MercatorProjection.lonToX(p_i.lon);
                        double dyLast_n = MercatorProjection.latToY(p_n.lat) - MercatorProjection.latToY(p_i.lat);

                        var headingLast = Math.Atan2(dyLast, dxLast);
                        var headingLast_n = Math.Atan2(dyLast_n, dxLast_n);
                        var headingChangeLast = headingLast_n - headingLast;


                        headingAndDistance[i].distance = Distance(p_p.lat, p_p.lon, p_i.lat, p_i.lon) + headingAndDistance[i-1].distance;
                       // headingAndDistance[i].curvature = headingLast - headingAndDistance[i - 1].curvature / traveledDistanceLast;
                        headingAndDistance[i].curvature = headingChangeLast / traveledDistanceLast;
                        headingAndDistance[i].heading = headingChangeLast;
                        headingAndDistance[i + 1].distance = headingAndDistance[i].distance + Distance(p_i.lat, p_i.lon, p_n.lat, p_n.lon);
                    }
                    else
                    {
                        traveledDistance = Distance(p_p.lat, p_p.lon, p_i.lat, p_i.lon) / 2 + Distance(p_i.lat, p_i.lon, p_n.lat, p_n.lon) / 2;
                        headingAndDistance[i].distance = Distance(p_p.lat, p_p.lon, p_i.lat, p_i.lon) + headingAndDistance[i-1].distance;
                        headingAndDistance[i].curvature = headingChange / traveledDistance;
                        headingAndDistance[i].heading = headingChange;
                        //headingAndDistance[i].curvature = heading - headingAndDistance[i-1].curvature / traveledDistance;
                    }
                }
                else
                {
                    Console.WriteLine("first point");
                }

            }

            return headingAndDistance;

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

        private static OpenDRIVERoad getRoadAttributes(KeyValuePair<way, List<node>> road, OpenDRIVERoad odRoad, double[] distanceVector)
        {
            if(road.Key.tag.Find(name => name.k == "name") != null)
            {
                int nameTagIndex = road.Key.tag.FindIndex(name => name.k == "name");
                odRoad.name = road.Key.tag[nameTagIndex].v;
            }
            odRoad.id = road.Key.id.ToString();
            odRoad.length = distanceVector.Last();
            //TO
            return odRoad;
            return odRoad;
        }
    }

}

