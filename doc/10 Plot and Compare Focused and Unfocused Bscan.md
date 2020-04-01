

## 10 Plot and Compare Focused and Unfocused Bscan

#### 10.1 Create Focused Beam Formations

Create a new method `GetFocusedBeamFormationCollection()` in `DeviceModel.cs`. Focused Beam formations method has one more step compared with Unfocused Beam: specify element delays.

In each beam formation, we assign delays to all pulser and receiver elements according to delay laws calculated. Please note that `GetElementDelay()` takes element index in this beam(range from 0 to `usedElementPerBeam` -1), and `SetElementId()` takes element index in this probe(range from 1 to `TotalElements`).

For example, we have a phased array probe with 32 elements and use 8 elements as a group to fire a beam. Then, the second beam includes 8 elements, which are element 2 - 9. 

For the first pulser element delay, we can set:

```c#
pulserDelays.GetElementDelay(0).SetElementId(2);// First delay is element 2
pulserDelays.GetElementDelay(0).SetDelay(10);   // First delay set to 10ns
```

Here is full code of `GetFocusedBeamFormationCollection()`.

```c#
# DeviceModel.cs

public IBeamFormationCollection GetFocusedBeamFormationCollection(
    IBeamSetFactory beamSetFactory,
    ProbeModel probe,
    double[] elementDelays)
{
    var beamFormations = beamSetFactory.CreateBeamFormationCollection();
    uint usedElementPerBeam = probe.UsedElementsPerBeam;
    uint totalElements = probe.TotalElements;

    for (uint beamIndex = 0; beamIndex < totalElements - usedElementPerBeam + 1; beamIndex++)
    {
        var beamFormation = beamSetFactory.CreateBeamFormation(
            usedElementPerBeam,
            usedElementPerBeam,
            beamIndex + 1,
            beamIndex + 1);

        // Add element delays
        var pulserDelays = beamFormation.GetPulserDelayCollection();
        var receiverDelays = beamFormation.GetReceiverDelayCollection();

        for (uint elemIndex=0; elemIndex< usedElementPerBeam; elemIndex++)
        {
            // Add pulser delay
            pulserDelays.GetElementDelay(elemIndex).SetElementId(elemIndex + beamIndex + 1);
            pulserDelays.GetElementDelay(elemIndex).SetDelay(elementDelays[elemIndex]);

            //Add receiver delay
            receiverDelays.GetElementDelay(elemIndex).SetElementId(elemIndex + beamIndex + 1);
            receiverDelays.GetElementDelay(elemIndex).SetDelay(elementDelays[elemIndex]);
        }

        beamFormations.Add(beamFormation);
    }
    
    return beamFormations;
}
```

#### 10.2 Create Focused Beam Set

Create a new method `CreatPAFocusedBeamSet()` in `DeviceModel.cs`, and call `GetFocusedBeamFormationCollection()` to get beam formations.

```c#
# DeviceModel.cs

public void CreatPAFocusedBeamSet(ProbeModel probe, double[] elementDelays)
{
    IDeviceConfiguration deviceConfiguration = device.GetConfiguration();
    ultrasoundConfiguration = deviceConfiguration.GetUltrasoundConfiguration();
    digitizerTechnology = ultrasoundConfiguration.GetDigitizerTechnology(UltrasoundTechnology.PhasedArray);
    IBeamSetFactory beamSetFactory = digitizerTechnology.GetBeamSetFactory();
    var beamFormations = GetFocusedBeamFormationCollection(beamSetFactory, probe, elementDelays);
    beamSet = beamSetFactory.CreateBeamSetPhasedArray("Phased Array", beamFormations);
}
```

#### 10.3 Make change in ShellViewModel

1. Make `probe` a public property in `ShellViewModel` class.

```c#
# ShellViewModel.cs

// Probe
public ProbeModel probe { get; set; }

public ShellViewModel()
{
    InitBscanPlot();

    // Init a probe
    probe = new ProbeModel
    {
        TotalElements = 64,
        UsedElementsPerBeam = 16,
        Frequency = 5,
        Pitch = 1,
    };
}
```

2. Create a new method `GetDelays()`

```c#
# ShellViewModel.cs

public double[] GetDelays()
{
    // Calculate element positions
    var positions = DelayLawModel.GetElementsPosition(probe);

    // Calculate element delays
    double velocity = 5800; // stainless steel block
    double depth = 17; // mm
    var delays = DelayLawModel.GetElementDelays(positions, velocity, depth);

    return delays;
}
```

3. Comment Unfocused beam and call Focused beam method.

```c#
# ShellViewModel.cs

public async void ConnectDevice()
{
    ...
    // Create PA Beam Set
    //deviceModel.CreatPABeamSet();
    //deviceModel.BindPAConnector();

    // Create PA Focused Beam Set
    var delays = GetDelays();
    deviceModel.CreatPAFocusedBeamSet(probe, delays);
    deviceModel.BindPAConnector();    
    ...
}
```

4. Run the program and it will plot Focused Bscan

#### 10.4 Compare Focused Bscan and Unfocused Bscan

In `GetFocusedBeamFormationCollection()` method, we can also add beam formations without assigning element delays, so that the first half of beams are focused beam and seconds half are unfocused beam.

```c#
# DeviceModel.cs

public IBeamFormationCollection GetFocusedBeamFormationCollection(
            IBeamSetFactory beamSetFactory,
            ProbeModel probe,
            double[] elementDelays)
{
    var beamFormations = beamSetFactory.CreateBeamFormationCollection();
    uint usedElementPerBeam = probe.UsedElementsPerBeam;
    uint totalElements = probe.TotalElements;

    // Add Focused beam formations
    for (uint beamIndex = 0; beamIndex < totalElements - usedElementPerBeam + 1; beamIndex++)
    {
        var beamFormation = beamSetFactory.CreateBeamFormation(
            usedElementPerBeam,
            usedElementPerBeam,
            beamIndex + 1,
            beamIndex + 1);

        // Add element delays
        var pulserDelays = beamFormation.GetPulserDelayCollection();
        var receiverDelays = beamFormation.GetReceiverDelayCollection();

        for (uint elemIndex=0; elemIndex< usedElementPerBeam; elemIndex++)
        {
            // Add pulser delay
            pulserDelays.GetElementDelay(elemIndex).SetElementId(elemIndex + beamIndex + 1);
            pulserDelays.GetElementDelay(elemIndex).SetDelay(elementDelays[elemIndex]);

            //Add receiver delay
            receiverDelays.GetElementDelay(elemIndex).SetElementId(elemIndex + beamIndex + 1);
            receiverDelays.GetElementDelay(elemIndex).SetDelay(elementDelays[elemIndex]);
        }

        beamFormations.Add(beamFormation);
    }

    // Add unfocused beam formations
    for (uint beamIndex = 0; beamIndex < totalElements - usedElementPerBeam + 1; beamIndex++)
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

Here is a screenshot:
![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/Focused%20and%20Unfocused%20Bscan.PNG)
#### 10.5 Source Code

Run `git checkout 10_Plot_and_Compoare_Focused_and_Unfocused_Bscan` to get source code for this section.
