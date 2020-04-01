## 9 Calculate Focused Beam Delays
![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/focused%20beam%200%20degree.PNG)
#### 9.1 Create a probe model

Add a class named `ProbeModel.cs`.

```c#
# ProbeModel.cs

namespace FPXDemo.Models
{
    public class ProbeModel
    {
        public uint TotalElements { get; set; }
        public uint UsedElementsPerBeam { get; set; }
        public double Pitch { get; set; } // mm
        public double Frequency { get; set; } // MHz
    }
}
```

#### 9.2 Create delay law model

Add a class named `DelayLawModel.cs`, and make it `static`.

```c#
# DelayLawModel.cs

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
    }
}
```

#### 9.3 Calculate probe element positions

Assume we have a phased array probe with 32 elements, pitch is 1mm. We use 8 elements as a group to fire a beam. Make the middle point of this group of 8 elements as zero point, coordinate (0, 0).

Then the elements' coordinates are:

- element 1:  (-3.5, 0)
- element 2:  (-2.5, 0)
- ...
- element 8: (3.5, 0)

```c#
# DelayLawModel.cs

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

    return positions;
}
```

We can also print out the positions to debug message.

```c#
for (int i=0; i<positions.Count; i++)
{
    Debug.WriteLine($"Element {i} position: X={positions[i].X} Y ={positions[i].Y}");
}
```

#### 9.4 Calculate Time of Flight and delays

Assume we are focusing at depth 17 mm, then focal point is (0, -17). Then we go through the following steps to decide the delay for each element:

1. Distance between element and focal point:
   The distance between element 1 (-3.5, 0) and focal point (0, -17) is about 17.36 mm
2. Find sound velocity:
   Assume the test block is made of stainless steel, whose sound velocity is 5800 m/s.
3. Time of flight:
   Element1 TOF = distance / velocity = (17.36 mm) / (5800 m/s) = 2993 nanoseconds.
4. Find Max Time of flight:
   Element 1 and 8 are the furthest elements to focal point, maxTOF = 2993 ns.
5. Add delay to other elements:
   Element 2 TOF = 2963 ns
   Delay 2 = 2993 - 2963 = 30 ns.

```c#
// Calculate element delays
public static double[] GetElementDelays(
    List<Point> elementPositions, // elements' positions
    double velocity, // Test block's velocity, in m/s
    double trueDepth // Focal point's true depth, in mm
	)
{
    var elementNumber = elementPositions.Count;
    double[] elementDelays = new double[elementNumber];

    Point focalPoint = new Point(0, trueDepth);

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

    return elementDelays;
}
```

We can also print out element delays to debug message.

```c#
for (int i = 0; i < elementNumber; i++)
{
    Debug.WriteLine($"Element {i} delay: {elementDelays[i]}");
}
```

#### 9.5 Verification

1. Create a probe object in `ShellViewModel` class constructor.
2. Calculate element positions.
3. Calculate element delays.

```c#
# ShellViewModel.cs

public ShellViewModel()
{
    InitBscanPlot();

    // Create a probe object
    ProbeModel probe = new ProbeModel
    {
        TotalElements = 32,
        UsedElementsPerBeam = 8,
        Frequency = 5,
        Pitch = 1,
    };

    // Calculate element positions
    var positions = DelayLawModel.GetElementsPosition(probe);

    // Calculate element delays
    double velocity = 5800; // stainless steel block
    double depth = 17; // mm
    var delays = DelayLawModel.GetElementDelays(positions, velocity, depth);
}
```

Run the program, we can get debug messages:

```c
# Output window

Element 0 position: X=-3.5 Y =0
Element 1 position: X=-2.5 Y =0
Element 2 position: X=-1.5 Y =0
Element 3 position: X=-0.5 Y =0
Element 4 position: X=0.5 Y =0
Element 5 position: X=1.5 Y =0
Element 6 position: X=2.5 Y =0
Element 7 position: X=3.5 Y =0
Element 0 delay: 0
Element 1 delay: 29.9508069431381
Element 2 delay: 50.0873779023741
Element 3 delay: 60.2075182784192
Element 4 delay: 60.2075182784192
Element 5 delay: 50.0873779023741
Element 6 delay: 29.9508069431381
Element 7 delay: 0
```

#### 9.6 Source Code

Run `git checkout 9_Calculate_focused_beam_delays` to get source code for this section.
