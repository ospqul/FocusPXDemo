## 13 Plot Sscan without correction

In [Lesson 10](https://github.com/ospqul/FocusPXDemo/blob/master/doc/10 Plot and Compare Focused and Unfocused Bscan.md) B-scan's every beam is 0 degree angle, so its plot image is a rectangle. However, each beam in S-scan has a different angle, therefore, s-scan plot image should be a sector shape.

In our example below, plot area X range is (-20, 20), Y range is (0, 50), distance between plot point is 0.1 mm. So we have 401 x 501 plot points. If plot point locates beyond plotting area, then it's plot value = 0; if plot point locates within plotting area, then we calculate its angle and distance to the center of probe, and find the nearest value in Ascan.

#### 13.1 Create AxisModel class

Create a new class `AxisModel` to describe plot area's axis. It has three properties: `Min` value, `Max` value and `Resolution`. Their units are mm.

Also, `AxisModel` has a method to get a list of points with same distance.

```c#
# AxisModel.cs
    
public class AxisModel
{
    public double Min { get; set; } // mm
    public double Max { get; set; } // mm
    public double Resolution { get; set; } // mm

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
```

#### 13.2 Create SscanGraphModel class

Create a new method `SscanGraphModel` to describe the S-scan plot area. It has two properties: `XAxis` and `YAxis`. Also, `SscanGraphModel` has a method to get the positions of all plot points.

```c#
# SscanGraphModel.cs

public class SscanGraphModel
{
    public AxisModel XAxis { get; set; }
    public AxisModel YAxis { get; set; }

    public Point[,] GetPlotPoints()
    {            
        var XPoints = XAxis.GetPoints();
        var YPoints = YAxis.GetPoints();

        Point[,] points = new Point[XPoints.Count, YPoints.Count];

        for (int i=0; i<XPoints.Count; i++)
        {
            for (int j=0; j<YPoints.Count; j++)
            {
                points[i, j] = new Point(XPoints[i], YPoints[j]);
            }
        }
        return points;
    }
}
```

#### 13.3 Initiate S-scan Graph

```c#
# ShellViewModel.cs

public void InitSscanGraph()
{
    AxisModel XAxis = new AxisModel
    {
        Min = -20, // mm
        Max = 20, // mm
        Resolution = 0.1 // mm
    };

    AxisModel YAxis = new AxisModel
    {
        Min = 0, // mm
        Max = 50, // mm
        Resolution = 0.1 // mm
    };

    sscanGraph = new SscanGraphModel
    {
        XAxis = XAxis,
        YAxis = YAxis,
    };
}
```

#### 13.4 Initiate S-scan Plot

Initiate `HeatMapSeries` with `sscanGraph`'s XAxis and YAxis range.

```c#
# ShellViewModel.cs
    
public void InitSscanPlot()
{
    plotModel = new PlotModel
    {
        Title = "Sscan Plotting",
    };

    // Add axis
    var axis = new LinearColorAxis();
    plotModel.Axes.Add(axis);

    // Add series
    heatMapSeries = new HeatMapSeries
    {
        X0 = sscanGraph.XAxis.Min,
        X1 = sscanGraph.XAxis.Max,
        Y0 = sscanGraph.YAxis.Min,
        Y1 = sscanGraph.YAxis.Max,
        Interpolate = true,
        RenderMethod = HeatMapRenderMethod.Bitmap,
        Data = plotData,
    };
    plotModel.Series.Add(heatMapSeries);
}
```

#### 13.5 Create StartSscan method

Create a new `StartSscan()` method to create consume data thread and plotting Sscan thread.

```c#
# ShellViewModel.cs

public void StartSscan()
{
    deviceModel.acquisition.ApplyConfiguration();
    deviceModel.acquisition.Start();

    var taskConsumeData = new Task(() => deviceModel.ConsumeData());
    taskConsumeData.Start();

    var taskPlottingSscan = new Task(() => PlottingSscan());
    taskPlottingSscan.Start();
}
```

#### 13.6 Plot S-scan

Create a new method`PlottingSscan()`, it has a similar structure as `PlottingBscan()`.

1. Read 2-d array data from FPX

   ```c#
   rawData = deviceModel.CollectBscanData();
   if (rawData.GetLength(0) < 1)
   { break; }
   ```

2. Get plot points positions and initialize `plotData` with the same size.

   ```c#
   var plotPoints = sscanGraph.GetPlotPoints();
   plotData = new double[plotPoints.GetLength(0), plotPoints.GetLength(1)];
   ```

3. Loop every plot point and assign a value to `plotData`.

   ```c#
   for (int xIndex=0; xIndex< plotPoints.GetLength(0); xIndex++)
   {
       for (int yIndex=0; yIndex< plotPoints.GetLength(1); yIndex++)
       {
           // assign plotData value
           ...
       }
   }
   ```

4. Calculate plot point's angle

   ```c#
   var p = plotPoints[xIndex, yIndex];
   double angle = Math.Atan2(p.X, p.Y) * 180 / Math.PI;
   ```

5. If angle is beyond plot area, assign `plotData` value to 0; if angle is within plot area, calculate time of flight from center to this plot point. Please note that when calculate time of flight, you should double the distance.

   ```c#
   if ((angle < sscanModel.StartAngle) || (angle > sscanModel.EndAngle))
   {
       plotData[xIndex, yIndex] = 0;
   }
   else
   {
       double radius = Math.Sqrt(p.X * p.X + p.Y * p.Y);
       double velocity = 5800; // m/s
       double TOF = radius * 2e6 / velocity; // nanoseconds
       // Find and assgin nearest value in rawData
       ...
   }
   ```

6. Find the nearest value in `rawData` and assign to `plotData`.

   ```c#
   // calculate beam index
   int rawXIndex = (int)Math.Round((angle - sscanModel.StartAngle) / sscanModel.AngleResolution); 
   // calculate Ascan Data index
   int rawYIndex = (int)Math.Round(TOF / 10); // 10 nanoseconds per Ascan data point
   plotData[xIndex, yIndex] = Math.Abs(rawData[rawXIndex][rawYIndex]);
   ```



Here is the full code of `PlottingSscan()` method.

```c#
# ShellViewModel.cs

public void PlottingSscan()
{
    int[][] rawData;
    int plottingIndex = 0;

    while (true)
    {
        try
        {
            rawData = deviceModel.CollectBscanData();
            if (rawData.GetLength(0) < 1)
            { break; }

            var plotPoints = sscanGraph.GetPlotPoints();

            plotData = new double[plotPoints.GetLength(0), plotPoints.GetLength(1)];

            for (int xIndex=0; xIndex< plotPoints.GetLength(0); xIndex++)
            {
                for (int yIndex=0; yIndex< plotPoints.GetLength(1); yIndex++)
                {
                    var p = plotPoints[xIndex, yIndex];
                    double angle = Math.Atan2(p.X, p.Y) * 180 / Math.PI;
                    if ((angle < sscanModel.StartAngle) || (angle > sscanModel.EndAngle))
                    {
                        plotData[xIndex, yIndex] = 0;
                    }
                    else
                    {
                        double radius = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                        double velocity = 5800; // m/s
                        double TOF = radius * 2e6 / velocity; // nanoseconds  
                        int rawXIndex = (int)Math.Round((angle - sscanModel.StartAngle) / sscanModel.AngleResolution);
                        int rawYIndex = (int)Math.Round(TOF / 10);
                        plotData[xIndex, yIndex] = Math.Abs(rawData[rawXIndex][rawYIndex]);
                    }
                }
            }
            plotModel.InvalidatePlot(true);
            heatMapSeries.Data = plotData;

            plottingIndex += 1;
            Logging = plottingIndex.ToString();
        }
        catch (Exception)
        {
            //throw;
        }
    }
}
```

#### 13.6 Plot Sscan

![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/Sscan%20before%20correction.PNG)

#### 13.7 Source Code

Run `git checkout 13_Plot_Sscan_without_correction` to get source code for this section.