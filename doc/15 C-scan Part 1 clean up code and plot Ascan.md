## 15 C-scan Part 1 clean up code and plot Ascan    

#### 15.1 C-scan

> **C-Scan** refers to the image produced when the data collected from an ultrasonic inspection is plotted on a plan view of the component.

See [https://www.bindt.org/What-is-NDT/Index-of-acronyms/C/C-Scan/](https://www.bindt.org/What-is-NDT/Index-of-acronyms/C/C-Scan/)

> Typically, a data collection gate is established on the A-scan and the  amplitude or the time-of-flight of the signal is recorded at regular  intervals as the transducer is scanned over the test piece. The relative signal amplitude or the time-of-flight is displayed as a shade of gray or a color for each of the positions where data was recorded. The C-scan presentation provides an image of the features that reflect and scatter the sound within and on the surfaces of the test piece.

See [https://www.nde-ed.org/EducationResources/CommunityCollege/Ultrasonics/EquipmentTrans/DataPres.htm](https://www.nde-ed.org/EducationResources/CommunityCollege/Ultrasonics/EquipmentTrans/DataPres.htm)

Basically, in order to plot a C-scan image, we need to acquire data from Foucs PX, and apply a **Gate** on A-scan to get C-scan values.

We will discuss how to implement a customized gate in next section.

#### 15.2 Prepare code for C-scan Plotting

Before we try to plot C-scan, let's 

1. remove unnecessary codes/methods/functions;
2. plot A-scan (refer to [5 Plotting Ascan in real time](https://github.com/ospqul/FocusPXDemo/blob/master/doc/5%20Plotting%20Ascan%20in%20real%20time.md)).

#### 15.3 Source Code

Run `git checkout 15_Cscan_Part1_Clean_up_code_and_Plot_Ascan` to get source code for this section.

