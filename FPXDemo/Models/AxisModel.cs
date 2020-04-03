using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPXDemo.Models
{
    public class AxisModel
    {
        public double Min { get; set; } // mm
        public double Max { get; set; } // mm
        public double Resolution { get; set; } // mm

        // For example, Min = -3 Max =3, Res = 1
        // We get points: (-3, -2, -1, 0, 1, 2, 3)
        public List<double> GetPoints()
        {
            List<double> points = new List<double>();
            int numberOfPoints = (int)Math.Floor((Max - Min) / Resolution);
            for (int i=0; i<numberOfPoints; i++)
            {
                points.Add(Min + i * Resolution);
            }
            return points;
        }
    }
}
