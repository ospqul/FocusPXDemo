using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPXDemo.Models
{
    public class ProbeModel : IProbeModel
    {
        public uint TotalElements { get; set; }
        public uint UsedElementsPerBeam { get; set; }
        public double Pitch { get; set; }
        public double Frequency { get; set; }
    }
}
