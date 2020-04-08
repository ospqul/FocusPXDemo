using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OlympusNDT.Instrumentation.NET;

namespace FPXDemo.Models
{
    public class DeviceModel
    {
        public IDevice device { get; set; }
        public IBeamSet beamSet { get; set; }
        public IUltrasoundConfiguration ultrasoundConfiguration { get; set; }
        public IDigitizerTechnology digitizerTechnology { get; set; }
        public IAcquisition acquisition { get; set; }

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
            //MessageBox.Show("Device is Found!");
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

        public void CreatBeamSet()
        {
            IDeviceConfiguration deviceConfiguration = device.GetConfiguration();
            ultrasoundConfiguration = deviceConfiguration.GetUltrasoundConfiguration();
            digitizerTechnology = ultrasoundConfiguration.GetDigitizerTechnology(UltrasoundTechnology.Conventional);
            IBeamSetFactory beamSetFactory = digitizerTechnology.GetBeamSetFactory();
            beamSet = beamSetFactory.CreateBeamSetConventional("Conventional");
        }

        public void CreatPABeamSet(ProbeModel probe)
        {
            IDeviceConfiguration deviceConfiguration = device.GetConfiguration();
            ultrasoundConfiguration = deviceConfiguration.GetUltrasoundConfiguration();
            digitizerTechnology = ultrasoundConfiguration.GetDigitizerTechnology(UltrasoundTechnology.PhasedArray);
            IBeamSetFactory beamSetFactory = digitizerTechnology.GetBeamSetFactory();
            var beamFormations = GetBeamFormationCollection(beamSetFactory, probe);
            beamSet = beamSetFactory.CreateBeamSetPhasedArray("Phased Array", beamFormations);
        }

        public IBeamFormationCollection GetBeamFormationCollection(IBeamSetFactory beamSetFactory, ProbeModel probe)
        {
            var beamFormations = beamSetFactory.CreateBeamFormationCollection();
            uint usedElementPerBeam = probe.UsedElementsPerBeam;
            uint totalElements = probe.TotalElements;

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

        // Create Focused Beam set
        public void CreatPAFocusedBeamSet(ProbeModel probe, double[] elementDelays)
        {
            IDeviceConfiguration deviceConfiguration = device.GetConfiguration();
            ultrasoundConfiguration = deviceConfiguration.GetUltrasoundConfiguration();
            digitizerTechnology = ultrasoundConfiguration.GetDigitizerTechnology(UltrasoundTechnology.PhasedArray);
            IBeamSetFactory beamSetFactory = digitizerTechnology.GetBeamSetFactory();
            var beamFormations = GetFocusedBeamFormationCollection(beamSetFactory, probe, elementDelays);
            beamSet = beamSetFactory.CreateBeamSetPhasedArray("Phased Array", beamFormations);
        }

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

        // Create Sscan Beam set
        public void CreatPASscanBeamSet(ProbeModel probe, double[][] elementDelays)
        {
            IDeviceConfiguration deviceConfiguration = device.GetConfiguration();
            ultrasoundConfiguration = deviceConfiguration.GetUltrasoundConfiguration();
            digitizerTechnology = ultrasoundConfiguration.GetDigitizerTechnology(UltrasoundTechnology.PhasedArray);
            IBeamSetFactory beamSetFactory = digitizerTechnology.GetBeamSetFactory();
            var beamFormations = GetSscanBeamFormationCollection(beamSetFactory, probe, elementDelays);
            beamSet = beamSetFactory.CreateBeamSetPhasedArray("Phased Array", beamFormations);
        }

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
                    pulserDelays.GetElementDelay(elemIndex).SetElementId(elemIndex + 1); // element starts from 1
                    pulserDelays.GetElementDelay(elemIndex).SetDelay(elementDelays[beamIndex][elemIndex]);

                    //Add receiver delay
                    receiverDelays.GetElementDelay(elemIndex).SetElementId(elemIndex + 1); // element starts from 1
                    receiverDelays.GetElementDelay(elemIndex).SetDelay(elementDelays[beamIndex][elemIndex]);
                }

                beamFormations.Add(beamFormation);
            }
            return beamFormations;
        }


        public void BindConnector()
        {
            // Create a connetor at P1/R1 with index 4
            IConnector connector = digitizerTechnology.GetConnectorCollection().GetConnector(4);
            ultrasoundConfiguration.GetFiringBeamSetCollection().Add(beamSet, connector);
        }

        public void BindPAConnector()
        {
            // Create a PA connetor, index is 0
            IConnector connector = digitizerTechnology.GetConnectorCollection().GetConnector(0);
            ultrasoundConfiguration.GetFiringBeamSetCollection().Add(beamSet, connector);
        }

        public void InitiateAcquisition()
        {
            if (device == null)
            {
                return;
            }
            acquisition = IAcquisition.CreateEx(device);
        }

        public ICycleData CollectCycleData()
        {
            if (acquisition == null)
            {
                return null;
            }

            var result = acquisition.WaitForDataEx();
            if (result.status == IAcquisition.WaitForDataResultEx.Status.DataAvailable)
            {
                return result.cycleData;
            }

            return null;
        }

        public int[] CollectAscanData()
        {
            if (acquisition == null)
            {
                return null;
            }

            var result = acquisition.WaitForDataEx();
            if (result.status == IAcquisition.WaitForDataResultEx.Status.DataAvailable)
            {
                var cycleData = result.cycleData;
                var ascan = cycleData.GetAscanCollection().GetAscan(0);
                int[] ascanData = new int[ascan.GetSampleQuantity()];

                for (int i=0; i<ascan.GetSampleQuantity(); i++)
                {
                    ascanData[i] = (int)Marshal.ReadInt32(ascan.GetData(), i * 4);
                }
                return ascanData;
            }

            return null;
        }

        public int[][] CollectRawData()
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
    }
}
