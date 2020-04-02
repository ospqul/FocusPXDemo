using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FPXDemo.Models
{
    public static class DelayLawModel
    {
        // Calculate element positions
        public static List<Point> GetElementsPosition(ProbeModel probe)
        {
            List<Point> positions = new List<Point>();

            uint elemNum = probe.UsedElementsPerBeam;
            double pitch = probe.Pitch;
            double range = pitch * (elemNum - 1);
            double midRange = range / 2;

            for (uint i = 0; i < elemNum; i++)
            {
                Point e = new Point(i * pitch - midRange, 0);
                positions.Add(e);
            }

            for (int i=0; i<positions.Count; i++)
            {
                Debug.WriteLine($"Element {i} position: X={positions[i].X} Y ={positions[i].Y}");
            }

            return positions;
        }

        // Calculate element delays
        public static double[] GetElementDelays(
            List<Point> elementPositions, // elements' positions
            double velocity, // Test block's velocity, in m/s
            //double trueDepth // Focal point's true depth, in mm
            Point focalPoint // Focal point's position in mm
            )
        {
            var elementNumber = elementPositions.Count;
            double[] elementDelays = new double[elementNumber];

            //Point focalPoint = new Point(0, trueDepth);

            // Calculate each element's time of flight to focal point
            for (int i=0; i< elementNumber; i++)
            {
                double distance = Point.Subtract(elementPositions[i], focalPoint).Length; //mm
                double tof = distance * 1e6 / velocity; // nano seconds
                elementDelays[i] = tof;
            }

            // Find max time of flight of elements and calculatet delay for each element
            // We need all pulser signals reach focal point at the same time
            double maxTOF = elementDelays.Max();
            for (int i = 0; i < elementNumber; i++)
            {
                elementDelays[i] = maxTOF - elementDelays[i];
            }

            for (int i = 0; i < elementNumber; i++)
            {
                Debug.WriteLine($"Element {i} delay: {elementDelays[i]}");
            }

            return elementDelays;
        }

        // calculate angles
        public static List<double> GetSscanAngles(SscanModel sscanModel)
        {
            List<double> angles = new List<double>();
            var angle = sscanModel.StartAngle;
            while (angle <= sscanModel.EndAngle)
            {
                angles.Add(angle);
                angle = angle + sscanModel.AngleResolution;
            }
            return angles;
        }

        // calculate sscan element delays
        // Every beam's element delays are different
        // so Sscan delay is a two dimension array
        // first dimension is beam, second dimension is element
        public static double[][] GetSscanDelays(
            List<Point> elementPositions, // elements' positions
            double velocity, // Test block's velocity, in m/s
            SscanModel sscanModel // Focal point's true depth, in mm
            )
        {
            var angles = GetSscanAngles(sscanModel);
            double[][] delays = new double[angles.Count][];
            int elemNum = elementPositions.Count;

            // Loop through each angle beam
            for (int angleIndex=0; angleIndex< angles.Count; angleIndex++)
            {
                // calculate focal point
                double focalY = sscanModel.FocusDepth;
                // focalX = Tan(angle) * Focus depth
                double focalX = Math.Tan(angles[angleIndex] * Math.PI / 180)
                    * sscanModel.FocusDepth;
                Point focalPoint = new Point(focalX, focalY);

                Debug.WriteLine($"Beam Angle: {angles[angleIndex]}," +
                    $" Focal Point: X {focalPoint.X} Y {focalPoint.Y}");

                // calculate delays
                delays[angleIndex] = GetElementDelays(elementPositions, velocity, focalPoint);
            }

            return delays;
        }
    }
}
