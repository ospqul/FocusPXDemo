## 3 Get Ascan Data

Ascan Data is the raw data of Focus PX. For each firing beam, you will get a 1-d array Ascan data. There are a few steps before we can collect Ascan Data.

#### 3.1 Start Device Firmware Package

Use `IFirmwarePackageScanner.GetFirmwarePackageCollection()` to find all available firmware packages in FocusPX. Check if `FocusPxPackage-1.3` is in Focus PX, then start with this firmware.

Add `DownloadFirmwarePackage()` to `DeviceModel` constructor.

```c#
# DeviceModel.cs
public DeviceModel()
{
    ...
    DownloadFirmwarePackage();
}
    
public void DownloadFirmwarePackage()
{
    string packageName = "FocusPxPackage-1.3";
    IFirmwarePackage firmwarePackage;
    IFirmwarePackageCollection firmwarePackages = IFirmwarePackageScanner.GetFirmwarePackageCollection();
    for (uint i=0; i<firmwarePackages.GetCount(); i++)
    {
        if (firmwarePackages.GetFirmwarePackage(i).GetName().Contains(packageName))
        {
            firmwarePackage = firmwarePackages.GetFirmwarePackage(i);
            device.Start(firmwarePackage);
            break;
        }
    }
}
```

#### 3.2 Create BeamSet

Get Device configuration and set Conventional Beamset.

```c#
# DeviceModel.cs
public void CreatBeamSet()
{
    IDeviceConfiguration deviceConfiguration = device.GetConfiguration();
    ultrasoundConfiguration = deviceConfiguration.GetUltrasoundConfiguration();
    digitizerTechnology = ultrasoundConfiguration.GetDigitizerTechnology(UltrasoundTechnology.Conventional);
    IBeamSetFactory beamSetFactory = digitizerTechnology.GetBeamSetFactory();
    beamSet = beamSetFactory.CreateBeamSetConventional("Conventional");
}
```

#### 3.3 Bind Connector with BeamSet

Create a connector with its index and connect to a BeamSet. We use P1/R1 because it can pulse and receive signal.

- P1       index: 0
- P2       index: 1
- P3       index: 2
- P4       index: 3
- P1/R1 index: 4
- P2/R2 index: 5
- P3/R3 index: 6
- P4/R4 index: 7

```c#
# DeviceModel.cs
public void BindConnector()
{
    // Create a connetor at P1/R1 with index 4
    IConnector connector = digitizerTechnology.GetConnectorCollection().GetConnector(4);
    ultrasoundConfiguration.GetFiringBeamSetCollection().Add(beamSet, connector);
}
```

#### 3.4 Initiate Acquisition

Use `acquisition = IAcquisition.CreateEx(device)` to initiate Acquisition.

```c#
# DeviceModel.cs
public void InitiateAcquisition()
{
    if (device == null)
    { return; }
    acquisition = IAcquisition.CreateEx(device);
}
```

#### 3.5 Collect Cycle Data

According to the `Instrumentation.chm` documentation:

>  The proposed application comprises
>
> - a main configuration thread, which configure the devices (See AcquisitionProcess Snippet, main.cpp)
>
> - a data consumer thread which get the data from the buffer. (See AcquisitionProcess Snippet, DataProcess.h)
>
>   (the buffer has a 5 seconds duration and must be read to avoid overflow)

Once acquisition is started, we need to read data from buffer continuously to avoid overflow. This topic will be covered in future lesson when we plot Ascan in real time. For now, we just start acquisition, collect a few cycles of data and stop acquisition.

```c#
# DeviceModel.cs
public ICycleData CollectCycleData()
{
    if (acquisition == null)
    	{ return null; }
    var result = acquisition.WaitForDataEx();
    if (result.status == IAcquisition.WaitForDataResultEx.Status.DataAvailable)
    	{ return result.cycleData; }
    return null;
}
```

#### 3.6 Collect Ascan Data

`GetData()` returns the pointer to  the start of Ascan Data memory. 

> Get pointer to the beginning of the a-scan The A-Scan data goes from the GetData return pointer to GetData + GetSampleQuantity

`IAscan.GetSampleQuantity()` returns the length of Ascan Data.

Use `Marshal.ReadInt32(IntPtr, int ofs)` to loop read each Ascan data point value.

```c#
# DeviceModel.cs
public int[] CollectAscanData()
{
    if (acquisition == null)
    { return null; }
    var result = acquisition.WaitForDataEx();
    if (result.status == IAcquisition.WaitForDataResultEx.Status.DataAvailable)
    {
        var cycleData = result.cycleData;
        var ascan = cycleData.GetAscanCollection().GetAscan(0);
        int[] ascanData = new int[ascan.GetSampleQuantity()];
        for (int i=0; i<ascan.GetSampleQuantity(); i++)
        { ascanData[i] = (int)Marshal.ReadInt32(ascan.GetData(), i * 4); }
        return ascanData;
    }
    return null;
}
```

#### 3.7 Add buttons and text boxes in GUI

Add the following buttons and text box in `ShellView.xaml`.

```xaml
# ShellView.xaml
<!-- Row 2: Create BeamSet -->
<Button x:Name="CreateBeamSet" Content="Create BeamSet"
        Grid.Column="1" Grid.Row="2" Margin="5"/>
<StackPanel Orientation="Horizontal" Grid.Column="2" Grid.Row="2" Margin="5">
    <Label Content="BeamSet Name"/>
    <TextBox x:Name="BeamSetName" MinWidth="50"/>
</StackPanel>

<!-- Row 3: Bind Connector -->
<Button x:Name="BindConnector" Content="Bind Connector"
        Grid.Column="1" Grid.Row="3" Margin="5"/>

<!-- Row 4: Acquisition -->
<Button x:Name="InitAcquisition" Content="InitAcquisition"
        Grid.Column="1" Grid.Row="4" Margin="5"/>
<StackPanel Orientation="Horizontal" Grid.Column="2" Grid.Row="4" Margin="5">
    <Button x:Name="CollectCycleData" Content="Collect CycleData" Margin="5,0"/>
    <Button x:Name="CollectAscanData" Content="Collect Ascan Data" Margin="5,0"/>
</StackPanel>
```

Add a text block for Logs.

```xaml
<!-- Last Row: Logging -->
<Border BorderThickness="1" BorderBrush="Black"
        Grid.Column="1" Grid.ColumnSpan="6" Grid.Row="6" Margin="10">
    <ScrollViewer>
        <TextBlock x:Name="Logging" TextWrapping="Wrap"/>
    </ScrollViewer>
</Border>
```

#### 3.8 Add code logic

Add code logic for buttons and text box in `ShellViewModel.cs`.

```c#
public void CreateBeamSet()
{
    deviceModel.CreatBeamSet();
    BeamSetName = deviceModel.beamSet.GetName();
}

public void BindConnector()
{ deviceModel.BindConnector(); }

public void InitAcquisition()
{ deviceModel.InitiateAcquisition(); }
```

Collect 10 cycles of data, print their cycle ids and stop acquisition.

```c#
public void CollectCycleData()
{
    int number = 10;
    deviceModel.acquisition.ApplyConfiguration();
    deviceModel.acquisition.Start();

    for (int i=0; i<number; i++)
    {
        var cycleData = deviceModel.CollectCycleData();
        Logging += "Cycle ID: " + cycleData.GetCycleId().ToString() + Environment.NewLine;
    }
    deviceModel.acquisition.Stop();
}
```

Collect 1 cycle of Ascan data, print out Ascan data values and stop acquisition.

```c#
public void CollectAscanData()
{
    deviceModel.acquisition.ApplyConfiguration();
    deviceModel.acquisition.Start();

    int[] ascanData = deviceModel.CollectAscanData();
    for (int i=0; i<ascanData.GetLength(0); i++)
    { Logging += ascanData[i].ToString() + ","; }
    Logging += Environment.NewLine;
    
    deviceModel.acquisition.Stop();
}
```

Run project and click buttons in sequence to collect Ascan Data.

#### 3.9 Source Code

Run `git checkout 3_Get_Ascan_Data` to get source code for this section.
