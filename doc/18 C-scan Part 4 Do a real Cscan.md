## 18 C-scan Part4 Do a real Cscan

#### 18.1 Probe

We are using probe 5L64-NW1 to test. 

Probe details can be found in [https://www.olympus-ims.com/en/probes/pa/near-wall/](https://www.olympus-ims.com/en/probes/pa/near-wall/).

```c#
// ShellViewModel.cs

probe = new ProbeModel
{
    TotalElements = 64, 
    UsedElementsPerBeam = 4,
    Frequency = 5,
    Pitch = 1,
};
```

#### 18.2 Corrosion Test Piece

This test piece is made of Plexiglass and is engraved to different depth from behind, so that C-scan image will present different colors.

#### 18.3 Beam Index

Only one A-Scan can be plotted at one time, now the first beam is plotted `PlotAscan(rawData[0]);`. But the first beam is at the edge of a probe, and we are more concerned about A-scan from center beams, so we can add one more text box on the GUI to choose which beam is plotted.

```xaml
# ShellView.xaml            

<Label Content="Beam Index"/>
<TextBox x:Name="BeamIndex" MinWidth="50"/>
```

```c#
// ShellViewModel.cs

private int _beamIndex = 0;

public int BeamIndex
{
    get { return _beamIndex; }
    set
    {
        _beamIndex = value;
        NotifyOfPropertyChange(() => BeamIndex);
    }
}

public void Plotting()
{
    ...
    // Plot Ascan, replace 0 with BeamIndex
    // PlotAscan(rawData[0]); // plot the first Beam
    PlotAscan(rawData[BeamIndex]); // plot selected Beam
}
```

#### 18.4 Run Program

When the program is running, we have to adjust the gate settings according to the signal. In this example, we set Gate `Start`to 100, Gate `Length` to 770 and Gate `Threshold `to 30.

#### 18.5 Source Code

Run `git checkout 18_Cscan_Part4_Do_a_real_Cscan` to get source code for this section.

