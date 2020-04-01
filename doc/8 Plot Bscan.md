## 8 Plot Bscan

#### 8.1 Initialize Bscan Plot Graph method

Oxyplot library can also plot Heatmap. Official tutorial of heatmap plotting can be accessed from this [link](https://oxyplot.readthedocs.io/en/latest/models/series/HeatMapSeries.html).

```c#
# ShellViewModel.cs

public PlotModel plotModel { get; set; }
public HeatMapSeries heatMapSeries { get; set; }
public double[,] plotData = { };

public ShellViewModel()
{
    InitBscanPlot();
}

public void InitBscanPlot()
{
    plotModel = new PlotModel
    {
        Title = "Bscan Plotting",
    };

    // Add axis
    var axis = new LinearColorAxis();
    plotModel.Axes.Add(axis);

    // Add series
    heatMapSeries = new HeatMapSeries
    {
        X0 = 0,
        X1 = 25,
        Y0 = 0,
        Y1 = 5000,
        Interpolate = true,
        RenderMethod = HeatMapRenderMethod.Bitmap,
        Data = plotData,
    };
    plotModel.Series.Add(heatMapSeries);
}
```

#### 8.2 Plotting Bscan method

`StartBscan()` method creates two threads, one thread to read empty data buffer to prevent buffer overflow; the other thread to plot bscan.

`PlottingBscan` use a `while` loop to

1. collect `bscanData`
2. convert `int[][] bscanData` into `double[,] plotData`
3. refresh bscan image with `plotModel.InvalidatePlot(true);`

```c#
# ShellViewModel.cs

public void StartBscan()
{
    deviceModel.acquisition.ApplyConfiguration();
    deviceModel.acquisition.Start();

    var taskConsumeData = new Task(() => deviceModel.ConsumeData());
    taskConsumeData.Start();

    var taskPlottingBscan = new Task(() => PlottingBscan());
    taskPlottingBscan.Start();
}
    
public void PlottingBscan()
{
    int[][] bscanData;
    int plottingIndex = 0;

    while (true)
    {
        try
        {
            bscanData = deviceModel.CollectBscanData();
            if (bscanData.GetLength(0) < 1)
            { break; }

            plotData = new double[bscanData.GetLength(0), bscanData[0].GetLength(0)];

            for (int i=0; i< bscanData.GetLength(0); i++)
            {
                for (int j=0; j< bscanData[0].GetLength(0); j++)
                {
                    plotData[i, j] = (double)bscanData[i][j];
                }
            }
			// update plot image
            plotModel.InvalidatePlot(true);
            heatMapSeries.Data = plotData;
            plottingIndex += 1;
            Logging = plottingIndex.ToString();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }
}
```

#### 8.3 Configure Phased Array in ConnectDevice method

```c#
# ShellViewModel.cs
    
public async void ConnectDevice()
{
    //deviceModel = new DeviceModel();
    deviceModel = await Task.Run(() => new DeviceModel());
    SerialNumber = deviceModel.device.GetInfo().GetSerialNumber();
    IPAddress = deviceModel.device.GetInfo().GetAddressIPv4();

    // Create Conventional Beam Set
    //CreateBeamSet();
    //BindConnector();

    // Create PA Beam Set
    deviceModel.CreatPABeamSet();
    deviceModel.BindPAConnector();

    InitAcquisition();

    // Init Ascan Settings
    AscanGain = deviceModel.beamSet.GetBeam(0).GetGain().ToString();
    AscanLength = deviceModel.beamSet.GetBeam(0).GetAscanLength().ToString();

    // Plotting Ascan
    //StartAscan();

    // Plotting Bscan
    StartBscan();
}
```

#### 8.4 Change Settings for all beams

1. Get the number of total beams: 
   `var beamCount = deviceModel.beamSet.GetBeamCount();`
2. Loop each beam and set value:
   `deviceModel.beamSet.GetBeam(i).SetGainEx(result);`  or
   `deviceModel.beamSet.GetBeam(i).SetAscanLength(result);`
3. Apply change:
   `deviceModel.acquisition.ApplyConfiguration();`

```c#
public string AscanGain
{
    get { return _ascanGain; }
    set
    {
        _ascanGain = value;
        NotifyOfPropertyChange(() => AscanGain);
        if (double.TryParse(_ascanGain, out double result))
        {
            var beamCount = deviceModel.beamSet.GetBeamCount();
            for (uint i = 0; i < beamCount; i++)
            {
                deviceModel.beamSet.GetBeam(i).SetGainEx(result);
            }
            deviceModel.acquisition.ApplyConfiguration();
        }
    }
}

public string AscanLength
{
    get { return _ascanLength; }
    set
    {
        _ascanLength = value;
        NotifyOfPropertyChange(() => AscanLength);
        if (double.TryParse(_ascanLength, out double result))
        {
            var beamCount = deviceModel.beamSet.GetBeamCount();
            for (uint i = 0; i < beamCount; i++)
            {
                deviceModel.beamSet.GetBeam(i).SetAscanLength(result);
            }
            deviceModel.acquisition.ApplyConfiguration();
        }
    }
}
```

#### 8.5 Unfocused Bscan

The indication in test block is a dot, but we only see line segment in bscan image. This is because we didn't set element delays, we were using unfocused beam. In next lesson, we can learn how to set element delays to generate focused beams.

![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/Unfocused%20Bscan.PNG)

#### 8.6 Source Code

Run `git checkout 8_Plot_Bscan` to get source code for this section.
