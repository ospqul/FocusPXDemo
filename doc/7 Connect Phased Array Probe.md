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
