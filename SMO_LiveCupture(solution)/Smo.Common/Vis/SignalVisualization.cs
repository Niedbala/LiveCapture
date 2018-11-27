using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SmoReader.Entities;
using Smo.Common.Contracts;

namespace SmoReader.Vis
{
    public class SignalVisualization
    {
        private Chart _chart = null;
        private List<decimal> _timeBase = new List<decimal>();

        private AnalysisResult _analysisResult;


        private List<decimal> GenerateTimeBase(ITimeData timeData)
        {
            var firstTime = timeData.AcraTime.First();

            var timeBase = timeData.AcraTime.Select(s =>
             {
                 var tickDiff = (s.Ticks - firstTime.Ticks);

                 var tiks = (Decimal)(tickDiff) / (decimal)10000000;
                 return tiks;
             }).ToList();

            return timeBase;
        }

        public SignalVisualization(AnalysisResult analysisResult, Chart chart = null)
        {
            _analysisResult = analysisResult;


            if (chart == null)
            {
                chart = new Chart();

                chart.Width = 500;
                chart.Height = 150;
                var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
                chart.ChartAreas.Add(chartArea);
            }

            _chart = chart;

            _timeBase = GenerateTimeBase(analysisResult);

            Init();

        }

        private void Init()
        {           
           
        }


        public void AddVerticalLine(int index, Color color)
        {

            if (index < 0)
                return;

            if (index >= _timeBase.Count)
                return;

            var lineTime = _timeBase[index];
            // the vertical line
            var vline = new VerticalLineAnnotation()
            {
                AxisX = _chart.ChartAreas[0].AxisX,
                AllowMoving = false,
                IsInfinitive = true,
                ClipToChartArea = _chart.ChartAreas[0].Name,
                Name = Guid.NewGuid().ToString(),
                LineColor = color,
                LineWidth = 2,
                X = (double)lineTime
            };


            _chart.Annotations.Add(vline);
        }

        public void AddTGline(int index)
        {
            AddVerticalLine(index, Color.Orange);
        }

        public void AddTakeOffLine(int index)
        {
            AddVerticalLine(index, Color.Blue);
        }
        public void AddLandingLine(int index)
        {
            AddVerticalLine(index, Color.Red);
        }


        private DataPoint CastDataPoint(decimal x, object y)
        {
            var retPoint = new DataPoint();

            if (y is int)
                 retPoint = new DataPoint((double) x, (int)y);
            if (y is double)
                retPoint = new DataPoint((double)x, (double)y);
            if (y is int)
                retPoint = new DataPoint((double)x, (int)y);
            else
            {
                retPoint = new DataPoint((double)x, (double)y);
            }
           

            return retPoint;

        }

        public void DrawGraph(SignalData signal, string nameDecorator = "")
        {



            _chart.Series.Clear();


            var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series1",
                Color = System.Drawing.Color.Green,
                IsVisibleInLegend = false,
                IsXValueIndexed = false,
                ChartType = SeriesChartType.Line
            };

            var data = signal.Samples;

            _chart.Series.Add(series1);
            for (int i = 0; i < data.Length; i++)
            {

                var point = CastDataPoint(_timeBase[i], data[i]);
                series1.Points.Add(point);



            }

            //    var median = Median(data);

            var max = 0;
            var min = 0;


            if (signal.Samples[0].GetType() == typeof(decimal))
            {
                max = (int)((decimal)data.Max());
                min = (int)((decimal)data.Min());
            }
            else if (signal.Samples[0].GetType() == typeof(long))
            {
                max = (int)((long)data.Max());
                min = (int)((long)data.Min());
            }
            else if (signal.Samples[0].GetType() == typeof(double))
            {
                max = (int)(double)data.Max();
                min = (int)(double)data.Min();
            }
            else
            {
                max = (int)data.Max();
                min = (int)data.Min();
            }



            //maxLabel.Text = max.ToString();

            //minLabel.Text = min.ToString();

            //  var typ = min.GetType();




            var mean = ((max + min) / 2);

            var extentsExpander = (double?)Math.Max(0.05 * (double)mean, 1);

            extentsExpander = extentsExpander ?? 0;



            _chart.ChartAreas[0].AxisY.Minimum = (double)min - (double)extentsExpander;


            _chart.ChartAreas[0].AxisY.Maximum = (double)max + (double)extentsExpander;

            _chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            _chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;

            _chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            _chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;

            _chart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
            _chart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);

            _chart.Invalidate();

            _chart.Update();

            var parent = @"C:\Roboczy\analiza\";

            var fileName = $@"{_analysisResult.AcraStartTime}_to_{_analysisResult.AcraEndTime}_{signal.Name}.jpg".Replace("-", "_")
                .Replace(":", "_");

            var path = $@"{parent}{fileName}";
            // DirectoryInfo parent = Directory.GetParent(path);

            if (!Directory.Exists(parent))
            {

                Directory.CreateDirectory(parent);

            }






        }


        public void DrawGraphToFile(SignalData signal, string path)
        {
            DrawGraph(signal);
            _chart.SaveImage(path, ImageFormat.Jpeg);
        }


        public string DrawGraphToBase64(SignalData signal)
        {
            string base64String = null;

            using (var chartimage = new MemoryStream())
            {
                DrawGraph(signal);

                _chart.SaveImage(chartimage, ImageFormat.Jpeg);
                base64String = Convert.ToBase64String(chartimage.ToArray());
            }

            return base64String;
        }


    }
}
