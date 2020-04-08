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
using System.Windows;

namespace FPXDemo.ViewModels
{
    class ShellViewModel : Screen
    {
        public DeviceModel deviceModel { get; set; }
        public PlotModel plotModel { get; set; }
        public LineSeries lineSeries { get; set; }     

        // Probe
        public ProbeModel probe { get; set; }

        private string _serialNumber;
        private string _ipAddress;
        private string _beamSetName;

        private string _ascanGain;
        private string _ascanLength;

        private string _logging;


        public ShellViewModel()
        {            
            // Init a probe
            probe = new ProbeModel
            {
                TotalElements = 64, // total 32 elements
                UsedElementsPerBeam = 1, // use all 32 elements
                Frequency = 5,
                Pitch = 1,
            };

            // Init Ascan
            InitAscan();

            // Init Cscan
            InitCscan();

        }

        public void InitAscan()
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

        public void InitCscan()
        {

        }     

        public async void ConnectDevice()
        {
            deviceModel = await Task.Run(() => new DeviceModel());
            SerialNumber = deviceModel.device.GetInfo().GetSerialNumber();
            IPAddress = deviceModel.device.GetInfo().GetAddressIPv4();

            // Create PA Beam Set
            deviceModel.CreatPABeamSet(probe);
            deviceModel.BindPAConnector();           

            InitAcquisition();

            // Init Ascan Settings
            AscanGain = deviceModel.beamSet.GetBeam(0).GetGain().ToString();
            AscanLength = deviceModel.beamSet.GetBeam(0).GetAscanLength().ToString();            

            // Start plotting
            StartPlotting();
        }

        public void StartPlotting()
        {
            deviceModel.acquisition.ApplyConfiguration();
            deviceModel.acquisition.Start();

            var taskConsumeData = new Task(() => deviceModel.ConsumeData());
            taskConsumeData.Start();

            var taskPlotting = new Task(() => Plotting());
            taskPlotting.Start();
        }      

        public void InitAcquisition()
        {
            deviceModel.InitiateAcquisition();
            Logging += "Init Acquisition!" + Environment.NewLine;
        }
        
        public void PlotAscan(int[] data)
        {
            lineSeries.Points.Clear();
            for (int i = 0; i < data.GetLength(0); i++)
            {
                lineSeries.Points.Add(new DataPoint(i, data[i]));
            }
            plotModel.InvalidatePlot(true);
        }        

        public void PlotCscan(int[][] data)
        {

        }

        public void Plotting()
        {
            int[][] rawData;
            int plottingIndex = 0;

            while (true)
            {
                try
                {
                    rawData = deviceModel.CollectRawData();

                    // Plot Ascan
                    PlotAscan(rawData[0]); // plot Beam 0 Ascan

                    // Plot Cscan
                    PlotCscan(rawData);

                    plottingIndex += 1;
                    Logging = plottingIndex.ToString();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
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
                    var beamCount = deviceModel.beamSet.GetBeamCount();
                    for (uint i = 0; i < beamCount; i++)
                    {
                        deviceModel.beamSet.GetBeam(i).SetGainEx(result);
                    }
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
                    var beamCount = deviceModel.beamSet.GetBeamCount();
                    for (uint i = 0; i < beamCount; i++)
                    {
                        deviceModel.beamSet.GetBeam(i).SetAscanLength(result);
                    }
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
