## 14 Plot Sscan after correction

#### 14.1 Why backwall is not straight

In [lesson 11](https://github.com/ospqul/FocusPXDemo/blob/master/doc/11%20Calculate%20Sscan%20Element%20delay.md) we calculate delays for each pulser and receiver elements. For instance, in the following beam, element 8 is the furthest element to focal point, so element 8's element delay is 0.

![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/Sscan%20beam%20delay%20diagram1.png)

Therefore, we can consider element 8 is **Time Zero**, and this beam's time of flight is element 8's time of flight. If we take center of the aperture as the Time Zero, then we will plot point a little further than the real focal point.

![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/Sscan%20beam%20delay%20diagram2.png)

#### 14.2 Beam Delay

We want our beam looks like it starts from center of the aperture, so the center of aperture has to be **Time Zero**. But in this case, element 8's delay is a negative value(element 8 needs to pulse before center elements); however, OpenView SDK cannot take a negative number as delay, so we have to introduce **Beam delay** to correct S-scan plotting.

**Beam delay** is the difference between TOF of center point of aperture and TOF of Time zero element.

For example,

```c#
double beamAngle = -45; // degree
double focusDepth = 17; // mm
double velocity = 5800; // m/s
uint elementNumber = 8;
double pitch = 1; // mm
```

then we can calculate element Time Of Flight (double of distance):

```c#
Element 0 TOF: 7485.61839520428 // ns
Element 1 TOF: 7704.79412810494 // ns
Element 2 TOF: 7932.90832436368 // ns
Element 3 TOF: 8169.21223481527 // ns
Element 4 TOF: 8413.01579224541 // ns
Element 5 TOF: 8663.68588027663 // ns
Element 6 TOF: 8920.64366915623 // ns
Element 7 TOF: 9183.3613693695  // ns
```

center point Time Of Flight:

```c#
Center TOF: 8290.217434600902 // ns
```

Beam delay is 9183 - 8290 = 893 nanoseconds. When we are plotting, we should delete the first 893 nanoseconds data.

#### 14.3 Set Beam Delay in SDK

In OpenView SDK, we can set beam delay with `AscanStart` easily.

Create a new method to correct Sscan and take element delays as argument .

1. Loop through each beam.
2. If used elements is an odd number, we double the delay of middle element and set to `AscanStart`.
3. If used element is an even number, we add the delays of two middle elements and set to `AscanStart`. This is a simple way to get beam delay, and we can ignore the minor error of TOF.

```c#
# ShellViewModel.cs

public void CorrectSscan(double[][] delays)
{
    for (uint i=0; i< delays.GetLength(0); i++)
    {
        if (probe.UsedElementsPerBeam % 2 == 1)
        {
            uint mid = probe.UsedElementsPerBeam / 2 + 1;
            // set ascan start to the double of middle element's delay
            deviceModel.beamSet.GetBeam(i).SetAscanStart(delays[i][mid] * 2);
        }
        else
        {
            uint mid = probe.UsedElementsPerBeam / 2;
            // set ascan start to the sum of the middle two elements' delays
            deviceModel.beamSet.GetBeam(i).SetAscanStart(delays[i][mid] + delays[i][mid+1]);
        }
    }
}
```

Then we can add this method after S-scan Beam set creation and before Acquisition starts.

```c#
# ShellViewModel.cs

...
// Create PA Sscan Beam Set
var delays = GetSscanDelay();
deviceModel.CreatPASscanBeamSet(probe, delays);
deviceModel.BindPAConnector();
// correct sscan
CorrectSscan(delays);

InitAcquisition();
...
```

#### 14.3 Plot Sscan

![](https://github.com/ospqul/FocusPXDemo/blob/master/resources/Sscan%20after%20correction.PNG)

#### 14.4 Source Code

Run `git checkout 14_Plot_Sscan_after_correction` to get source code for this section.
