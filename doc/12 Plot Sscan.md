

## 12 Plot Sscan

#### 12.1 Create Beam Formations

Create a new method `GetSscanBeamFormationCollection()` in `DeviceModel.cs`. Every beam starts from element 1.

```c#
# DeviceModel.cs

public IBeamFormationCollection GetSscanBeamFormationCollection(
    IBeamSetFactory beamSetFactory,
    ProbeModel probe,
    double[][] elementDelays) // sscan delay 2-d array
{
    var beamFormations = beamSetFactory.CreateBeamFormationCollection();
    uint usedElementPerBeam = probe.UsedElementsPerBeam;
    uint totalElements = probe.TotalElements;

    // Add Focused beam formations
    for (uint beamIndex = 0; beamIndex < elementDelays.GetLength(0); beamIndex++)
    {
        var beamFormation = beamSetFactory.CreateBeamFormation(
            usedElementPerBeam,
            usedElementPerBeam,
            1,  // pulser element starts from 1
            1); // receiver element starts from 1

        // Add element delays
        var pulserDelays = beamFormation.GetPulserDelayCollection();
        var receiverDelays = beamFormation.GetReceiverDelayCollection();

        for (uint elemIndex = 0; elemIndex < usedElementPerBeam; elemIndex++)
        {
            // Add pulser delay
            pulserDelays.GetElementDelay(elemIndex).SetElementId(elemIndex + 1);
            pulserDelays.GetElementDelay(elemIndex).SetDelay(elementDelays[beamIndex][elemIndex]);

            //Add receiver delay
            receiverDelays.GetElementDelay(elemIndex).SetElementId(elemIndex + 1);
            receiverDelays.GetElementDelay(elemIndex).SetDelay(elementDelays[beamIndex][elemIndex]);
        }

        beamFormations.Add(beamFormation);
    }
    return beamFormations;
}
```

#### 12.2 Create Focused Beam Set

Create a new method `CreatPASscanBeamSet()` in `DeviceModel.cs`, and call `GetSscanBeamFormationCollection()` to get beam formations.

```c#
# DeviceModel.cs

public void CreatPASscanBeamSet(ProbeModel probe, double[][] elementDelays)
{
    IDeviceConfiguration deviceConfiguration = device.GetConfiguration();
    ultrasoundConfiguration = deviceConfiguration.GetUltrasoundConfiguration();
    digitizerTechnology = ultrasoundConfiguration.GetDigitizerTechnology(UltrasoundTechnology.PhasedArray);
    IBeamSetFactory beamSetFactory = digitizerTechnology.GetBeamSetFactory();
    var beamFormations = GetSscanBeamFormationCollection(beamSetFactory, probe, elementDelays);
    beamSet = beamSetFactory.CreateBeamSetPhasedArray("Phased Array", beamFormations);
}
```

#### 12.3 Make change in ShellViewModel

1. Make `probe` and `sscanModel` public property in `ShellViewModel` class.

```c#
# ShellViewModel.cs

public ProbeModel probe { get; set; }
public SscanModel sscanModel { get; set; }

public ShellViewModel()
{
    InitBscanPlot();

    // Init a probe
    probe = new ProbeModel
    {
        TotalElements = 32, // total 32 elements
        UsedElementsPerBeam = 32, // use all 32 elements
        Frequency = 5,
        Pitch = 1,
    };

    // Init Sscan Settings
    sscanModel = new SscanModel
    {
        StartAngle = -45, // degree
        EndAngle = 45,
        AngleResolution = 1,
        FocusDepth = 17,  //mm
    };
}
```

2. Create a new method `GetSscanDelay()`

```c#
# ShellViewModel.cs

public double[][] GetSscanDelay()
{
    // Calculate sscan delays
    // Calculate element positions
    var positions = DelayLawModel.GetElementsPosition(probe);

    // Calculate element delays
    double velocity = 5800; // stainless steel block
    var delays = DelayLawModel.GetSscanDelays(positions, velocity, sscanModel);
    return delays;
}
```

3. Comment Focused beam method and create Sscan Beam Set.

```c#
# ShellViewModel.cs

public async void ConnectDevice()
{
    ...
    // Create PA Focused Beam Set
    //var delays = GetDelays();
    //deviceModel.CreatPAFocusedBeamSet(probe, delays);
    //deviceModel.BindPAConnector();

    // Create PA Sscan Beam Set
    var delays = GetSscanDelay();
    deviceModel.CreatPASscanBeamSet(probe, delays);
    deviceModel.BindPAConnector();
    ...
}
```

4. Run the program and it will plot Sscan

![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/Plot%20Sscan%20Raw%20data.PNG)

#### 12.4 Source Code

Run `git checkout 12_Plot_Sscan` to get source code for this section.