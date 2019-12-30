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

        private string _serialNumber;
        private string _ipAddress;
        private string _beamSetName;

        private string _logging;


        public void ConnectDevice()
        {
            deviceModel = new DeviceModel();
            SerialNumber = deviceModel.device.GetInfo().GetSerialNumber();
            IPAddress = deviceModel.device.GetInfo().GetAddressIPv4();
        }

        public void CreateBeamSet()
        {
            deviceModel.CreatBeamSet();
            BeamSetName = deviceModel.beamSet.GetName();
            Logging += "BeamSet is created!" + Environment.NewLine;
        }

        public void BindConnector()
        {
            deviceModel.BindConnector();
            Logging += "Connector is binded!" + Environment.NewLine;
        }

        public void InitAcquisition()
        {
            deviceModel.InitiateAcquisition();
            Logging += "Init Acquisition!" + Environment.NewLine;
        }

        public void CollectCycleData()
        {
            int number = 10;

            deviceModel.acquisition.ApplyConfiguration();
            deviceModel.acquisition.Start();

            for (int i=0; i<number; i++)
            {
                var cycleData = deviceModel.CollectCycleData();
                Logging += "Cycle ID: " + cycleData.GetCycleId().ToString() + Environment.NewLine;
            }

            deviceModel.acquisition.Stop();
        }

        public void CollectAscanData()
        {
            deviceModel.acquisition.ApplyConfiguration();
            deviceModel.acquisition.Start();

            int[] ascanData = deviceModel.CollectAscanData();

            for (int i=0; i<ascanData.GetLength(0); i++)
            {
                Logging += ascanData[i].ToString() + ",";
            }

            Logging += Environment.NewLine;

            deviceModel.acquisition.Stop();
        }

        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                _serialNumber = value;
                NotifyOfPropertyChange(() => SerialNumber);
            }
        }

        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                NotifyOfPropertyChange(() => IPAddress);
            }
        }

        public string BeamSetName
        {
            get { return _beamSetName; }
            set
            {
                _beamSetName = value;
                NotifyOfPropertyChange(() => BeamSetName);
            }
        }

        public string Logging
        {
            get { return _logging; }
            set
            {
                _logging = value;
                NotifyOfPropertyChange(() => Logging);
            }
        }



    }
}
