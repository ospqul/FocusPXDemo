---
typora-root-url: resources
---

# OpenView SDK Tutorial (C#)

OpenView SDK is designed to help create custom applications for non-destructive inspections of industrial and commercial materials.

This tutorial will help you build a Focus PX application based on OpenView SDK from scratch.

The source codes are available in Github: https://github.com/ospqul/FocusPXDemo/tree/master

Video courses are also available [here](https://github.com/ospqul/FocusPXDemo/tree/master/Training%20Videos).

You could clone a local repo: `git clone https://github.com/ospqul/FocusPXDemo.git`

You could also check out the source code branched out for each lesson.

```bash
$ git branch                                                                               
  1_Set_Environment
  2_Add_NET_lib_and_Connect_Device
  3_Get_Ascan_Data
  4_Plot_Ascan
  5_plotting_Ascan_in_real_time
  6_Change_Ascan_Settings
  7_Connect_Phased_Array_Probe
  8_Plot_Bscan
  
$ git checkout 1_Set_Environment
Switched to branch '1_Set_Environment'
```



## 0 Prerequisites

#### 0.1 Install OpenView SDK

Download Link: [OpenView SDK Download](https://www.olympus-ims.com/en/service-and-support/downloads/#!dlOpen=%2Fen%2Fdownloads%2Fdetail%2F%3F0[downloads][id]%3D276828566)

Follow the instruction to install OpenView SDK.

Useful Openview SDK documents are available under `C:\OlympusNDT\OpenView SDK\1.0\Doc\` folder.

#### 0.2 Install Visual Studio 2017 Community edition

Follow the instruction to install Visual Studio 2017 Community edition:

https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2017



## 1 Setup Environment

#### 1.1 Create a new WPF project

**Windows Presentation Foundation (WPF)** is a powerful tool to build a GUI application.

Open Visual Studio -> New Project -> Visual c# -> Windows Desktop -> WPF App (.NET Framework) -> Set Solution Name to "FPXDemo" -> OK

#### 1.2 MVVM Pattern

The WPF is built to take full advantage of the **Model-View-ViewModel (MVVM)** pattern. The main goal of MVVM pattern is to provide a clear separation between domain logic and presentation layer, so that you can write **maintainable**, **testable**, and **extensible** code. 

- **Model** − It simply holds the data and has nothing to do with any of the business logic.

- **ViewModel** − It acts as the link/connection between the Model and View and makes stuff look pretty.

- **View** − It simply holds the formatted data and essentially delegates everything to the Model.

  

**Caliburn.Micro** package is a small framework that supports MVVM pattern and enables you to build solution quickly. So let's install Caliburn.Micro in our new project.

Right click project -> Manage NuGet Packages -> Browse -> Search "Caliburn" -> Select "Caliburn.Micro" -> Install latest version

#### 1.3 Setup MVVM

Remove `MainWindow.xaml` from project, and remove `StartupUri="MainWindow.xaml"` from `App.xaml` file, because we will start this application from our own view.

Add a new class file `Bootstrapper.cs` under project. This code tells WPF to start from `ShellViewModel`, and we will create a `ShellView` window file and `ShellViewModel` class file later.

```c#
# Bootstrapper.cs    
using Caliburn.Micro;
using FPXDemo.ViewModels;
using System.Windows;

namespace FPXDemo
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
```

Modify and add Bootstrapper to `App.xaml`.

```xaml
<Application x:Class="FPXDemo.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:FPXDemo">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary>
                    <local:Bootstrapper x:Key="Bootstrapper"/>
                </ResourceDictionary>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Add 3 new folders under project: `Views`, `ViewModels`, and `Models`.

Add a new window file `ShellView.xaml` under `Views` folder.

Add a new class file `ShellViewModel.cs` under `ViewModels` folder.

Setup is completed by now, and you should be able to rebuild and start this project.

Project folder structure will be similar to this.

```mathematica
FPXDemo
   |--> Models
   |--> ViewModels
   |         |--> ShellViewModel.cs
   |--> Views
   |      |--> ShellView.xaml
   |--> App.config
   |--> App.xaml
   |--> Bootstrapper.cs
```

#### 1.4 Source Code

Run `git checkout 1_Set_Environment` to get source code for this section.



## 2 Add_NET_lib_and_Connect_Device

#### 2.1 Add OpenView .Net Library to project

Go to OpenView SDK installation folder and copy `OlympusNDT.Instrumentation.NET.dll` file into project folder. 

If you install OpenView SDK by default settings, the location would be `C:\OlympusNDT\OpenView SDK\1.0\Bin\.NET\OlympusNDT.Instrumentation.NET.dll`.

In Visual Studio, right click project and select `Add` -> `Reference` -> `Browse` the copied file `OlympusNDT.Instrumentation.NET.dll` -> `OK`.

Now you can see `OlympusNDT.Instrumentation.NET` under project `References`.

An important thing is to set solution platform to x64 in Visual Studio Configuration Manager, because this .NET library only supports 64-bit applications.

#### 2.2 Connect Device

Add a new class file `DeviceModel.cs` under `Models` folder. In `DeviceModel` constructor, we try to discover FocusPX device and get device handler `IDevice device`.

If there is no device found, then it will prompt a message box.

```c#
# DeviceModel.cs
using System.Windows;
using OlympusNDT.Instrumentation.NET;

namespace FPXDemo.Models
{
    public class DeviceModel
    {
        public IDevice device { get; set; }

        public DeviceModel()
        {
            Utilities.ResolveDependenciesPath();
            int timeout = 5000;
            IDeviceDiscovery deviceDiscovery = IDeviceDiscovery.Create("192.168.0.1");
            DiscoverResult discoverResult = deviceDiscovery.DiscoverFor(timeout);
            device = discoverResult.device;
            if (discoverResult.status != DiscoverResult.Status.DeviceFound)
            {
                MessageBox.Show("Device is not Found!");
                return;
            }
        }
    }
}
```

Next, add a button `Connect Device` on the UI in file `ShellView.xaml`, and set its property `x:Name="ConnectDevice"`. In this way, we bind this button to the `ConnectDevice()` function in `ShellViewModel` class.

```xaml
<Window x:Class="FPXDemo.Views.ShellView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:FPXDemo.Views" WindowStartupLocation="CenterScreen"
        mc:Ignorable="d"
        Title="FocusPX Demo" Height="450" Width="800">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="20"/>
            <ColumnDefinition Width="auto"/>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="20"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="20"/>
            <RowDefinition Height="auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="20"/>
        </Grid.RowDefinitions>
        
        <!-- Row 1: Connect Device -->
        <Button x:Name="ConnectDevice" Content="Connect Device" 
                Grid.Column="1" Grid.Row="1" />  
    </Grid>
</Window>
```

Finally, implement code logic in `ShellViewModel.cs`. 

`ShellViewModel` class is derived from `Screen` class of Caliburn.Micro.

Add a function `public void ConnectDevice()` in `ShellViewModel` class, so that when the button `Connect Device` is clicked, function `ConnectDevice()` is executed. `ConnectDevice()` initializes an instance of `DeviceModel`.

```c#
# ShellViewModel.cs
using Caliburn.Micro;
using FPXDemo.Models;

namespace FPXDemo.ViewModels
{
    class ShellViewModel : Screen
    {
        public DeviceModel deviceModel { get; set; }

        public void ConnectDevice()
        {
            deviceModel = new DeviceModel();
        }
    }
}
```

Run the project, you will get a window with a button. If you click the button and don't get a message box, then you have connected Focus PX successfully.

#### 2.3 Read from Device

After device is connected successfully, we can read some simple info from device, and display them in UI.

First, add two textBoxes in UI, one displays the serial number of this device; the other one displays its ip address.

```xaml
...
<!-- Row 1: Connect Device -->
<Button x:Name="ConnectDevice" Content="Connect Device" 
	Grid.Column="1" Grid.Row="1" />

<StackPanel Orientation="Horizontal" Grid.Column="2" Grid.Row="1">
	<Label Content="Serial Number" />
	<TextBox x:Name="SerialNumber" MinWidth="50"/>
	<Label Content="IP Address"/>
	<TextBox x:Name="IPAddress" MinWidth="50" />
</StackPanel>
...
```

Second, declare `SerialNumber` and `IPAddress` in `ShellViewModel` class. When the value of `SerialNumber` is changed, `NotifyOfPropertyChange(() => SerialNumber)` will notify UI to update its value.

```c#
# ShellViewModel.cs
private string _serialNumber;
public string SerialNumber
{
    get { return _serialNumber; }
    set
    {
        _serialNumber = value;
        NotifyOfPropertyChange(() => SerialNumber);
    }
}

private string _ipAddress;
public string IPAddress
{
    get { return _ipAddress; }
    set
    {
        _ipAddress = value;
        NotifyOfPropertyChange(() => IPAddress);
    }
}
```

Get info from device and set values. 

```c#
# ShellViewModel.cs
public void ConnectDevice()
        {
            deviceModel = new DeviceModel();
            SerialNumber = deviceModel.device.GetInfo().GetSerialNumber();
            IPAddress = deviceModel.device.GetInfo().GetAddressIPv4();
        }
```

Run this project, and click `Connect Device` button, if a device is found, its serial number and ip address will be displayed in each text boxes.

#### 2.4 Source Code

Run `git checkout 2_Add_NET_lib_and_Connect_Device` to get source code for this section.



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



## 4 Plot Ascan

There are a lot C# plotting libraries available in the market, for example, [Oxyplot](https://github.com/oxyplot/oxyplot), [Live Charts](https://lvcharts.net/), and [InteractiveDataDisplay](https://github.com/microsoft/InteractiveDataDisplay.WPF) from Microsoft. You could pick whichever plotting library you are familiar with.

This course takes Oxyplot as an example to show you how to plot Ascan, and future courses will show how to plot other scan views with Oxyplot.

#### 4.1 Install Oxyplot package

Go to `Manage NuGet Packages`, search "Oxyplot", and install latest version of  `Oxyplot.Wpf`.

#### 4.2 Add oxyplot control in GUI

Add `xmlns:oxy="http://oxyplot.org/wpf"` to `Window` 's property in `ShellView.xaml`.

Add oxyplot view and bind PlotView Model to `plotModel`.

```xaml
# ShellView.xaml
<Window x:Class="FPXDemo.Views.ShellView"
        ...
        xmlns:oxy="http://oxyplot.org/wpf">
...
<!-- Plotting Area -->
<oxy:PlotView Grid.Column="6" Grid.Row="1" Grid.RowSpan="6" Model="{Binding plotModel}" Margin="10"/>        
```

Add a `Plot Ascan` button to trigger plotting.

```xaml
<!-- Row 5: Plot Ascan -->
<Button x:Name="PlotAscan" Content="Plot Ascan" Grid.Column="1" Grid.Row="5" Margin="5"/>
```



#### 4.3 Initialize PlotView Model

Initialize Oxyplot PlotView Model in `ShellViewModel` class constructor.

Initiate an instance for Oxyplot PlotModel, its name, **plotModel**, should match `Model="{Binding plotModel}"`.

Use `plotModel.Axes.Add(Axis item)` function to add `xAxis` and `yAxis` to the graph. `xAxis` represents the Ascan Data points' index number, and `yAxis` represents the value of Ascan Data point. In the future, if we know the Ascan's compression factor and sound velocity, we can convert `xAxis` from data point index number to the depth/distance of test piece.

Use `plotModel.Series.Add(Series item)` function to add plotting line to the graph.

```c#
# ShellViewModel.cs
...
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

class ShellViewModel : Screen
{
    public PlotModel plotModel { get; set; }
    public LineSeries lineSeries { get; set; }

    public ShellViewModel()
    {
        plotModel = new PlotModel
        {
            Title = "Ascan Plotting",
        };

        LinearAxis xAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            MajorGridlineStyle = LineStyle.Solid,
        };
        plotModel.Axes.Add(xAxis);

        LinearAxis yAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Solid,
        };
        plotModel.Axes.Add(yAxis);

        lineSeries = new LineSeries
        {
            Title = "Ascan Data",
            Color = OxyColors.Blue,
            StrokeThickness = 1.5,
        };
        plotModel.Series.Add(lineSeries);
    }
}
```

#### 4.4 Draw on graph

Use `lineSeries.Points.Add(DataPoint item)` to add points to the plotting line.

Use `plotModel.InvalidatePlot(true)` to display the plotting line.

```c#
# ShellViewModel.cs
public void PlotAscan()
{
    deviceModel.acquisition.ApplyConfiguration();
    deviceModel.acquisition.Start();

    int[] ascanData = deviceModel.CollectAscanData();
    for (int i = 0; i < ascanData.GetLength(0); i++)
    {
        lineSeries.Points.Add(new DataPoint(i, ascanData[i]));
    }
    plotModel.InvalidatePlot(true);
    deviceModel.acquisition.Stop();
}
```

Run the project and click buttons in sequence to plot a single Ascan.

#### 4.5 Source Code

Run `git checkout 4_Plot_Ascan` to get source code for this section.



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

## 7 Connect Phased Array Probe

In this lesson, we are going to connect with a phased array probe and collect data from each beam.

#### 7.1 Create Phased Array BeamSet

We can create a new a new method `CreatePABeamSet` in `DeviceModel.cs` file. 

When you call `CreateBeamSetPhasedArray()`  to create phased array beamset, an extra argument is required, `beamFormations`. `beamFormations` is where you define how many beams and what are the element delays in a beam. Section 7.2 will show a basic example to generation `beamFormations`.

```c#
# DeviceModel.cs
    
public void CreatPABeamSet()
{
    IDeviceConfiguration deviceConfiguration = device.GetConfiguration();
    ultrasoundConfiguration = deviceConfiguration.GetUltrasoundConfiguration();
    digitizerTechnology = ultrasoundConfiguration.GetDigitizerTechnology(UltrasoundTechnology.PhasedArray);
    IBeamSetFactory beamSetFactory = digitizerTechnology.GetBeamSetFactory();
    var beamFormations = GetBeamFormationCollection(beamSetFactory);
    beamSet = beamSetFactory.CreateBeamSetPhasedArray("Phased Array", beamFormations);
}
```

#### 7.2 Generate beamFormations

For example, we have a phased array probe, which has 32 elements.  8 elements are used to form a beam, so we get total 25 beams:

- Beam 1: element 1 ~ 8
- Beam 2: element 2 ~ 9
- Beam 3: element 3 ~ 10
- ...
- Beam 25: element 25 ~ 32

```c#
# DeviceModel.cs

public IBeamFormationCollection GetBeamFormationCollection(IBeamSetFactory beamSetFactory)
{
    var beamFormations = beamSetFactory.CreateBeamFormationCollection();
    uint usedElementPerBeam = 8;
    uint totalElements = 32;

    for (uint beamIndex=0; beamIndex<totalElements-usedElementPerBeam+1; beamIndex++)
    {
        var beamFormation = beamSetFactory.CreateBeamFormation(
            usedElementPerBeam,
            usedElementPerBeam,
            beamIndex + 1,
            beamIndex + 1);
        beamFormations.Add(beamFormation);
    }

    return beamFormations;
}
```

#### 7.3 Bind Phased Array BeamSet to Connector

Create a new method `BindPAConnector()` in DeviceModel.cs file.

We have one physical PA connector with index 0, and bind the beamset generated in 7.1 to connector 0.

```c#
# DeviceModel.cs

public void BindPAConnector()
{
    // Create a PA connetor, index is 0
    IConnector connector = digitizerTechnology.GetConnectorCollection().GetConnector(0);
    ultrasoundConfiguration.GetFiringBeamSetCollection().Add(beamSet, connector);
}
```

#### 7.4 Collect Data

In section 7.2, we have generated 25 beams, then we should get 25 ascan data. 

Create a 2-d array bscanData, and loop copy each ascan data into bscanData.

`int[][] bscanData = new int[ascans.GetCount()][];`

We can use [Marshal.Copy](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.copy?view=netframework-4.8) method to copy data from a managed array to an unmanaged memory pointer, or from an unmanaged memory pointer to a managed array.

Source: `ascan.GetData()`

Destination: `ascanData`

StartIndex: `0`

Length: `ascan.GetSampleQuantity()`

```c#
public int[][] CollectBscanData()
{
	if (acquisition == null)
    {
    	return null;
    }

    var result = acquisition.WaitForDataEx();
    if (result.status == IAcquisition.WaitForDataResultEx.Status.DataAvailable)
    {
        var cycleData = result.cycleData;
        var ascans = cycleData.GetAscanCollection();
        int[][] bscanData = new int[ascans.GetCount()][];

        for (uint index=0; index<ascans.GetCount(); index++)
        {
            var ascan = ascans.GetAscan(index);
            int[] ascanData = new int[ascan.GetSampleQuantity()];
            Marshal.Copy(ascan.GetData(), ascanData, 0, (int)ascan.GetSampleQuantity());
            bscanData[index] = ascanData;
        }
        return bscanData;
    }

    return null;
}
```

#### 7.5 Source Code

Run `git checkout 7_Connect_Phased_Array_Probe` to get source code for this section.

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

![Unfocused Bscan](/../../../../work/FocusPXDemo/resources/Unfocused Bscan.PNG)

#### 8.6 Source Code

Run `git checkout 8_Plot_Bscan` to get source code for this section.