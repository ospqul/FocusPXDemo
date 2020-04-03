using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FPXDemo.Models
{
    public class SscanGraphModel
    {
        // Sscan Graph includes XAxis and YAxis
        public AxisModel XAxis { get; set; }
        public AxisModel YAxis { get; set; }

        // Get 2-d array plot points coordinates
        public Point[,] GetPlotPoints()
        {
            var XPoints = XAxis.GetPoints();
            var YPoints = YAxis.GetPoints();

            Point[,] points = new Point[XPoints.Count, YPoints.Count];

            for (int i = 0; i < XPoints.Count; i++)
            {
                for (int j = 0; j < YPoints.Count; j++)
                {
                    points[i, j] = new Point(XPoints[i], YPoints[j]);
                }
            }

            return points;
        }
    }
}
