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

namespace Osm2Od
{
    public partial class CurvatureDistanceChart : Form
    {

        public double[] curvatureVector { get; set; }
        public double[] distanceVector { get; set; }
        public double[] junctionPoints_x { get; set; }
        public double[] junctionPoints_y { get; set; }
        Chart curvatureDistanceChart;
        public CurvatureDistanceChart(double[] curvatureVector, double[] distanceVector, double[] junctionPoints_x , double[] junctionPoints_y)
        {
            this.components = new System.ComponentModel.Container();
            bindDataToChart(curvatureVector, distanceVector, junctionPoints_x,junctionPoints_y);
            InitializeComponent();
            Application.Run(this);


        }

        public void CurvatureDistanceChart_Load(object sender, EventArgs e)
        {
            Console.WriteLine("LOADED");

            //ChartArea curvatureChartArea = new ChartArea();
            //Series curvature = new Series();
            //this.curvatureDistanceChart = new Chart();
            //((System.ComponentModel.ISupportInitialize)(this.curvatureDistanceChart)).BeginInit();
            //this.SuspendLayout();
            ////chart
            //curvature.Points.DataBindXY(this.curvatureVector, this.distanceVector);
            //curvature.ChartType = SeriesChartType.Line;
            //curvature.Color = Color.Red;
            //curvature.BorderWidth = 3;
            //this.curvatureDistanceChart.Series.Add(curvature);
            //this.curvatureDistanceChart.ChartAreas.Add(curvatureChartArea);
            //this.Controls.Add(this.curvatureDistanceChart);

            //((System.ComponentModel.ISupportInitialize)(this.curvatureDistanceChart)).EndInit();
            //curvatureDistanceChart.Invalidate();
            //Application.Run(this);

        }

        public void bindDataToChart(double[] curvatureVector, double[] distanceVector, double[] junctionPoints_x, double[] junctionPoints_y)
        {
            this.curvatureVector = curvatureVector;
            this.distanceVector = distanceVector;
            this.junctionPoints_x = junctionPoints_x;
            this.junctionPoints_y = junctionPoints_y;
        }

    }
}
