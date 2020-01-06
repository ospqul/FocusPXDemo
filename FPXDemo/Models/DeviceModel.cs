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

        public void BindConnector()
        {
            // Create a connetor at P1/R1 with index 4
            IConnector connector = digitizerTechnology.GetConnectorCollection().GetConnector(4);
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

        public void ConsumeData()
        {
            try
            {
                var dataResult = acquisition.WaitForDataEx();
                while (dataResult.status == IAcquisition.WaitForDataResultEx.Status.DataAvailable)
                {
                    using (var cycleData = dataResult.cycleData)
                        dataResult = acquisition.WaitForDataEx();
                }
                dataResult.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
