using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPXDemo.Models
{
    public class SscanModel
    {
        public double StartAngle { get; set; }// degree
        public double EndAngle { get; set; }
        public double AngleResolution { get; set; }
        public double FocusDepth { get; set; } //mm
    }
}
