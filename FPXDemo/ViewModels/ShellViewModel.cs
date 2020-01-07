using Caliburn.Micro;
using FPXDemo.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
        public PlotModel plotModel { get; set; }
        public LineSeries lineSeries { get; set; }

        private string _serialNumber;
        private string _ipAddress;
        private string _beamSetName;

        private string _ascanGain;
        private string _ascanLength;

        private string _logging;


        public ShellViewModel()
        {
            plotModel = new PlotModel
            {
                Title = "Ascan Plotting",
            };

            LinearAxis xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
            };
            plotModel.Axes.Add(xAxis);

            LinearAxis yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
            };
            plotModel.Axes.Add(yAxis);

            lineSeries = new LineSeries
            {
                Title = "Ascan Data",
                Color = OxyColors.Blue,
                StrokeThickness = 1.5,
            };
            plotModel.Series.Add(lineSeries);
        }

        public async void ConnectDevice()
        {
            //deviceModel = new DeviceModel();
            deviceModel = await Task.Run(() => new DeviceModel());
            SerialNumber = deviceModel.device.GetInfo().GetSerialNumber();
            IPAddress = deviceModel.device.GetInfo().GetAddressIPv4();
            CreateBeamSet();
            BindConnector();
            InitAcquisition();

            // Init Ascan Settings
            AscanGain = deviceModel.beamSet.GetBeam(0).GetGain().ToString();
            AscanLength = deviceModel.beamSet.GetBeam(0).GetAscanLength().ToString();

            StartAscan();
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

        public void PlotAscan()
        {
            deviceModel.acquisition.ApplyConfiguration();
            deviceModel.acquisition.Start();

            int[] ascanData = deviceModel.CollectAscanData();

            for (int i = 0; i < ascanData.GetLength(0); i++)
            {
                lineSeries.Points.Add(new DataPoint(i, ascanData[i]));
            }
            plotModel.InvalidatePlot(true);
            deviceModel.acquisition.Stop();
        }

        public void PlottingAscan()
        {
            int[] ascanData;
            int plottingIndex = 0;

            while (true)
            {
                try
                {
                    ascanData = deviceModel.CollectAscanData();
                    if (ascanData.GetLength(0) < 1)
                    { break; }

                    lineSeries.Points.Clear();

                    for (int i = 0; i < ascanData.GetLength(0); i++)
                    {
                        lineSeries.Points.Add(new DataPoint(i, ascanData[i]));
                    }
                    plotModel.InvalidatePlot(true);
                    plottingIndex += 1;
                    Logging = plottingIndex.ToString();
                }
                catch (Exception)
                {

                    //throw;
                }
            }
        }

        public void StartAscan()
        {
            deviceModel.acquisition.ApplyConfiguration();
            deviceModel.acquisition.Start();

            var taskConsumeData = new Task(() => deviceModel.ConsumeData());
            taskConsumeData.Start();

            var taskPlottingAscan = new Task(() => PlottingAscan());
            taskPlottingAscan.Start();
        }

        public void StopAscan()
        {
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
                     
        public string AscanGain
        {
            get { return _ascanGain; }
            set
            {
                _ascanGain = value;
                NotifyOfPropertyChange(() => AscanGain);
                if (double.TryParse(_ascanGain, out double result))
                {
                    deviceModel.beamSet.GetBeam(0).SetGainEx(result);
                    deviceModel.acquisition.ApplyConfiguration();
                }
            }
        }

        public string AscanLength
        {
            get { return _ascanLength; }
            set
            {
                _ascanLength = value;
                NotifyOfPropertyChange(() => AscanLength);
                if (double.TryParse(_ascanLength, out double result))
                {
                    deviceModel.beamSet.GetBeam(0).SetAscanLength(result);
                    deviceModel.acquisition.ApplyConfiguration();
                }
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
