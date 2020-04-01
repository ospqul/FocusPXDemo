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
