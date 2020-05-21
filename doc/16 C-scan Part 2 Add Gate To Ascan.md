## 16 C-scan Part2 Add Gate To Ascan   

#### 16.1 Gate

There are different kinds of gate, and different gates are suitable for different inspection cases.

This section will implement a gate to measure TOF(time of flight) for a corrosion inspection project.

![Ascan Gate](https://github.com/ospqul/FocusPXDemo/blob/master/resources/CscanGate.PNG)

#### 16.2 Gate Class

We need 3 properties to describe a gate:

1. Starting point
2. Gate length
3. Gate Threshold

```c#
# GateModel.cs

public class GateModel
{
	public double Start { get; set; }
	public double Length { get; set; }
	public double Threshold { get; set; }
}
```

#### 16.3 Initialize a gate

Initialize a gate in `ShellViewModel` constructor.

```c#
// ShellViewModel.cs

public GateModel gate { get; set; }

gate = new GateModel
{
    Start = 10,
    Length = 50,
    Threshold = 20,
};
```

#### 16.4 Add Gate As Annotation

```c#
// ShellViewModel.cs

public ArrowAnnotation annotation { get; set; }

// InitAscan()
annotation = new ArrowAnnotation
{
    HeadLength = 0,
    HeadWidth = 0,
    Text = "Gate",
    TextColor = OxyColors.Red,
    StrokeThickness = 5,
    Color = OxyColors.Red,
};
plotModel.Annotations.Add(annotation);
}
```

#### 16.5 Plot Gate in Ascan Plotting

```c#
// ShellViewModel.cs        
        
public void PlotGate()
{
    if (annotation != null)
    {
        annotation.StartPoint = new DataPoint(gate.Start, gate.Threshold);
        annotation.EndPoint = new DataPoint(gate.Start + gate.Length, gate.Threshold);
        
        // update plotting
        plotModel.InvalidatePlot(true);
    }
}
```

#### 16.6 Add Gate Settings in GUI

Add Gate Settings in GUI, and bind their values. Call `PlotGate()` to update gate in A-scan plotting when settings are changed.

```xaml
# ShellView.xaml      

<!-- Row 3: Gate Settings -->
<StackPanel Orientation="Horizontal" Grid.Column="1" Grid.ColumnSpan="2"
            Grid.Row="3" Margin="5">
    <Label Content="Gate"/>
    <Label Content="Start"/>
    <TextBox x:Name="GateStart" MinWidth="50"/>
    <Label Content="Length"/>
    <TextBox x:Name="GateLength" MinWidth="50"/>
    <Label Content="Threshold"/>
    <TextBox x:Name="GateThreshold" MinWidth="50"/>
</StackPanel>
```

```c#
// ShellViewModel.cs
    
private string _gateStart;

public string GateStart
{
    get { return _gateStart; }
    set
    {
        _gateStart = value;
        NotifyOfPropertyChange(() => GateStart);
        if (double.TryParse(_gateStart, out double result))
        {
            gate.Start = result;
            // replot Gate when value is changed
            PlotGate();
        }
    }
}

private string _gateLength;

public string GateLength
{
    get { return _gateLength; }
    set
    {
        _gateLength = value;
        NotifyOfPropertyChange(() => GateLength);
        if (double.TryParse(_gateLength, out double result))
        {
            gate.Length = result;
            PlotGate();
        }
    }
}

private string _gateThreshold;

public string GateThreshold
{
    get { return _gateThreshold; }
    set
    {
        _gateThreshold = value;
        NotifyOfPropertyChange(() => GateThreshold);
        if (double.TryParse(_gateThreshold, out double result))
        {
            gate.Threshold = result;
            PlotGate();
        }
    }
}
```

#### 16.7 Source Code

Run `git checkout 16_Cscan_Part2_Add_Gate_To_Ascan` to get source code for this section.

