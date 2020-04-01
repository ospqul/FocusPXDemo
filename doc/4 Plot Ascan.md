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
