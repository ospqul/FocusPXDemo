using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPXDemo.Models
{
    public static class DetectSignal
    {
        public static double CrossGateLocation(int[] data, GateModel gate)
        {
            double location = gate.Start;

            // loop from gate start to gate end, find the first location that value above threshold
            for (int i = (int)gate.Start; i < gate.Start + gate.Length; i++)
            {
                if (data[i] > gate.Threshold)
                {
                    location = i;
                    break;
                }
            }

            return location;
        }
    }
}
