using Caliburn.Micro;
using FPXDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPXDemo.ViewModels
{
    class ShellViewModel : Screen
    {
        public DeviceModel deviceModel { get; set; }

        public void ConnectDevice()
        {
            deviceModel = new DeviceModel();
            SerialNumber = deviceModel.device.GetInfo().GetSerialNumber();
            IPAddress = deviceModel.device.GetInfo().GetAddressIPv4();
        }

        private string _serialNumber;

        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                _serialNumber = value;
                NotifyOfPropertyChange(() => SerialNumber);
            }
        }

        private string _ipAddress;

        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                NotifyOfPropertyChange(() => IPAddress);
            }
        }




    }
}
