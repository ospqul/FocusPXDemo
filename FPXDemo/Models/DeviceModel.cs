using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OlympusNDT.Instrumentation.NET;

namespace FPXDemo.Models
{
    public class DeviceModel
    {
        public IDevice device { get; set; }

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
        }
    }
}
