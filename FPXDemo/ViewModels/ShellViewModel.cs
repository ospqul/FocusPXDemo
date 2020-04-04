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

        // Bscan Plotting
        public HeatMapSeries heatMapSeries { get; set; }
        public double[,] plotData = { };

        // Probe
        public ProbeModel probe { get; set; }

        // Sscan Model setting
        public SscanModel sscanModel { get; set; }
        public SscanGraphModel sscanGraph { get; set; }

        private string _serialNumber;
        private string _ipAddress;
        private string _beamSetName;

        private string _ascanGain;
        private string _ascanLength;

        private string _logging;


        public ShellViewModel()
        {
            //InitBscanPlot();

            // Init a probe
            probe = new ProbeModel
            {
                TotalElements = 32, // total 32 elements
                UsedElementsPerBeam = 32, // use all 32 elements
                Frequency = 5,
                Pitch = 1,
            };

            // Init Sscan Settings
            sscanModel = new SscanModel
            {
                StartAngle = -45, // degree
                EndAngle = 45,
                AngleResolution = 1,
                FocusDepth = 17,  //mm
            };

            // Init Sscan Grap and plot
            InitSscanGraph();
            InitSscanPlot();
        }

        public double[][] GetSscanDelay()
        {
            // Calculate sscan delays
            // Calculate element positions
            var positions = DelayLawModel.GetElementsPosition(probe);

            // Calculate element delays
            double velocity = 5800; // stainless steel block
            var delays = DelayLawModel.GetSscanDelays(positions, velocity, sscanModel);
            return delays;
        }

        public double[] GetDelays()
        {
            // Calculate element positions
            var positions = DelayLawModel.GetElementsPosition(probe);

            // Calculate element delays
            double velocity = 5800; // stainless steel block
            double depth = 17; // mm
            Point focalPoint = new Point(0, depth);
            var delays = DelayLawModel.GetElementDelays(positions, velocity, focalPoint);

            return delays;
        }

        public void InitBscanPlot()
        {
            plotModel = new PlotModel
            {
                Title = "Bscan Plotting",
            };

            // Add axis
            var axis = new LinearColorAxis();
            plotModel.Axes.Add(axis);

            // Add series
            heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = 25,
                Y0 = 0,
                Y1 = 5000,
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Bitmap,
                Data = plotData,
            };
            plotModel.Series.Add(heatMapSeries);

        }

        public void InitSscanGraph()
        {
            AxisModel XAxis = new AxisModel
            {
                Min = -20, // mm
                Max = 20, // mm
                Resolution = 0.1 // mm
            };

            AxisModel YAxis = new AxisModel
            {
                Min = 0, // mm
                Max = 50, // mm
                Resolution = 0.1 // mm
            };

            sscanGraph = new SscanGraphModel
            {
                XAxis = XAxis,
                YAxis = YAxis,
            };

        }

        public void InitSscanPlot()
        {
            plotModel = new PlotModel
            {
                Title = "Sscan Plotting",
            };

            // Add axis
            var axis = new LinearColorAxis();
            plotModel.Axes.Add(axis);

            // Add series
            heatMapSeries = new HeatMapSeries
            {
                X0 = sscanGraph.XAxis.Min,
                X1 = sscanGraph.XAxis.Max,
                Y0 = sscanGraph.YAxis.Min,
                Y1 = sscanGraph.YAxis.Max,
                Interpolate = true,
                RenderMethod = HeatMapRenderMethod.Bitmap,
                Data = plotData,
            };
            plotModel.Series.Add(heatMapSeries);

        }

        public async void ConnectDevice()
        {
            //deviceModel = new DeviceModel();
            deviceModel = await Task.Run(() => new DeviceModel());
            SerialNumber = deviceModel.device.GetInfo().GetSerialNumber();
            IPAddress = deviceModel.device.GetInfo().GetAddressIPv4();

            // Create Conventional Beam Set
            //CreateBeamSet();
            //BindConnector();

            // Create PA Beam Set
            //deviceModel.CreatPABeamSet();
            //deviceModel.BindPAConnector();

            // Create PA Focused Beam Set
            //var delays = GetDelays();
            //deviceModel.CreatPAFocusedBeamSet(probe, delays);
            //deviceModel.BindPAConnector();

            // Create PA Sscan Beam Set
            var delays = GetSscanDelay();
            deviceModel.CreatPASscanBeamSet(probe, delays);
            deviceModel.BindPAConnector();
            // correct sscan
            CorrectSscan(delays);

            InitAcquisition();

            // Init Ascan Settings
            AscanGain = deviceModel.beamSet.GetBeam(0).GetGain().ToString();
            AscanLength = deviceModel.beamSet.GetBeam(0).GetAscanLength().ToString();

            // Plotting Ascan
            //StartAscan();

            // Plotting Bscan
            //StartBscan();

            // Plotting Sscan
            StartSscan();
        }

        // take middle element's delay to correct sscan plotting
        public void CorrectSscan(double[][] delays)
        {
            uint middleElement = probe.UsedElementsPerBeam / 2;
            for (uint i=0; i< delays.GetLength(0); i++)
            {
                // set ascan start to the double of middle element's delay
                deviceModel.beamSet.GetBeam(i).SetAscanStart(delays[i][middleElement]*2);
            }
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

        public void PlottingBscan()
        {
            int[][] bscanData;
            int plottingIndex = 0;

            while (true)
            {
                try
                {
                    bscanData = deviceModel.CollectBscanData();
                    if (bscanData.GetLength(0) < 1)
                    { break; }

                    plotData = new double[bscanData.GetLength(0), bscanData[0].GetLength(0)];

                    for (int i=0; i< bscanData.GetLength(0); i++)
                    {
                        for (int j=0; j< bscanData[0].GetLength(0); j++)
                        {
                            plotData[i, j] = (double)bscanData[i][j];
                        }
                    }

                    plotModel.InvalidatePlot(true);
                    heatMapSeries.Data = plotData;

                    plottingIndex += 1;
                    Logging = plottingIndex.ToString();
                }
                catch (Exception)
                {

                    //throw;
                }
            }
        }

        public void PlottingSscan()
        {
            int[][] rawData;
            int plottingIndex = 0;

            while (true)
            {
                try
                {
                    rawData = deviceModel.CollectBscanData();
                    if (rawData.GetLength(0) < 1)
                    { break; }

                    // Get plot points
                    var plotPoints = sscanGraph.GetPlotPoints();

                    plotData = new double[plotPoints.GetLength(0), plotPoints.GetLength(1)];

                    // Assign plot value to each plot points
                    for (int xIndex=0; xIndex< plotPoints.GetLength(0); xIndex++)
                    {
                        for (int yIndex=0; yIndex< plotPoints.GetLength(1); yIndex++)
                        {
                            // current plot point
                            var p = plotPoints[xIndex, yIndex];
                            // X / Y = Tan(angle)
                            double angle = Math.Atan2(p.X, p.Y) * 180 / Math.PI;

                            // if angle is not in range (start angle, end angle)
                            // then assign this plot point value = 0
                            if ((angle < sscanModel.StartAngle) || (angle > sscanModel.EndAngle))
                            {
                                plotData[xIndex, yIndex] = 0;
                            }
                            // if angle is in this range, then assgin it's value according to raw data
                            else
                            {
                                double radius = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                                // find this plot point in which beam
                                int rawXIndex = (int)Math.Round((angle - sscanModel.StartAngle) / sscanModel.AngleResolution);
                                // find nearest Ascan value in this beam
                                double velocity = 5800; // m/s
                                int rawYIndex = (int)Math.Round((radius * 2e5) / velocity);
                                plotData[xIndex, yIndex] = Math.Abs(rawData[rawXIndex][rawYIndex]);
                            }
                        }
                    }
                    plotModel.InvalidatePlot(true);
                    heatMapSeries.Data = plotData;

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

        public void StartBscan()
        {
            deviceModel.acquisition.ApplyConfiguration();
            deviceModel.acquisition.Start();

            var taskConsumeData = new Task(() => deviceModel.ConsumeData());
            taskConsumeData.Start();

            var taskPlottingBscan = new Task(() => PlottingBscan());
            taskPlottingBscan.Start();
        }

        public void StartSscan()
        {
            deviceModel.acquisition.ApplyConfiguration();
            deviceModel.acquisition.Start();

            var taskConsumeData = new Task(() => deviceModel.ConsumeData());
            taskConsumeData.Start();

            var taskPlottingSscan = new Task(() => PlottingSscan());
            taskPlottingSscan.Start();
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
