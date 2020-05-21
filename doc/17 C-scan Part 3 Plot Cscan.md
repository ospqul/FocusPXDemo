## 17 C-scan Part3 Plot Cscan

#### 17.1 Heatmap Library

How to paint pixel by pixel in .Net WPF is not the main purpose of this training, therefore, you could develop your own way to plot a C-scan image, or you could add reference to [Heatmap Lirary](https://github.com/ospqul/FocusPXDemo/tree/master/Heatmap) in project and follow this tutorial to paint C-scan image.

#### 17.2 Gate Logic to Get Cscan value

Loop data from gate starting point till end, if data value is about threshold, return its location.

![Gate](https://github.com/ospqul/FocusPXDemo/blob/master/resources/CscanGate.PNG)

```c#
// DetectSignal.cs

public static class DetectSignal
{
    public static double CrossGateLocation(int[] data, GateModel gate)
    {
        double location = gate.Start;
        // loop from gate start to gate end
        //find the first location that value above threshold
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
```

#### 17.3 CscanModel Class

We need 2 properties to describe a C-scan plotting:

1. Width
2. Height

```c#
# CscanModel.cs

public class CscanModel
{
	public int Width { get; set; }
	public int Height { get; set; }
}
```

#### 17.4 Add Cscan Image on GUI

Bind `heatmapGraph` in `ShellViewModel.cs`.

```xaml
# ShellView.xaml

<!-- Cscan -->
<Image Source="{Binding heatmapGraph}" Margin="5" Height="200"/>
```

```c#
// ShellViewModel.cs

public HeatmapModel heatmapModel { get; set; }
private BitmapSource _heatmapGraph;

public BitmapSource heatmapGraph
{
    get { return _heatmapGraph; }
    set
    {
        _heatmapGraph = value;
        NotifyOfPropertyChange(() => heatmapGraph);
    }
}
```

#### 17.5 Initialize C-scan Model

In this example, we will do one-line C-scan, so we set C-scan image height to the number of beams. So each beam will paint a row, and each cycle data will paint a column.

```c#
// ShellViewModel.cs

public CscanModel cscanModel { get; set; }

public void InitCscan()
{
    cscanModel = new CscanModel
    {
        Width = 400,
        // Height = number of beams
        Height = (int)(probe.TotalElements - probe.UsedElementsPerBeam + 1),
    };
    // initialize a heatmap object
    heatmapModel = new HeatmapModel(cscanModel.Width, cscanModel.Height);
	// update heatmapGraph in GUI
    heatmapGraph = heatmapModel.BitmapToImageSource();
}
```

#### 17.6 Plot C-scan method

For each cycle data collected from Focus PX, one column of pixels are painted based on C-scan values. In this example, we have defined C-scan image width to 400, meaning that we need to collect 400 cycles of data to plot a full C-scan image.

`xPos` defines which column should be painted.

```c#
// ShellViewModel.cs

public void PlotCscan(int[][] data, int xPos)
{
    //  this color palette has 256 colors
    int colorNumber = heatmapModel.colorList.Count();
	// loop through each beam to get C-scan value and paint pixel
    for (int i=0; i<data.GetLength(0); i++)
    {
        // Get cscan value with gate
        var loc = DetectSignal.CrossGateLocation(data[i], gate);
        // if signal locates at the start of the gate, paint it with color[0]
        // if signal locates at the end of the gate, paint it with color[255]
        var color = (loc - gate.Start) * colorNumber / gate.Length;
        // Paint
        heatmapModel.PaintHeatMapPoint(new HeatmapPoint(xPos, i, (int)color));
    }
    // update cscan plotting
    heatmapGraph = heatmapModel.BitmapToImageSource();
}
```

#### 17.6 Add Plot C-scan into Plotting

Add `PlotCscan()` into `Plotting()` so that each data acquisition cycle will plot both A-scan and C-scan.

When `plottingIndex` is beyond C-scan plotting area, `plottingIndex` is reset to 0, and C-scan is plotting from the beginning.

```c#
// ShellViewModel.cs
    
public void Plotting()
{
    int[][] rawData;
    int plottingIndex = 0;

    while (true)
    {
        try
        {
            rawData = deviceModel.CollectRawData();
            // Plot Ascan
            PlotAscan(rawData[0]); // plot the first Beam
            // Plot Cscan
            PlotCscan(rawData, plottingIndex);
            // start to plot from beginning when cscan ends
            if (plottingIndex > cscanModel.Width)
            {
                plottingIndex = 0;
            }
            else
            {
                plottingIndex += 1;
            }
            Logging = plottingIndex.ToString();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }
}
```

#### 17.8 Plot C-scan

We use a 32-element probe to test, and a test block with an indication located at 17mm.

![PlotCscan](https://github.com/ospqul/FocusPXDemo/blob/master/resources/PlotCscan.PNG)

When moving the probe over indication, you could see the indication moves from top to bottom in the C-scan image. Next section will use a corrosion test piece to demonstrate a good example of corrosion inspection.

![CscanUI](https://github.com/ospqul/FocusPXDemo/blob/master/resources/PlotCscanUI.PNG)

#### 17.7 Source Code

Run `git checkout 17_Cscan_Part3_Plot_Cscan` to get source code for this section.

