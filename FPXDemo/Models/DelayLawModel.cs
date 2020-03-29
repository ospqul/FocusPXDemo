using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FPXDemo.Models
{
    public static class DelayLawModel
    {
        public static List<Point> GetElementsPosition(IProbeModel probe)
        {
            List<Point> positions = new List<Point>();

            uint elementNumber = probe.UsedElementsPerBeam;
            double pitch = probe.Pitch;

            double range = (elementNumber - 1) * pitch;

            for (uint i=0; i< elementNumber; i++)
            {
                positions.Add(new Point(i * pitch - 0.5 * range, 0));
            }

            return positions;
        }

        public static double[] GetElementDelays(
            List<Point> elementsPosition, // mm
            double velocity, // m/s
            double trueDepth // mm
            )
        {
            var elementNumber = elementsPosition.Count();
            double[] elementDelays = new double[elementNumber];

            Point focalPoint = new Point(0, trueDepth);

            // Calculate each element's time of flight
            for (int i = 0; i < elementNumber; i++)
            {
                double distance = Point.Subtract(elementsPosition[i], focalPoint).Length; // mm
                double timeOfFlight = distance * 1e6 / velocity ; // nanoseconds
                elementDelays[i] = timeOfFlight;
            }

            // Calculate delay
            for (int i = 0; i < elementNumber; i++)
            {
                elementDelays[i] = elementDelays.Max() - elementDelays[i];
            }

            return elementDelays;
        }
    }
}
