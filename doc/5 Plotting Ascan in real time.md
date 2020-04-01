## 5 Plotting Ascan in real time

The acquisition process has been explained in [3.5 Collect Cycle Data](#3.5-collect-cycle-data). Basically, we need to initiate two threads: one thread is reading/consuming data continuously to prevent buffer overflow; the main thread is reading data and plotting graph.

#### 5.1 Consume Data Function

Create a `ConsumeData()` function in `DeviceModel` class to read data from buffer. While the status is `DataAvailable`, use `cycleData = dataResult.cycleData` to read out cycle data and do nothing else. This function finishes when there is no new data available.

```c#
# DeviceModel.cs        
public void ConsumeData()
{
    try
    {
        var dataResult = acquisition.WaitForDataEx();
        while (dataResult.status == IAcquisition.WaitForDataResultEx.Status.DataAvailable)
        {
            using (var cycleData = dataResult.cycleData)
                dataResult = acquisition.WaitForDataEx();
        }
        dataResult.Dispose();
    }
    catch (Exception e)
    {
        MessageBox.Show(e.ToString());
    }
}
```

#### 5.2 Plotting Function

Create a `PlottingAscan()` function in `ShellViewModel` class to read data and plot. While loop read data and plot graph. This function finishes when there is no new data available.

Also please note that before plotting a new Ascan, it is necessary to use `lineSeries.Points.Clear()` clear all old points; otherwise, new plotting lines will just stack on old lines.

```c#
# ShellViewModel.cs       
public void PlottingAscan()
{
    int[] ascanData;
    while (true)
    {
        ascanData = deviceModel.CollectAscanData();
        if (ascanData.GetLength(0) < 1)
        { break; }
        
		lineSeries.Points.Clear();
        
        for (int i = 0; i < ascanData.GetLength(0); i++)
        {
            lineSeries.Points.Add(new DataPoint(i, ascanData[i]));
        }
        plotModel.InvalidatePlot(true);
    }
}
```

#### 5.3 Create Start and Stop button in GUI

```xaml
# ShellView.xaml
<!-- Row 5: Plot Ascan -->
<Button x:Name="PlotAscan" Content="Plot Ascan"
        Grid.Column="1" Grid.Row="5" Margin="5"/>
<StackPanel Grid.Column="2" Grid.Row="5" Orientation="Horizontal">
    <Button x:Name="StartAscan" Content="Start Ascan" Margin="5"/>
    <Button x:Name="StopAscan" Content="Start Ascan" Margin="5"/>
</StackPanel>
```

#### 5.4 Complete Start and Stop Logic

Still, we need to apply configuration and start acquisition first. Then, we initiate two `Task` objects right after start acquisition to execute `deviceModel.ConsumeData()` and `PlottingAscan()`.

```c#
# ShellViewModel.cs       
public void StartAscan()
{
    deviceModel.acquisition.ApplyConfiguration();
    deviceModel.acquisition.Start();

    var taskConsumeData = new Task(() => deviceModel.ConsumeData());
    taskConsumeData.Start();

    var taskPlottingAscan = new Task(() => PlottingAscan());
    taskPlottingAscan.Start();
}
```

If we want to stop, we just stop acquisition, because the two tasks will complete if there is no more data available.

```c#
# ShellViewModel.cs  
public void StopAscan()
{
	deviceModel.acquisition.Stop();
}
```

#### 5.5 Make your app more responsive

You may have noticed that every time we hit the `Connect Device` button, this app will be "dead" or nonresponsive for a few seconds. This is because Focus PX needs a few seconds for initialization. Since we have learned how to use C# `Task` object to execute tasks asynchronously, we can also use it to make our app more responsive during Focus PX initialization.

- `Task.Run()` will execute `new DeviceModel()` asynchronously.
- `await` keyword will ask app to wait till `new DeviceModel()` completes, and then execute the following code.
- Use the `async` modifier to specify `ConnectDevice()` is asynchronous.

```c#
# ShellViewModel.cs
public async void ConnectDevice()
{
    // deviceModel = new DeviceModel();
    deviceModel = await Task.Run(() => new DeviceModel());
    ...
}
```

#### 5.6 Source Code

Run `git checkout 5_plotting_Ascan_in_real_time` to get source code for this section.
