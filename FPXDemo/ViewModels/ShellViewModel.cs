using Caliburn.Micro;
using FPXDemo.Models;
using Heatmap;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FPXDemo.ViewModels
{
    class ShellViewModel : Screen
    {
        // Ascan settings
        public DeviceModel deviceModel { get; set; }
        public PlotModel plotModel { get; set; }
        public LineSeries lineSeries { get; set; }
        public ArrowAnnotation annotation { get; set; }

        // Cscan Settings
        public CscanModel cscanModel { get; set; }
        public HeatmapModel heatmapModel { get; set; }
        private BitmapSource _heatmapGraph;

        public BitmapSource heatmapGraph
        {
            get { return _heatmapGraph; }
            set
            {
                _heatmapGraph = value;
                NotifyOfPropertyChange(() => heatmapGraph);
            }
        }


        // Probe
        public ProbeModel probe { get; set; }

        // Gate 
        public GateModel gate { get; set; }

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
                TotalElements = 64, 
                UsedElementsPerBeam = 4,
                Frequency = 5,
                Pitch = 1,
            };

            // Init a gate
            gate = new GateModel
            {
                Start = 10,
                Length = 50,
                Threshold = 20,
            };
            GateStart = gate.Start.ToString();
            GateLength = gate.Length.ToString();
            GateThreshold = gate.Threshold.ToString();

            // Init Ascan
            InitAscan();
            PlotGate();

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

            // Add gate into Ascan Plot
            annotation = new ArrowAnnotation
            {
                HeadLength = 0,
                HeadWidth = 0,
                Text = "Gate",
                TextColor = OxyColors.Red,
                StrokeThickness = 5,
                Color = OxyColors.Red,
            };
            plotModel.Annotations.Add(annotation);
        }

        public void PlotGate()
        {
            if (annotation != null)
            {
                annotation.StartPoint = new DataPoint(gate.Start, gate.Threshold);
                annotation.EndPoint = new DataPoint(gate.Start + gate.Length, gate.Threshold);
                plotModel.InvalidatePlot(true);
            }
        }

        public void InitCscan()
        {
            cscanModel = new CscanModel
            {
                Width = 800,
                Height = (int)(probe.TotalElements - probe.UsedElementsPerBeam + 1),
            };

            heatmapModel = new HeatmapModel(cscanModel.Width, cscanModel.Height);

            heatmapGraph = heatmapModel.BitmapToImageSource();
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

        public void PlotCscan(int[][] data, int xPos)
        {
            int colorNumber = heatmapModel.colorList.Count();

            for (int i=0; i<data.GetLength(0); i++)
            {
                // Get cscan value with gate
                var loc = DetectSignal.CrossGateLocation(data[i], gate);

                // Get cscan paint color
                var color = (loc - gate.Start) * colorNumber / gate.Length;

                // Paint
                heatmapModel.PaintHeatMapPoint(new HeatmapPoint(xPos, i, (int)color));
            }

            // update cscan plotting
            heatmapGraph = heatmapModel.BitmapToImageSource();

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
                    PlotAscan(rawData[BeamIndex]); // plot selected Beam

                    // Plot Cscan
                    PlotCscan(rawData, plottingIndex);

                    // replot when cscan ends
                    if (plottingIndex > cscanModel.Width)
                    {
                        plottingIndex = 0;
                    }
                    else
                    {
                        plottingIndex += 1;
                    }
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

        private int _beamIndex = 0;

        public int BeamIndex
        {
            get { return _beamIndex; }
            set
            {
                _beamIndex = value;
                NotifyOfPropertyChange(() => BeamIndex);
            }
        }


        // Gate Settings UI
        private string _gateStart;

        public string GateStart
        {
            get { return _gateStart; }
            set
            {
                _gateStart = value;
                NotifyOfPropertyChange(() => GateStart);
                if (double.TryParse(_gateStart, out double result))
                {
                    gate.Start = result;
                    PlotGate();
                }
            }
        }

        private string _gateLength;

        public string GateLength
        {
            get { return _gateLength; }
            set
            {
                _gateLength = value;
                NotifyOfPropertyChange(() => GateLength);
                if (double.TryParse(_gateLength, out double result))
                {
                    gate.Length = result;
                    PlotGate();
                }
            }
        }

        private string _gateThreshold;

        public string GateThreshold
        {
            get { return _gateThreshold; }
            set
            {
                _gateThreshold = value;
                NotifyOfPropertyChange(() => GateThreshold);
                if (double.TryParse(_gateThreshold, out double result))
                {
                    gate.Threshold = result;
                    PlotGate();
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
