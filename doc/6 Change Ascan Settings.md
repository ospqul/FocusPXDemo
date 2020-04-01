## 6 Change Ascan Settings

Since we are already very familiar with the steps for data acquisition, let's clear up the GUI and reduce manual work first.

Let's add the 4 steps into `ConnectDevice()` so that when we click `Connect Device` button, we start plotting Ascan directly.

```c#
# ShellViewModel.cs        
public async void ConnectDevice()
{
    //deviceModel = new DeviceModel();
    deviceModel = await Task.Run(() => new DeviceModel());
    SerialNumber = deviceModel.device.GetInfo().GetSerialNumber();
    IPAddress = deviceModel.device.GetInfo().GetAddressIPv4();
    CreateBeamSet();
    BindConnector();
    InitAcquisition();
    StartAscan();
}
```

Comment Row 2, Row3 and Row4 in `ShellView.xaml`.

#### 6.1 Add Text Boxes for Ascan Gain and Length

```xaml
# ShellView.xaml
<!-- Row6: Ascan Settings -->
<StackPanel Grid.Column="1" Grid.Row="6" Orientation="Horizontal"
            Margin="5">
    <Label Content="Gain"/>
    <TextBox x:Name="AscanGain" MinWidth="50" />

</StackPanel>
<StackPanel Grid.Column="2" Grid.Row="6" Orientation="Horizontal"
            Margin="5">
    <Label Content="Ascan Length" />
    <TextBox x:Name="AscanLength" MinWidth="50"/>
</StackPanel>
```

#### 6.2 Add Code Logic

When value is changed, if the new value is `double`, then set the value accordingly. Remember to apply configuration after new value is set.

```c#
# ShellViewModel.cs
private string _ascanGain;

public string AscanGain
{
    get { return _ascanGain; }
    set
    {
        _ascanGain = value;
        NotifyOfPropertyChange(() => AscanGain);
        if (double.TryParse(_ascanGain, out double result))
        {
            deviceModel.beamSet.GetBeam(0).SetGainEx(result);
            deviceModel.acquisition.ApplyConfiguration();
        }
    }
}

private string _ascanLenght;

public string AscanLength
{
    get { return _ascanLenght; }
    set
    {
        _ascanLenght = value;
        NotifyOfPropertyChange(() => AscanLength);
        if (double.TryParse(_ascanLenght, out double result))
        {
            deviceModel.beamSet.GetBeam(0).SetAscanLength(result);
            deviceModel.acquisition.ApplyConfiguration();
        }
    }
}
```

#### 6.3 Read initial value

When connect device, we would like to see Ascan Gain and Ascan Length's default values.

```c#
# ShellViewModel.cs
public async void ConnectDevice()
{
	...
    InitAcquisition();
    
    AscanGain = deviceModel.beamSet.GetBeam(0).GetGain().ToString();
    AscanLength = deviceModel.beamSet.GetBeam(0).GetAscanLength().ToString();
    ...
}
```

#### 6.4 Improve threading tasks

As you may have noticed that when we change the Gain value or Ascan Length value, the plotting sometimes will stop. That's because when we apply changes, there may be loss of data that would cause some issue to stop the threading. So let's make some improvements.

Use `try { ... } catch { ... }` to prevent accidental exceptions.

```c#
# DeviceModel.cs
public void ConsumeData()
{
    while (true)
    {
        try
        {
            var dataResult = acquisition.WaitForDataEx();
            if (dataResult.status == IAcquisition.WaitForDataResultEx.Status.DataAvailable)
            {
                using (var cycleData = dataResult.cycleData)
                {
                    dataResult = acquisition.WaitForDataEx();
                    dataResult.Dispose();
                }
            }                    
        }
        catch
        { }
    }
}
```

```c#
# ShellViewModel.cs   
public void PlottingAscan()
{
    int[] ascanData;
    int plottingIndex = 0;

    while (true)
    {
        try
        {
            ascanData = deviceModel.CollectAscanData();
            lineSeries.Points.Clear();
            for (int i = 0; i < ascanData.GetLength(0); i++)
            {
                lineSeries.Points.Add(new DataPoint(i, ascanData[i]));
            }
            plotModel.InvalidatePlot(true);
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
#### 6.5 Source Code

Run `git checkout 6_Change_Ascan_Settings` to get source code for this section.
