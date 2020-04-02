## 11 Calculate Sscan Element delay.md

#### 11.1 SscanModel class

 ![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/sscan%20diagram.PNG)

As shown in the diagram, we need 4 properties to describe a S-scan: `StartAngle`, `EndAngle`, `AngleResolution`, and `FocusDepth`. Angle's unit is degree and depth's unit is mm.

Create a new `SscanModel` class.

```c#
# SscanModel.cs

public class SscanModel
{
    public double StartAngle { get; set; }// degree
    public double EndAngle { get; set; }
    public double AngleResolution { get; set; }
    public double FocusDepth { get; set; } //mm
}
```

#### 11.2 Calculate element delays for each beam

Since we already have the method to calculate b-scan focused beam element delays, we can make use of it to calculate element delays of every focused beam with a minor change:

Take `Point focalPoint` as argument instead of `double trueDepth`.

```c#
# DelayLawModel.cs

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
```

#### 11.3 Get beam angles

Calculate beam angles with sscanModel.

```c#
# DelayLawModel.cs
    
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
```

#### 11.4 Get S-scan element delays

When we calculate B-scan delays, all beam are 0 degree beam with same depth. Therefore, element delays are the same every beam, we can use one-dimension element delays for all beams.

But S-scan beams have different angles and focal point, element delays are different for each beam, so we need two-dimension delays, first dimension is beam, second dimension is element.

Please note that `Math.Tan()` take radius as argument, angle needs to convert from degree to radius.

```c#
# DelayLawModel.cs

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
```

#### 11.5 Verification

1. Create probe and sscanModel object in `ShellViewModel` class constructor.
2. Calculate element positions.
3. Calculate element delays.

```c#
# ShellViewModel.cs

public ShellViewModel()
{
    InitBscanPlot();

    // Init a probe
    probe = new ProbeModel
    {
        TotalElements = 64,
        UsedElementsPerBeam = 16,
        Frequency = 5,
        Pitch = 1,
    };

    // Init Sscan Settings
    sscanModel = new SscanModel
    {
        StartAngle = -45, // degree
        EndAngle = 45,
        AngleResolution = 1,
        FocusDepth = 17,  //mm
    };

    // Calculate sscan delays
    var positions = DelayLawModel.GetElementsPosition(probe);
    double velocity = 5800; // stainless steel block
    var delays = DelayLawModel.GetSscanDelays(positions, velocity, sscanModel);
}
```

Run the program, we can get debug messages from output window.

#### 11.6 Source Code

Run `git checkout 11_Calculate_Sscan_Element_delay` to get source code for this section.
