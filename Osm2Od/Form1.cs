using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using Osm2Od;
using System.Device.Location;
using ConsoleApp1;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;
using System.Drawing;


namespace Osm2Od
{
    public partial class Form1 : Form
    {
        public Dictionary<ulong, List<node>> waysNodesDict { get; set; }
        public Osm2OdConverter converterHandler { get; set; }
        public Tuple<Double,Double> originPoint { get; set; }
        public Form1()
        {
            InitializeComponent();
            InitilizeOSM2OdModule();
            comboBox1.DisplayMember = "Key";
            comboBox1.ValueMember = "Value";
            comboBox1.DataSource = new BindingSource(this.waysNodesDict, null);


        }

        void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<node> selectedWayNodes = ((KeyValuePair<ulong,List<node>>)comboBox1.SelectedItem).Value;
            ulong selectedWay = ((KeyValuePair<ulong, List<node>>)comboBox1.SelectedItem).Key;
            Console.WriteLine("Nicely Done You Imported This Way Data");
            this.graphRoad(selectedWay);
        }

        public void InitilizeOSM2OdModule()
        {
            Console.WriteLine("[*] Welcome to OSM2OD converter\n Please insert valid path to OSM XML schema:\n >> ");
            string osmFilePath = Path.GetFullPath(@"C:\vORLESUNGEN\Studienarbeit\map (3).osm");//Console.ReadLine();
            Console.WriteLine("File Path Valid, Analysing the file .... ");
            osm osmResult = null;

            XmlSerializer serializer = new XmlSerializer(typeof(osm));

            using (FileStream fileStream = new FileStream(osmFilePath, FileMode.Open))
            {
                osmResult = (osm)serializer.Deserialize(fileStream);
                Console.WriteLine("File is being Analysed .... ");

            }

            this.converterHandler = new Osm2OdConverter();
            if (osmResult != null)
            {
                converterHandler.OsmModel = osmResult;
                this.originPoint = converterHandler.calculateBounds();
                this.waysNodesDict = converterHandler.convertRoads();
            }

        }

        public void graphRoad(ulong road)
        {
            //populate dataset with some demo data..
            DataSet dataSet = new DataSet();
            DataTable dt = new DataTable();
            dt.Columns.Add("x", typeof(double));
            dt.Columns.Add("y", typeof(double));

            foreach (node roadNodes in this.waysNodesDict[road])
            {
                DataRow r1 = dt.NewRow();

                double xRoadNode = roadNodes.lat - this.originPoint.Item1;
                double yRoadNode = roadNodes.lon - this.originPoint.Item2;

                var xyRoadNode = new GeoCoordinate(xRoadNode, yRoadNode);

                r1[0] = xyRoadNode.Latitude;
                r1[1] = xyRoadNode.Latitude;
                dt.Rows.Add(r1);
            }
            dataSet.Tables.Add(dt);


            //prepare chart control...
            Chart chart = this.chart1;
            chart.DataSource = dataSet.Tables[0];
            chart.Width = 1300;
            chart.Height = 800;
            //create serie...


            //databind...
            chart.DataBind();
            //save result...
            //chart.SaveImage(@"c:\myChart.png", ChartImageFormat.Png);
        }
    }
}
