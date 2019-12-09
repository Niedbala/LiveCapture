using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
//using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.AirPcap;
using SharpPcap.WinPcap;
using PacketDotNet;
using Smo.Startup;
using Smo.Common.Entities;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using SciChart.Core.Extensions;


namespace WinformsExample
{
    public partial class Chart_form : Form
    {
        private double[] ChartArray = new double[1000];
        private bool stop_chart = false;
        public static bool every = true;
        public static bool dy = false;

        private Set_chart_form set_chart_form = new Set_chart_form();
        private Thread Time;
        public static int chart_width = 10000;
        public static bool show_last_value = true;
        public static bool show_legend = true;
        Warning_Form warnform = new Warning_Form();
        Warningform2 warnform2 = new Warningform2();
       // Marker_Setting marker_setings_form = new Marker_Setting();
        public static int view_point = 0;
        public static string resampling = "1";
        public static Dictionary<string, List<ValueType>> extractedSamples = new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, List<ValueType>> extractedPartOfSamples= new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, List<ValueType>> extractedPartOfSamples2 = new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, List<ValueType>> extractedSamples2 = new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, List<ValueType>> temp_exSam = new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, string> dic1 = new Dictionary<string, string>();
        public static Dictionary<string, string> dic2 = new Dictionary<string, string>();
        public static List<string> keys = new List<string>();
        public static List<string> occurence = new List<string>();
        // public static List<DateTime> timearray_128f = new List<DateTime>();
        // public static List<DateTime> timearray_256f = new List<DateTime>();
        public static List<double> timearrayf = new List<double>();
        public static List<double> timearrayf2 = new List<double>();
        public static List<double> timearray_128f = new List<double>();
        public static List<double> timearray_256f = new List<double>();
        public static Dictionary<string, List<double>> time_dictionatyf = new Dictionary<string, List<double>>();
        public static Dictionary<string, List<double>> time_dictionatyf2 = new Dictionary<string, List<double>>();
        public static string grid = "Auto";
        private double axismax;
        private double axismin;
        private int axis_changed = 0;
        private double axis1 = 0.0;
        private double axis2 = 0.0;
        private int packetCount;
        public static List<ParameterDefinition> parametr_definition = new List<ParameterDefinition>();
        public static List<int> repeat_occurance = new List<int>();
        public static Dictionary<int, List<int>> occurance = new Dictionary<int, List<int>>();
        public static Dictionary<int, double> last_times = new Dictionary<int, double>();
        public static int xValue;
        public static int series_iter;
        public static List<Series> list_series = new List<Series>();
        public static List<Series> list_series2 = new List<Series>();
        public static string[] mrk_series = new string[4];
        public static bool mouse_event = false;
        public static int hold_coursor = 0;
        public static bool croos_cursor_on = true;
        public static Dictionary<int,List<string>> MarkerSeries = new Dictionary<int, List<string>>();
        public static int ProbkowanieLast = 0;
        public static bool indexing = true;
        public static bool indexing2 = true;
        public static int globe_max_count;
        public static string globe_max_name;
        public static Dictionary<int, Dictionary<string, List<ValueType>>> interpolate_samples = new Dictionary<int, Dictionary<string, List<ValueType>>>();
        public static Dictionary<int, Dictionary<string, List<ValueType>>> interpolate_samples2 = new Dictionary<int, Dictionary<string, List<ValueType>>>();

        public int X_diff { get; private set; }

        public Chart_form()
        {
            InitializeComponent();
            var sample = CaptureForm.extractedSamples;
            var parameter_def = CaptureForm.parametr_definition;
            if (dic1.Count == 0)
            {
                parameter_def.ForEach(x => keys.Add(x.ToString()));
                parameter_def.ForEach(x => occurence.Add(x.Occurrences.ToString()));
                //dodawanie chasklistbox;
                dic1 = keys.Zip(occurence, (k, v) => new { k, v })
                  .ToDictionary(x => x.k, x => x.v);
            }
            dodawanie_checklistbox(keys, occurence, checkedListBox1);
            dodawanie_checklistbox(keys, occurence, checkedListBox2);



            chart1.Series.Clear();

            chart1.Titles.Add("opis").Name = "opis";
            chart1.Titles["opis"].Visible = false;

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;

            //chart1.ChartAreas[0].AxisX.IsReversed = true;
            SetZoom(chart1);


            chart2.Series.Clear();

            chart2.Titles.Add("opis").Name = "opis";
            chart2.Titles["opis"].Visible = false;

            chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart2.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;

            //chart2.ChartAreas[0].AxisX.IsReversed = true;

            SetZoom(chart2);

            axismax = chart1.ChartAreas[0].AxisY.ScaleView.ViewMaximum;

            axismin = chart1.ChartAreas[0].AxisY.ScaleView.ViewMinimum;

        }
        private void TimeHandler()
        {
            while (true)
            {

                Thread.Sleep(1000);
            }
        }

        private void SetZoom(Chart chart)
        {


            chart.ChartAreas[0].CursorX.Interval = 0.1;
            chart.ChartAreas[0].CursorY.Interval = 0.1;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;

            chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart.ChartAreas[0].CursorX.SelectionColor = Color.DarkGray;
            chart.ChartAreas[0].CursorY.SelectionColor = Color.DarkGray;

        }

        private void dodawanie_checklistbox(List<string> keys, List<string> occurences,CheckedListBox checkedListBox)
        {
            checkedListBox.Items.Clear();
            
            List<string> name_list = new List<string>();

            for (int i = keys.Count() - 1; i >= 0; i--)
            {
                string[] split_key = keys[i].ToString().Split('_');
                var name = "(" + occurences[i] + ")" + split_key[split_key.Count() - 1];

                if (name.Contains("AnalogIn")) { name = name + "_" + split_key[1]; }
                if (name.Contains("Analog")) { name = name + "_" + split_key[4]; }
                
                name_list.Add(name);
                checkedListBox.Items.Insert(0, name);
            }



        }

        public static List<double> samplings = new List<double>();

        public void dodawanie_checklistbox2(List<string> keys, CheckedListBox checkedListBox, Dictionary<string, List<ValueType>> extractedSamples)
        {
            checkedListBox.Items.Clear();

            List<string> name_list = new List<string>();

            for (int i = keys.Count() - 1; i >= 0; i--)
            {
                string[] split_key = keys[i].ToString().Split('_');
                
                var name = "(" + extractedSamples[keys[i]].Count.ToString() + ")" + split_key[split_key.Count() - 1];
                if (!samplings.Contains(extractedSamples[keys[i]].Count)) { samplings.Add(extractedSamples[keys[i]].Count); };
                if (name.Contains("AnalogIn")) { name = name + "_" + split_key[1]; }
                if (name.Contains("Analog")) { name = name + "_" + split_key[4]; }
                if (name.Split('_').Last() == "B") { name = name + "_" + split_key[5]; }
                name_list.Add(name);
                checkedListBox.Items.Insert(0, name);
            }



        }

        private void dodawanie_checklistbox3(List<string> keys, CheckedListBox checkedListBox)
        {
            checkedListBox.Items.Clear();

            List<string> name_list = new List<string>();

            for (int i = keys.Count() - 1; i >= 0; i--)
            {
                string[] split_key = keys[i].ToString().Split('_');

                var name = "(" + extractedSamples2[keys[i]].Count.ToString() + ")" + split_key[split_key.Count() - 1];

                if (name.Contains("AnalogIn")) { name = name + "_" + split_key[1]; }
                name_list.Add(name);
                checkedListBox.Items.Insert(0, name);
            }



        }
        private void StartChart()
        {

            try
            {
                chart1.Series.Clear();
                chart2.Series.Clear();
                List<bool> co_eksportowac1 = new List<bool>();

                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (checkedListBox1.GetItemCheckState(i).ToString() == "Checked")
                        co_eksportowac1.Add(true);
                    else
                        co_eksportowac1.Add(false);
                }

                List<bool> co_eksportowac2 = new List<bool>();

                for (int i = 0; i < checkedListBox2.Items.Count; i++)
                {
                    if (checkedListBox2.GetItemCheckState(i).ToString() == "Checked")
                        co_eksportowac2.Add(true);
                    else
                        co_eksportowac2.Add(false);
                }

                //chart1.Series["Series1"].Points.Clear();
                //List<string> keys = new List<string>(CaptureForm.extractedSamples.Keys);
                int probkowanie = 0;
                var index = 0;
                for (int i = 0; i <= co_eksportowac1.Count() - 1; i++)
                    if (co_eksportowac1[i])
                    {
                        var probkowanie_key = 1;
                        index++;
                        try
                        {
                            probkowanie_key = Int32.Parse(dic1[keys[i]]);//.Split(')').First().Split('(').Last();
                        }
                        catch
                        {
                            return;
                        }
                        if (probkowanie == 0)
                        { probkowanie = probkowanie_key; }
                        if (probkowanie != probkowanie_key) { warnform.ShowDialog(); break; }
                        chart1.Series.Add(keys[i]).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine; ;
                        chart1.Series[keys[i]].ToolTip = "x = {#VALX}" + '\r' + '\n' + "y = {#VALY}";
                        chart1.Series[keys[i]].XValueType = ChartValueType.DateTime;
                        chart1.Series[keys[i]].IsXValueIndexed = indexing;
                        chart1.Series[keys[i]].SmartLabelStyle.Enabled = false;
                        chart1.Series[keys[i]].LegendText =
                            "#" + index.ToString() + " " + keys[i];
                        //chart1.Series[keys[i]].IsValueShownAsLabel = true;
                    }

                chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
                chart1.ChartAreas[0].AxisY.LabelStyle.Format = "N2";
                probkowanie = 0;
                for (int i = 0; i <= co_eksportowac2.Count() - 1; i++)
                    if (co_eksportowac2[i])
                    {
                        var probkowanie_key = Int32.Parse(dic1[keys[i]]);//.Split(')').First().Split('(').Last();
                        if (probkowanie == 0)
                        { probkowanie = probkowanie_key; }
                        if (probkowanie != probkowanie_key) { warnform.ShowDialog(); break; }
                        chart2.Series.Add(keys[i]).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine; ;
                        chart2.Series[keys[i]].ToolTip = "x = {#VALX}" + '\r' + '\n' + "y = {#VALY}";
                        chart2.Series[keys[i]].XValueType = ChartValueType.DateTime;
                        chart2.Series[keys[i]].IsXValueIndexed = true;
                       
                    }
                chart2.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
                chart2.ChartAreas[0].AxisY.LabelStyle.Format = "N2";
                list_series = chart1.Series.ToList();
                list_series2 = chart2.Series.ToList();
                Time = new Thread(new ThreadStart(this.TimeHandler));
                Time.IsBackground = true;
                Time.Start();

                if (MarkerSeries.Count == 0)
                {
                    var mrk_ser = new List<string>();
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            mrk_ser.Add(list_series[i].ToString());
                        }
                        catch
                        {
                            mrk_ser.Add("");
                        }

                    }


                    for (int i = 0; i < 4; i++)
                    {
                        MarkerSeries.Add(i, new List<string>(mrk_ser));
                    }

                }
                else
                {
                    foreach (var key in MarkerSeries.Keys)
                    {
                        //for (var i = 0; i < MarkerSeries[key].Count - 1; i++)
                        //{
                        var i = 0;
                        var series = list_series.Select(x => x.ToString()).ToList();
                        while (i < MarkerSeries[key].Count)
                        {
                            var str = (MarkerSeries[key][i]).ToString();
                            var b = (series.Contains(str));
                            if (!((series.Contains(MarkerSeries[key][i])) || (MarkerSeries[key][i] == "")))
                            {
                                if (!(i == 2))
                                {
                                    MarkerSeries[key][i] = MarkerSeries[key][i + 1];
                                    MarkerSeries[key][i + 1] = "";
                                }
                                else
                                { MarkerSeries[key][i] = ""; }
                            }
                            else { i++; }
                        }

                        if (MarkerSeries[key].All(x => x == ""))
                        {
                            try
                            {
                                MarkerSeries[key][0] = list_series[0].ToString();
                            }
                            catch { }
                            try
                            {
                                MarkerSeries[key][1] = list_series[1].ToString();
                            }
                            catch { }
                            try
                            {
                                MarkerSeries[key][2] = list_series[2].ToString();
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }
        
        private List<Series> StartChart_file(Chart chart1, CheckedListBox checkedListBox1, Dictionary<string, List<ValueType>> extractedSamples, bool isIndexed, List<Series> list_series,bool indexing )
        {

            if (grid == "None") { chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false; }
            else if (grid == "Auto") { chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true; }
            else { chart1.ChartAreas[0].AxisX.MajorGrid.Interval = Int64.Parse(grid); }

            if (grid == "None") { chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = false; }
            else if (grid == "Auto") { chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = true; }
            else { chart2.ChartAreas[0].AxisX.MajorGrid.Interval = Int64.Parse(grid); }

            chart1.Series.Clear();
            
            List<bool> co_eksportowac1 = new List<bool>();

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemCheckState(i).ToString() == "Checked")
                    co_eksportowac1.Add(true);
                else
                    co_eksportowac1.Add(false);
            }


            //chart1.Series["Series1"].Points.Clear();
            List<string> keys = new List<string>(extractedSamples.Keys);
            int probkowanie = 0;
            var index = 0;
            var max_count = 0;
            var max_name = "";
            for (int i = 0; i <= co_eksportowac1.Count() - 1; i++)
                if (co_eksportowac1[i])
                {
                    index++;
                    //if (indexing)
                    //{
                    //    var probkowanie_key = 1;
                    //    try
                    //    {
                    //        probkowanie_key = Int32.Parse(extractedSamples[keys[i]].Count.ToString());//.Split(')').First().Split('(').Last();
                    //    }
                    //    catch
                    //    {
                    //        return;
                    //    }
                    //    if (probkowanie == 0)
                    //    {
                    //        probkowanie = probkowanie_key;
                    //    }
                    //    if (probkowanie != probkowanie_key) { warnform.ShowDialog(); break; }
                    //    probkowanieNew = probkowanie;

                    //    if (probkowanieNew != ProbkowanieLast)
                    //    {
                    //        chart1.ChartAreas[0].AxisX.ScaleView.Position = double.NaN;
                    //        chart1.ChartAreas[0].AxisX.ScaleView.Size = double.NaN;
                    //        chart1.ChartAreas[0].AxisY.ScaleView.Position = double.NaN;
                    //        chart1.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
                    //        chart1.ChartAreas[0].AxisX.MajorGrid.Interval = double.NaN;
                    //        chart1.ChartAreas[0].AxisY.MajorGrid.Interval = double.NaN;

                    //    }
                    //}
                    //else
                    //{
                    var key_count = Int32.Parse(extractedSamples[keys[i]].Count.ToString());
                    
                    if ( key_count > max_count) { max_count = key_count; max_name = keys[i]; }

                    chart1.ChartAreas[0].AxisX.ScaleView.Position = double.NaN;
                        chart1.ChartAreas[0].AxisX.ScaleView.Size = double.NaN;
                        chart1.ChartAreas[0].AxisY.ScaleView.Position = double.NaN;
                        chart1.ChartAreas[0].AxisY.ScaleView.Size = double.NaN;
                        chart1.ChartAreas[0].AxisX.MajorGrid.Interval = double.NaN;
                        chart1.ChartAreas[0].AxisY.MajorGrid.Interval = double.NaN;
                    //}
                    chart1.Series.Add(keys[i]).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine; ;
                    chart1.Series[keys[i]].ToolTip = "x = {#VALX}" + '\r' + '\n' + "y = {#VALY}";
                    chart1.Series[keys[i]].XValueType = ChartValueType.Double;
                    chart1.Series[keys[i]].IsXValueIndexed = indexing;  // tu zmieniłem
                    chart1.Series[keys[i]].SmartLabelStyle.Enabled = false;
                    chart1.Series[keys[i]].IsVisibleInLegend = show_legend;
                    chart1.Series[keys[i]].LegendText =
                        "#" + index.ToString() + " " + keys[i];
                    //chart1.Series[keys[i]].IsValueShownAsLabel = true;
                }
            globe_max_count = max_count;
            globe_max_name = max_name;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "N2";
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "N2";
            list_series = chart1.Series.ToList();



            if (MarkerSeries.Count == 0)
            {
                var mrk_ser = new List<string>();
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        mrk_ser.Add(list_series[i].ToString());
                    }
                    catch
                    {
                        mrk_ser.Add("");
                    }

                }


                for (int i = 0; i < 4; i++)
                {
                    MarkerSeries.Add(i, new List<string>(mrk_ser));
                }

            }
            else
            {
                foreach (var key in MarkerSeries.Keys)
                {
                    //for (var i = 0; i < MarkerSeries[key].Count - 1; i++)
                    //{
                    var i = 0;
                    var series = list_series.Select(x => x.ToString()).ToList();
                    while( i < MarkerSeries[key].Count)
                    {
                        var str = (MarkerSeries[key][i]).ToString();
                        var b = (series.Contains(str));
                        if (!((series.Contains(MarkerSeries[key][i])) || (MarkerSeries[key][i] =="")))
                        {
                            if (!(i == 2))
                            {
                                MarkerSeries[key][i] = MarkerSeries[key][i + 1];
                                MarkerSeries[key][i + 1] = "";
                            }
                            else
                            { MarkerSeries[key][i] = ""; }
                        }
                        else { i++; }
                    }

                    if (MarkerSeries[key].All(x => x =="")) {
                        try
                        {
                            MarkerSeries[key][0] = list_series[0].ToString();
                        }
                        catch { }
                        try
                        {
                            MarkerSeries[key][1] = list_series[1].ToString();
                        }
                        catch { }
                        try
                        {
                            MarkerSeries[key][2] = list_series[2].ToString();
                        }
                        catch { }
                    }
                }
            }

            return list_series;

        }

            private void toolStripButton1_Click(object sender, EventArgs e)
        {
            StartChart();
            CaptureForm.start_chart = true;
            stop_chart = false;

        }
        public void UpdateChart()
        {
            if (stop_chart == false)
            {
               
                Changechart(chart1,list_series);
                
                Changechart(chart2,list_series2);
            }

        }




        public void Changechart(Chart chart, List<Series> list_series)
        {

            //var series = chart.Series.ToList();

            List<DateTime> timebase = new List<DateTime>();
            for (var index = 0; index < list_series.Count; index++)
            {
                Series seria = list_series[index];
                chart.Series[seria.ToString().Substring(7)].Points.Clear();

                string name = seria.ToString().Split('-')[1];
                var chartarray = CaptureForm.extractedSamples[seria.ToString().Substring(7)]
                    .Select(x => Convert.ToDouble(x)).ToList();

                

                
                int iter = 0;
                if (chartarray.Count() < chart_width)
                {
                    iter = 0;
                }
                else
                {
                    iter = chartarray.Count() - chart_width - 1;
                }

                var time_divide = dic1[name];
                //if (time_divide < 3) { timebase = CaptureForm.timearray; }
                // if (time_divide > 120 && time_divide < 130) { timebase = CaptureForm.timearray_128; }
                // if (time_divide > 250 && time_divide < 260) { timebase = CaptureForm.timearray_256; }
                timebase = CaptureForm.time_dictionaty[time_divide];

                var sw = new Stopwatch();
                sw.Start();
                int iteration = Int32.Parse(resampling);
                if (every)
                {
                    iteration = Int32.Parse(resampling.Split('_').Last());
                    //if (iteration == 1)
                    //{
                       // chart.Series[seria.ToString().Substring(7)].Points.DataBindXY(timebase, chartarray);
                    //}
                    //else
                    //{
                        for (int i = iter; i < chartarray.Count() - 1; i += iteration)
                        {
                            chart.Series[seria.ToString().Substring(7)].Points.AddXY(timebase[i], chartarray[i]);
                        
                            //chart1.Series[seria.ToString().Substring(7)].XValueType(Data);
                        }
                    //}

                }

                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds.ToString());
                sw.Start();
                if (dy)
                {
                    int iterable = 0;
                    bool dir = chartarray[iter] - chartarray[iter - 1] > 0;
                    for (int i = iter + 1; i < chartarray.Count() - 1; i++)
                    {
                        if ((chartarray[i] - chartarray[i - 1] > 0 && !dir) ||
                            (chartarray[i] - chartarray[i - 1] < 0 && dir))
                        {
                            chart.Series[seria.ToString().Substring(7)].Points
                                .AddXY(timebase[i - 1], chartarray[i - 1]);
                            dir = !dir;
                            iterable++;
                        }
                    }
                }

                //chart.Series[seria.ToString().Substring(7)].LegendText =
                   // "#" + index.ToString() + chart.Series[seria.ToString().Substring(7)].LegendText;

                                                                         // chart.DataBindCrossTable(chartarray);
                                                                         //chart.Series[seria.ToString().Substring(7)].Points.Last().Label = chartarray[chartarray.Count() - 1].ToString();
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds.ToString());
            }

            if (true)
            {
                try
                {
                    
                    var name = list_series[series_iter].ToString().Substring(7);
                    //.Single(s => s.Equals(max));
                    var YVAL = CaptureForm.extractedSamples[name][xValue];
                    label4.Text = String.Concat(String.Concat(xValue.ToString(), " , "), YVAL.ToString());

                    //int which_point = (view_point * (chartarray.Count() - 10) / 1000) + 1;
                    //string[] split_key = seria.ToString().Split('_');
                    //chart.Series[seria.ToString().Substring(7)].LegendText = split_key[split_key.Count() - 1] + ";Val:" + Math.Round(chart.Series[seria.ToString().Substring(7)].Points[which_point].YValues[0]).ToString();
                    //chart.Series[seria.ToString().Substring(7)].LegendText = split_key[split_key.Count() - 1] + ";Val:" + Math.Round(chart.Series[seria.ToString().Substring(7)].Points.Last().YValues[0]).ToString();
                }
                catch (Exception ex)
                {

                }
                //string[] split_key = keys[i].ToString().Split('_')
                //chart.Series[seria.ToString().Substring(7)].Points.Last;
            }



        }
        //kvar sw = new Stopwatch();
        public void Changechartfile(Chart chart, Dictionary<string, List<ValueType>> extractedSamples, Dictionary<string, List<double>> time_dictionatyf, Dictionary<string, string> dic, List<Series> list_series, Dictionary<int, Dictionary<string, List<ValueType>>> interpolate_samples)
        {



            var series = chart.Series.ToList();

            //List<DateTime> timebase = new List<DateTime>();
             timebase = new List<double>();
             for (var index = 0; index < series.Count; index++)
             {
                 Series seria = series[index];
                 chart.Series[seria.ToString().Substring(7)].Points.Clear();
                 var strings = seria.ToString().Split('-');
                 string name = strings[1];
                // if (strings.Count() == 2) { name = strings[1]; }
                // else if (strings.Count() == 3) { name = strings[1] + '-' + strings[2]; }
                // else if (strings.Count() == 4) { name = strings[1].Remove(strings[1].Length - 1); }
                var num = extractedSamples[seria.ToString().Substring(7)].Count;
                var chartarray = new List<double>();
                if (indexing)
                {
                    if (num == globe_max_count)
                    {
                        chartarray = extractedSamples[seria.ToString().Substring(7)].Select(x => Convert.ToDouble(x))
                           .ToList();
                    }
                    else
                    {
                        chartarray = interpolate_samples[num][seria.ToString().Substring(7)].Select(x => Convert.ToDouble(x))
                           .ToList();
                    }
                    name = globe_max_name;
                }
                else
                {
                    chartarray = extractedSamples[seria.ToString().Substring(7)].Select(x => Convert.ToDouble(x))
                       .ToList();
                }
               
                 int iter = 1;
                 string time_divide = "";
                 try
                 {
                     time_divide = dic[name];
                 }
                 catch
                 {
                     try
                     {
                         time_divide = dic[name.Remove(strings[1].Length - 1)];
                     }
                     catch
                     {
                         try
                         {
                             time_divide = dic[strings[1] + '-' + strings[2].Remove(strings[2].Length - 1)];
                         }
                         catch
                         {
                             time_divide = dic[strings[1] + '-' + strings[2]];
                         }
                     }
                 }

                 timebase = time_dictionatyf[time_divide];

                 //var time_divide = chartarray.Count() / (timearrayf.Count() - 1);
                 //if (time_divide < 3) { timebase = timearrayf; }
                 //if (time_divide > 120 && time_divide < 130) { timebase = timearray_128f; }
                 //if (time_divide > 250 && time_divide < 260) { timebase = timearray_256f; }


                 int iteration = Int32.Parse(resamplingFile);
                 if (everyFile)
                 {
                     iteration = Int32.Parse(resamplingFile.Split('_').Last());
                    if (iteration == 1)
                    {
                        chart.Series[seria.ToString().Substring(7)].Points.DataBindXY(timebase, chartarray);
                    }
                    else
                    {
                        for (int i = iter; i < chartarray.Count() - 1; i += iteration)
                        {
                            chart.Series[seria.ToString().Substring(7)].Points.AddXY(timebase[i], chartarray[i]);

                            //    chart1.Series[seria.ToString().Substring(7)].XValueType(Data);
                        }
                    }
                   
                }
                 //sw.Stop();
                 //Console.WriteLine(sw.ElapsedMilliseconds.ToString());
                 //sw.Start();

                 if (dyFile)
                 {
                     int iterable = 0;
                     bool dir = chartarray[iter] - chartarray[iter - 1] > 0;
                     for (int i = iter + 1; i < chartarray.Count() - 1; i++)
                     {
                         if ((chartarray[i] - chartarray[i - 1] > 0 && !dir) ||
                             (chartarray[i] - chartarray[i - 1] < 0 && dir))
                         {
                             chart.Series[seria.ToString().Substring(7)].Points
                                 .AddXY(timebase[i - 1], chartarray[i - 1]);
                             dir = !dir;
                             iterable++;
                         }
                     }
                 }
                
                

                 // chart.DataBindCrossTable(chartarray);
                 //chart.Series[seria.ToString().Substring(7)].Points.Last().Label = chartarray[chartarray.Count() - 1].ToString();
                 //sw.Stop();
                 // Console.WriteLine(sw.ElapsedMilliseconds.ToString());
             }

            //if (probkowanieNew != ProbkowanieLast)
            //{
            //    chart.ChartAreas[0].AxisX.Maximum = timebase.Count();

            //}
        }
       


        private void Chart_form_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(button6, "Draw sample settings");
        }

        private void Chart_form_FormClosing(object sender, FormClosingEventArgs e)
        {
            CaptureForm.start_chart = false;
            Thread.Sleep(2000);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            stop_chart = true;
            extractedSamples = new Dictionary<string, List<ValueType>>();
            foreach (var series in list_series)
            {
                var data = chart1.Series[series.ToString().Substring(7)].Points.ToList().Select(x => (ValueType)x.YValues[0]).ToList();
                
                extractedSamples[series.ToString().Substring(7)] = new List<ValueType>(data);
                
            }
            time_dictionatyf["1"] = chart1.Series[list_series[0].ToString().Substring(7)].Points.ToList().Select(x => x.XValue).ToList();
            dic1 = new Dictionary<string, string>();
            dic1[list_series[0].ToString().Substring(7)] = "1";
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            set_chart_form = new Set_chart_form();
            set_chart_form.Show();
        }

        //private void trackBar1_Scroll(object sender, EventArgs e)
        //{
        //    view_point = trackBar1.Value;
        //}

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
            
        private void textBox1_Click(object sender, EventArgs e)
        {

            //OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\";
            //openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            //openFileDialog1.FilterIndex = 2;
            //openFileDialog1.RestoreDirectory = true;
            //openFileDialog1.ShowDialog();

            //textBox1.Text = openFileDialog1.FileName;


            FolderBrowserDialog katalog = new FolderBrowserDialog();
            using (StreamReader sr = new StreamReader("actual_settings\\path.txt"))
            {

                katalog.SelectedPath = sr.ReadLine();


            }

            katalog.ShowDialog();
            var kat_sciezka = katalog.SelectedPath.ToString();
            textBox1.Text = kat_sciezka;
            using (StreamWriter sw = File.CreateText("actual_settings\\path.txt"))
            {
                sw.WriteLine(katalog.SelectedPath);
            }
            

            if (kat_sciezka.Count() > 0)
            {
                textBox1.Text = kat_sciezka;
                DirectoryInfo katalog2 = new DirectoryInfo(kat_sciezka);
                try
                {
                    FileInfo[] pliki_test = katalog2.GetFiles(".cap");
                }

                catch (DirectoryNotFoundException ex)
                {
                    MessageBox.Show("Brak katalogu o podanej nazwie.", "UWAGA!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //this.Close(); //tak mozna zamknac okno
                    return;
                }


                FileInfo[] pliki = katalog2.GetFiles();
                comboBox1.Items.Clear();
                comboBox1.Text = "";
                comboBox1.Items.AddRange(pliki);

            }
            }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Wskaż plik", "UWAGA!", MessageBoxButtons.OK);
                return;
            }
            if (textBox1.Text == "")
            {
                warnform2.ShowDialog(); return;

            }
            else
            {
                extractedSamples = new Dictionary<string, List<ValueType>>();
                time_dictionatyf = new Dictionary<string, List<double>>();
                timearrayf = new List<double>();
                timearray_128f = new List<double>();
                timearray_256f = new List<double>();
                last_times = new Dictionary<int, double>();
                dic1 = new Dictionary<string, string>();
                packetIndex = 0;
                packetCount = 0;
                keys = new List<string>();
                occurance = new Dictionary<int, List<int>>();
                ICaptureDevice device;
                string capFile = textBox1.Text + "\\" + comboBox1.SelectedItem.ToString();
                if (capFile[capFile.Count() - 3] == 'c')
                {
                    try
                    {
                        // Get an offline device
                        device = new CaptureFileReaderDevice(capFile);

                        // Open the device
                        device.Open();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Caught exception when opening file" + ex.ToString());
                        return;
                    }
                    //var converter = Converter.BuildLiveConverter(CaptureForm.aircraftname, CaptureForm.path_configxml, CaptureForm.path_scalingtable, CaptureForm.path_instrumentsetting);
                    var converter = Converter.BuildLiveConverter(CaptureForm.aircraftname, CaptureForm.path_configxml, CaptureForm.path_scalingtable, CaptureForm.path_instrumentsetting);
                    device.OnPacketArrival +=
                    new PacketArrivalEventHandler((_sender, _e) => device_OnPacketArrival(this, _e, converter, extractedSamples, time_dictionatyf));
                    device.Capture();
                    var keys2 = new List<string>(extractedSamples.Keys);
                    try
                    {
                        this.Invoke((MethodInvoker)delegate { Updatelabel2(); });
                        //UpdateChart();
                    }
                    catch
                    {
                    }
                    dodawanie_checklistbox2(keys2, checkedListBox1, extractedSamples);
                    if (dic1.Count == 0)
                    {
                        parametr_definition.ForEach(x => keys.Add(x.ToString()));
                        parametr_definition.ForEach(x => occurence.Add(x.Occurrences.ToString()));
                        //dodawanie chasklistbox;
                        dic1 = keys.Zip(occurence, (k, v) => new { k, v })
                          .ToDictionary(x => x.k, x => x.v);
                    }
                    
                    Dictionary <string, int> dictionary_keys = new Dictionary<string, int>();
                    var keys3 = new List<string>();
                    var occurence2 = new List<int>();
                    extractedSamples.Keys.ToList().ForEach(x => keys3.Add(x));
                    extractedSamples.Keys.ToList().ForEach(x => occurence2.Add(extractedSamples[x].Count));
                   // var min_occure = occurence2.Min();
                   // var occurence3 = occurence2.Select(x => x / min_occure).ToList();
                    var dic3= keys3.Zip(occurence2, (k, v) => new { k, v })
                          .ToDictionary(x => x.k, x => x.v);

                    var occure2= dic3.Values.ToList().Distinct().ToList();
                    //var occure2 = ocure.Select(x => x).ToList();
                    occure2.Sort();
                    var occure3 = new List<int> (occure2);
                    occure2.Remove(occure2.Last());
                    occure3.Remove(occure3.First());
                   
                    int i = 0;
                    List<string> parametry = new List<string>();
                    interpolate_samples = new Dictionary<int, Dictionary<string, List<ValueType>>>();
                    foreach (var num in occure2)
                    {
                        parametry.AddRange(dic3.Where(x => x.Value == num).Select(x => x.Key).ToList());
                        interpolate_samples[num] = new Dictionary<string, List<ValueType>>();
                        foreach (var par in parametry)
                        {
                            var new_signal = interpolate(extractedSamples[par], occure3[i]);

                            
                            
                            interpolate_samples[num][par] = new List<ValueType>(new_signal);

                            //if (interpolate_samples.ContainsKey(num))
                            //{
                            //    if(interpolate_samples[num].ContainsKey(par))
                            //    {
                                    
                            //    }
                            //    else
                            //    {

                            //    }
                             
                            //}
                            //else
                            //{

                            //}


                            //if (occurance.ContainsKey(x.StreamID))
                            //{
                            //    occurance?[x.StreamID].Add(x.Occurrences);
                            //    repeat_occurance.Add(x.Occurrences);
                            //}
                            //else
                            //{
                            //    occurance[x.StreamID] = new List<int>() { x.Occurrences };
                            //    repeat_occurance.Add(x.Occurrences);
                            //}
                        }
                        i++;
                        
                    }
                    

                }
                if (capFile[capFile.Count() - 3] == 't')
                {
                    //MessageBox.Show("odczyt tsv", "UWAGA!", MessageBoxButtons.OK);
                    text_file_reader(capFile,extractedSamples,time_dictionatyf, checkedListBox1, dic1);
                    return;
                }





            }
        }

        public List<ValueType> interpolate (List<ValueType> Y, double SampleCount)
        {   //moge tu policzyc count z Y
            //mam ile musi byc probek na kocu

            var ilosc = Y.Count();
            var SampleRate = SampleCount / ilosc;

            List<ValueType> new_signal = new List<ValueType>();
            if (SampleRate == 2)
            {
               
                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add((UInt32)Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < SampleCount; i++)
                        { new_signal.Add((UInt32)Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < SampleRate; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                new_signal.Add((UInt32)Y[Y.Count() - 1]);
                new_signal.Add((UInt32)Y[Y.Count() - 1]);
            }
            else if (SampleRate < 2)
            {
                
                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add((UInt32)Y[a]);
                    
                }
                new_signal.Add((UInt32)Y[Y.Count() - 1]);
                
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add((UInt32)0); }
            }
            else if (SampleRate > 2 && SampleRate < 5)
            {

                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add((UInt32)Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < (int)SampleCount; i++)
                        { new_signal.Add((UInt32)Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < (int)SampleRate; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                new_signal.Add((UInt32)Y[Y.Count() - 1]);
                new_signal.Add((UInt32)Y[Y.Count() - 1]);
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add((UInt32)0); }
            }
            else if (SampleRate > 768 && SampleRate < 775)
            {
                
                for (int i = 1; i < 96; i++)
                { new_signal.Add((UInt32)0); }
                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add((UInt32)Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < 768; i++)
                        { new_signal.Add((UInt32)Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < 768; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                for (int i = 1; i < 96; i++)
                { new_signal.Add((UInt32)Y[Y.Count() - 1]); }
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add((UInt32)0); }
            }
            else if (SampleRate > 755 && SampleRate < 768)
            {

                for (int i = 1; i < 96; i++)
                { new_signal.Add((UInt32)0); }
                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add((UInt32)Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < 768; i++)
                        { new_signal.Add((UInt32)Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < 768; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                for (int i = 1; i < 96; i++)
                { new_signal.Add((UInt32)Y[Y.Count() - 1]); }
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add((UInt32)0); }
            }
            else if (SampleRate > 370 && SampleRate < 390)
            {
                for (int i = 1; i < 48; i++)
                { new_signal.Add((UInt32)0); }
                for (int a = 0; a < Y.Count() - 1; a++)
                {
                    new_signal.Add((UInt32)Y[a]);
                    if (Y[a] == Y[a + 1])
                    {
                        for (int i = 1; i < 384; i++)
                        { new_signal.Add((UInt32)Y[a]); }
                    }
                    else if (Y[a] != Y[a + 1])
                    {
                        var dis = (Convert.ToDouble(Y[a + 1]) - Convert.ToDouble(Y[a])) / SampleCount;
                        for (int i = 1; i < 384; i++)
                        { new_signal.Add(Convert.ToUInt32(Convert.ToDouble(Y[a]) + (dis * i))); }

                    }
                }
                for (int i = 1; i < 48; i++)
                { new_signal.Add((UInt32)Y[Y.Count() - 1]); }
                for (int i = new_signal.Count; i < SampleCount; i++)
                { new_signal.Add((UInt32)0); }
            }

                return new_signal;
        }

        private void text_file_reader(string file_name, Dictionary<string, List<ValueType>> extractedSamples, Dictionary<string, List<double>> time_dictionatyf,CheckedListBox checkedListBox,Dictionary<string, string> dic)
        {
            try
            {
                
                using (StreamReader read = new StreamReader(file_name))
                {
                    String header = read.ReadLine();
                    string[] parametry = header.Split('\t');
                    List<string> parametry_list = new List<string>(parametry);
                    parametry_list.Remove(parametry_list.Last());
                    foreach (var paramter in parametry_list)
                    {
                        extractedSamples[paramter] = new List<ValueType>();
                    }

                    while(read.Peek() > -1)
                    {
                        String line = read.ReadLine();
                        string[] data_line = line.Split('\t');
                        List<string> data_line_list = new List<string>(data_line);
                        data_line_list.Remove(data_line_list.Last());
                        for ( int i = 0; i < data_line_list.Count(); i++)
                        {
                            extractedSamples[parametry_list[i]].Add(Convert.ToDouble(data_line_list[i]));
                        }
                    }


                }

                var keys2 = new List<string>(extractedSamples.Keys);
                var matchingvalues = keys2.Where(stringToCheck => stringToCheck.Contains("Time_with_occurence_")).ToList();
                dodawanie_checklistbox2(keys2, checkedListBox, extractedSamples);
                //time_dictionatyf[matchingvalues[0].Split('_')[3]] = extractedSamples[matchingvalues[0]];
                foreach (var time_occure in matchingvalues)
                {
                    time_dictionatyf[time_occure.Split('_')[3]] = new List<double>();
                    for (int i = 0; i < extractedSamples[time_occure].Count(); i++)
                    {
                        time_dictionatyf[time_occure.Split('_')[3]].Add((double)extractedSamples[time_occure][i]);
                    }
                }
                
                //dic = keys2.ToDictionary(x => x, x => matchingvalues[0].Split('_')[3]);
                 //dic = keys2.Zip(matchingvalues, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v.Split('_')[3]);
                int iter = 0;
                foreach (var key in keys2)
                {
                    dic[key] = matchingvalues[iter].Split('_')[3];
                    if (key.Contains("Time_with_occurence_"))
                    {
                        iter++;
                    }
                }


            }
            catch { }

        }
        
        private static int packetIndex = 0;
        private static double last_time = 0;
        public static Dictionary<int, DateTime?> LastAcraTime = new Dictionary<int, DateTime?>();
        private void device_OnPacketArrival(object sender, CaptureEventArgs e, LiveConverter converter, Dictionary<string, List<ValueType>> extractedSamples, Dictionary<string, List<double>> time_dictionatyf)
        {
            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet && e.Packet.Data.Count() > 1)
            {
                //var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                //var ethernetPacket = (PacketDotNet.EthernetPacket)packet;


                packetIndex++;

                var result = converter.DecodePacket(e.Packet);
                //result.IsSuccess

                if (LastAcraTime.ContainsKey(result.streamID))
                {
                    if (result.AcraTime == LastAcraTime[result.streamID]) return;
                    else { LastAcraTime[result.streamID] = result.AcraTime; }
                }
                else
                {
                    LastAcraTime[result.streamID] = result.AcraTime ;
                }

                
                temp_exSam = new Dictionary<string, List<ValueType>>();
                if (packetCount == 0)
                {
                    packetCount++;



                    repeat_occurance = new List<int>();
                    parametr_definition = converter.parameterDefinitions;
                    parametr_definition.ForEach(x =>
                    {
                        if (repeat_occurance.Any(b => b == x.Occurrences) == false)
                        {
                            if (occurance.ContainsKey(x.StreamID))
                            {
                                occurance?[x.StreamID].Add(x.Occurrences);
                                repeat_occurance.Add(x.Occurrences);
                            }
                            else
                            {
                                occurance[x.StreamID] = new List<int>() { x.Occurrences };
                                repeat_occurance.Add(x.Occurrences);
                            }

                        }
                        last_times = occurance.Keys.ToDictionary(w => w ,w=> 0.0);


                    });

                }

                    result.Samples.ForEach(s =>
                {
                    if (extractedSamples.ContainsKey(s.Key))
                        extractedSamples?[s.Key].Add(s.Value);


                    else
                        extractedSamples[s.Key] = new List<ValueType>() { s.Value };


                    //if (temp_exSam.ContainsKey(s.Key))
                    //    temp_exSam?[s.Key].Add(s.Value);
                    //else
                    //    temp_exSam[s.Key] = new List<ValueType>() { s.Value };
                });
                // double[] last_times = new double[occurance.Count()];


                try
                {
                    if (last_times != null && last_times[result.streamID] == 0.0)
                    {
                        if (result.AcraTime != null)
                            last_times[result.streamID] = result.AcraTime.Value.TimeOfDay.TotalSeconds;
                    }

                }
                catch { }
                try
                { 
                    if (last_times[result.streamID] != 0.0)
                    {
                        foreach (var time_divide in occurance[result.streamID])
                        {
                            //TimeSpan delta = TimeSpan.FromTicks((timearrayf[timearrayf.Count() - 1].Subtract(timearrayf[timearrayf.Count() - 2]).Ticks) / time_divide);
                            double delta = ( result.AcraTime.Value.TimeOfDay.TotalSeconds - last_times[result.streamID]) / (double)time_divide;
                            if (time_dictionatyf.ContainsKey(time_divide.ToString()))
                            {
                                
                                for (int i = 0; i < time_divide ; i++)
                                {
                                    time_dictionatyf[time_divide.ToString()].Add(time_dictionatyf[time_divide.ToString()].Last() + delta);

                                }
                                
                            }


                            else
                            {
                                time_dictionatyf[time_divide.ToString()] = new List<double>();
                                time_dictionatyf[time_divide.ToString()].Add(delta);
                                for (int i = 0; i < time_divide - 1 ; i++)
                                {
                                    time_dictionatyf[time_divide.ToString()].Add(time_dictionatyf[time_divide.ToString()].Last() + delta);

                                }


                            }
                            
                        }
                        last_times[result.streamID] = result.AcraTime.Value.TimeOfDay.TotalSeconds;
                    }
                }
                catch { }
            
                if (packetIndex % 1000 == 0)
                {

                    try
                    {
                        this.Invoke((MethodInvoker)delegate { Updatelabel(); });
                        //UpdateChart();
                    }
                    catch
                    {
                    }
                }

            }
        }

        public void Updatelabel()
        {
            label3.Text = "Process pacet" + packetIndex.ToString();
            Application.DoEvents();
        }
        public void Updatelabel2()
        {
            label3.Text = "DONE";
            Application.DoEvents();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                list_series = StartChart_file(chart1, checkedListBox1, extractedSamples,false,list_series,indexing);
                Changechartfile(chart1, extractedSamples, time_dictionatyf, dic1, list_series, interpolate_samples);
            }
            catch { }
           if (mrk_1.BackColor == System.Drawing.Color.Lime) { marker_refresh(mrk_lbl_1, 1);}
           if (mrk_2.BackColor == System.Drawing.Color.Lime) { marker_refresh(mrk_lbl_2, 2); }
           if (mrk_3.BackColor == System.Drawing.Color.Lime) { marker_refresh(mrk_lbl_3, 3); }
           if (mrk_4.BackColor == System.Drawing.Color.Lime) { marker_refresh(mrk_lbl_4, 4); }
        }

        public void button_Click()
        {
            list_series = StartChart_file(chart1, checkedListBox1, extractedSamples,false,list_series,indexing);
            Changechartfile(chart1, extractedSamples, time_dictionatyf, dic1,list_series, interpolate_samples);
        }


        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            //double yOffset = GetYOffset(chart1, e.X);
            if (croos_cursor_on)
            {
                try
                {
                    Point mousePoint = new Point(e.X, e.Y);
                    chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, false);
                    chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, false);


                    //chart1.ChartAreas[0].CursorX.SelectionStart

                    xValue = (int) chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                    double yValue = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);

                    var name = list_series[series_iter].ToString().Substring(7);
                    //.Single(s => s.Equals(max));
                    var count = extractedSamples[name].Count;
                    ValueType YVAL;
                    xValue = xValue * Int32.Parse(resamplingFile);
                    if(count < globe_max_count)
                    { YVAL = interpolate_samples[count][name][xValue]; }
                    else
                    {  YVAL = extractedSamples[name][xValue]; }
                    
                    label4.Text = String.Concat(String.Concat(xValue.ToString(), " , "), YVAL.ToString(),"\n #", (series_iter + 1).ToString());
                    label4.Location = new Point(10, e.Y + 40);
                }
                catch
                {
                }
            }

            if (mouse_event)
            {
                try
                {
                    Label mrk = new Label();

                    Point mousePoint = new Point(e.X, e.Y);
                    switch (hold_coursor)
                    {
                        case 0:
                            mrk_1.Location = new Point(e.X + 5, 50);
                            mrk_lbl_1.Location = new Point(e.X , 350);
                            lbl_num_mrk1.Location = new Point(e.X, 32);
                            break;
                        case 1:
                            mrk_2.Location = new Point(e.X + 5, 50);
                            mrk_lbl_2.Location = new Point(e.X , 350);
                            lbl_num_mrk2.Location = new Point(e.X , 32);
                            break;
                        case 2:
                            mrk_3.Location = new Point(e.X + 5, 50);
                            mrk_lbl_3.Location = new Point(e.X , 350);
                            lbl_num_mrk3.Location = new Point(e.X , 32);
                            break;
                        case 3:
                            mrk_4.Location = new Point(e.X + 5, 50);
                            mrk_lbl_4.Location = new Point(e.X , 350);
                            lbl_num_mrk4.Location = new Point(e.X , 32);
                            break;
                    }

                    //mrk.Location = new Point(e.X + X_diff, 500);
                }
                catch
                {
                }
            }
        }
        
        private void chart1_AxisViewChanged(object sender, ViewEventArgs e)
        {
            double ymax = chart1.ChartAreas[0].AxisY.ScaleView.ViewMaximum;

            double ymin = chart1.ChartAreas[0].AxisY.ScaleView.ViewMinimum;
            if (axis_changed != 0 && Math.Abs(ymax - axis1) > 0 && Math.Abs(ymin - axis2) > 0)
            {
                //marker_hider(mrk_1, mrk_lbl_1,lbl_num_mrk1);
                //marker_hider(mrk_2, mrk_lbl_2, lbl_num_mrk2);
                //marker_hider(mrk_3, mrk_lbl_3, lbl_num_mrk3);
                //marker_hider(mrk_4, mrk_lbl_4, lbl_num_mrk4);

                int i = found_i((int)(ymax - ymin));

                axismax = round((int)ymax, i);

                axismin = round((int)ymin, i);

                if (ymax != axismax || ymin != axismin)
                { chart1.ChartAreas[0].AxisY.ScaleView.Zoom(axismin, axismax);
                    axis1 = axismax;
                    axis2 = axismin;
                    }

             
                //chart1.ChartAreas[0].AxisY.ScaleView.Zoom(11000, 48000);
            }
            else
            {
                axis_changed++;
                double axis1 = chart1.ChartAreas[0].AxisY.ScaleView.ViewMaximum;
                double axis2 = chart1.ChartAreas[0].AxisY.ScaleView.ViewMinimum;
            }
           
                
          

        }
        private int found_i(int num)
        {
            int wynik = 0, i = 0, m = 0;
            bool dalej = true;
            do
            {
                m = num % 10;
                num = num / 10;
                i++;
                if (num == 0) { dalej = false; break; }
                if (m > 5) { num = num + 1; }
            } while (dalej);
            wynik = m * (int)(Math.Pow(10, i - 1));

            if (i != 0)
                return i - 1;
            else
                return 0;
        }

        private int round(int num, int i)
        {
            int wynik = 0;
            if (i== 0) { num = 1; i = 1; }
            int m = num % (int)(Math.Pow(10, i - 1));
            int temp = num / (int)(Math.Pow(10, i - 1));
            if(m > 5 * (int)(Math.Pow(10, i - 2)))
            wynik = (temp + 1) * (int)(Math.Pow(10, i - 1));
            else
            wynik = (temp ) * (int)(Math.Pow(10, i - 1));

            return wynik;
        }

        private void chart1_AxisViewChanging(object sender, ViewEventArgs e)
        {


        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_Click(object sender, EventArgs e)
        {

            //OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\";
            //openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            //openFileDialog1.FilterIndex = 2;
            //openFileDialog1.RestoreDirectory = true;
            //openFileDialog1.ShowDialog();

            //textBox2.Text = openFileDialog1.FileName;

            FolderBrowserDialog katalog = new FolderBrowserDialog();
            // katalog.SelectedPath = @"C:\Users\pniedbala\Desktop\SSR\test_data\305_509_fulltest\14_509_12102017";
            using (StreamReader sr = new StreamReader("actual_settings\\path.txt"))
            {

                katalog.SelectedPath = sr.ReadLine();
                

            }

            katalog.ShowDialog();
            var kat_sciezka = katalog.SelectedPath.ToString();
            textBox2.Text = kat_sciezka;
            using (StreamWriter sw = File.CreateText("actual_settings\\path.txt"))
            {
                sw.WriteLine(katalog.SelectedPath);
            }

            if (kat_sciezka.Count() > 0)
            {
                textBox2.Text = kat_sciezka;
                DirectoryInfo katalog2 = new DirectoryInfo(kat_sciezka);
                try
                {
                    FileInfo[] pliki_test = katalog2.GetFiles(".cap");
                }

                catch (DirectoryNotFoundException ex)
                {
                    MessageBox.Show("Brak katalogu o podanej nazwie.", "UWAGA!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //this.Close(); //tak mozna zamknac okno
                    return;
                }


                FileInfo[] pliki = katalog2.GetFiles();
                comboBox2.Items.Clear();
                comboBox2.Text = "";
                comboBox2.Items.AddRange(pliki);

            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Wskaż plik", "UWAGA!", MessageBoxButtons.OK);
                return;
            }
            if (textBox2.Text == "")
            {
                warnform2.ShowDialog(); return;

            }
            else
            {
                extractedSamples2 = new Dictionary<string, List<ValueType>>();
                time_dictionatyf2 = new Dictionary<string, List<double>>();
                timearrayf = new List<double>();
                timearray_128f = new List<double>();
                timearray_256f = new List<double>();
                last_times = new Dictionary<int, double>();
                packetIndex = 0;
                packetCount = 0;
                dic2 = new Dictionary<string, string>();
                keys = new List<string>();
                occurance = new Dictionary<int, List<int>>();
                ICaptureDevice device;
                string capFile = textBox2.Text + "\\" + comboBox2.SelectedItem.ToString();
                if (capFile[capFile.Count() - 3] == 'c')
                {
                    try
                    {
                        // Get an offline device
                        device = new CaptureFileReaderDevice(capFile);

                        // Open the device
                        device.Open();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Caught exception when opening file" + ex.ToString());
                        return;
                    }
                    //var converter = Converter.BuildLiveConverter(CaptureForm.aircraftname, CaptureForm.path_configxml, CaptureForm.path_scalingtable, CaptureForm.path_instrumentsetting);
                    var converter = Converter.BuildLiveConverter(CaptureForm.aircraftname, CaptureForm.path_configxml, CaptureForm.path_scalingtable, CaptureForm.path_instrumentsetting);
                    device.OnPacketArrival +=
                    new PacketArrivalEventHandler((_sender, _e) =>
                        device_OnPacketArrival(this, _e, converter, extractedSamples2, time_dictionatyf2));
                    device.Capture();
                    var keys2 = new List<string>(extractedSamples2.Keys);

                    dodawanie_checklistbox2(keys2, checkedListBox2,extractedSamples2);
                    if (dic2.Count == 0)
                    {
                        parametr_definition.ForEach(x => keys.Add(x.ToString()));
                        parametr_definition.ForEach(x => occurence.Add(x.Occurrences.ToString()));
                        //dodawanie chasklistbox;
                        dic2 = keys.Zip(occurence, (k, v) => new { k, v })
                          .ToDictionary(x => x.k, x => x.v);
                    }

                    Dictionary<string, int> dictionary_keys = new Dictionary<string, int>();
                    var keys3 = new List<string>();
                    var occurence2 = new List<int>();
                    extractedSamples2.Keys.ToList().ForEach(x => keys3.Add(x));
                    extractedSamples2.Keys.ToList().ForEach(x => occurence2.Add(extractedSamples2[x].Count));
                    // var min_occure = occurence2.Min();
                    // var occurence3 = occurence2.Select(x => x / min_occure).ToList();
                    var dic3 = keys3.Zip(occurence2, (k, v) => new { k, v })
                          .ToDictionary(x => x.k, x => x.v);

                    var occure2 = dic3.Values.ToList().Distinct().ToList();
                    //var occure2 = ocure.Select(x => x).ToList();
                    occure2.Sort();
                    var occure3 = new List<int>(occure2);
                    occure2.Remove(occure2.Last());
                    occure3.Remove(occure3.First());

                    int i = 0;
                    List<string> parametry = new List<string>();
                    foreach (var num in occure2)
                    {
                        parametry.AddRange(dic3.Where(x => x.Value == num).Select(x => x.Key).ToList());
                        interpolate_samples2[num] = new Dictionary<string, List<ValueType>>();
                        foreach (var par in parametry)
                        {
                            var new_signal = interpolate(extractedSamples2[par], occure3[i]);



                            interpolate_samples2[num][par] = new List<ValueType>(new_signal);

                            //if (interpolate_samples.ContainsKey(num))
                            //{
                            //    if(interpolate_samples[num].ContainsKey(par))
                            //    {

                            //    }
                            //    else
                            //    {

                            //    }

                            //}
                            //else
                            //{

                            //}


                            //if (occurance.ContainsKey(x.StreamID))
                            //{
                            //    occurance?[x.StreamID].Add(x.Occurrences);
                            //    repeat_occurance.Add(x.Occurrences);
                            //}
                            //else
                            //{
                            //    occurance[x.StreamID] = new List<int>() { x.Occurrences };
                            //    repeat_occurance.Add(x.Occurrences);
                            //}
                        }
                        i++;

                    }
                }
                if (capFile[capFile.Count() - 3] == 't')
                    text_file_reader(capFile, extractedSamples2, time_dictionatyf2, checkedListBox2, dic2);
               









            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            list_series2 = StartChart_file(chart2,checkedListBox2,extractedSamples2,true,list_series2,indexing2);
            Changechartfile(chart2, extractedSamples2, time_dictionatyf2, dic2,list_series2, interpolate_samples2);
        }
        public int FileCount = 0;
        private int time;

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            try
            {
                //chart1.Series[keys[i]].IsXValueIndexed = true;
                double max = chart1.ChartAreas[0].AxisX.ScaleView.ViewMaximum - 1;
                double min = chart1.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                var name = chart1.Series[0].ToString().Substring(7);
                extractedPartOfSamples = new Dictionary<string, List<ValueType>>();
                FileCount = 1;
                //var div = dic[name];

                //var strings = seria.ToString().Split('-');
                //string name = strings[1];
                //// if (strings.Count() == 2) { name = strings[1]; }
                //// else if (strings.Count() == 3) { name = strings[1] + '-' + strings[2]; }
                //// else if (strings.Count() == 4) { name = strings[1].Remove(strings[1].Length - 1); }

                //var chartarray = extractedSamples[seria.ToString().Substring(7)];
                //int iter = 0;
                //string time_divide = "";
                //try { time_divide = dic[name]; }
                //catch
                //{
                //    try { time_divide = dic[name.remove(strings[1].length - 1)]; }
                //    catch
                //    {
                //        try { time_divide = dic[strings[1] + '-' + strings[2].remove(strings[2].length - 1)]; }
                //        catch { time_divide = dic[strings[1] + '-' + strings[2]]; }
                //    }


                
                var list_series = chart1.Series.ToList();
                int series_max_count = 0 ;
                foreach (var series in list_series)
                {
                    name = series.ToString().Substring(7);
                    var count = extractedSamples[name].Count;
                    if (count > series_max_count) { series_max_count = count; }
                }


                    int max_it =0, min_it= 0;
                foreach (var series in list_series)
                {
                    name = series.ToString().Substring(7);
                    var count = extractedSamples[name].Count();
                    if(globe_max_count > count)
                    {
                        min_it = (int)Math.Round((min * count) / series_max_count);
                        max_it = (int)Math.Round((max * count) / series_max_count);
                        if(max_it > count) { max_it = count; }
                    }
                    else
                    {
                        min_it = (int)Math.Round(min);
                        max_it = (int)Math.Round(max);
                        if (max_it > count) { max_it = count; }
                    }

                    for (int i = min_it; i < max_it; i++)
                    {
                        //extractedPartOfSamples.Add(extractedSamples[list_series[0].ToString().Substring(7)][i]);



                        if (extractedPartOfSamples.ContainsKey(name))
                            extractedPartOfSamples?[name].Add(extractedSamples[name][i]);
                        else
                            extractedPartOfSamples[name] = new List<ValueType>() { extractedSamples[name][i] };


                    }
                
                    var strings = series.ToString().Split('-');
                     string name2 = strings[1];
                    string time_divide = "";
                    try { time_divide = dic1[name2]; }
                    catch
                    {
                        try { time_divide = dic1[name2.Remove(strings[1].Length - 1)]; }
                        catch
                        {
                            try { time_divide = dic1[strings[1] + '-' + strings[2].Remove(strings[2].Length - 1)]; }
                            catch { time_divide = dic1[strings[1] + '-' + strings[2]]; }
                        }
                    }
                    if (!extractedPartOfSamples.Keys.Contains("Time_with_occurence_" + time_divide))
                    {
                        extractedPartOfSamples["Time_with_occurence_" + time_divide] = new List<ValueType>();
                        for (int it = min_it; it < max_it; it++)
                        {
                            extractedPartOfSamples["Time_with_occurence_" + time_divide].Add(time_dictionatyf[time_divide][it]);
                        }
                    }

                }

                extractedPartOfSamples.OrderByDescending(x => x.Value.Count).ToDictionary(x => x.Key, x => x.Value);

                string headerLine = "";
                foreach (string key in extractedPartOfSamples.Keys)
                {
                    headerLine = headerLine + key + "\t";
                }
                
                int len = series_max_count;
                //FileCount++;
                //string dumpTextPath = "C:\\Users\\pniedbala\\Desktop\\test_data\\509_valid_file1\\data_read2.tsv";
                var directory =CaptureForm.path_savetsv;
                var name_file = directory + "\\" + comboBox1.Text.Split('.')[0] + "_" + System.DateTime.Today.ToString("MM-dd-yyyy") + "_file_1.tsv";
                while (File.Exists(name_file))
                {
                    FileCount++;
                    name_file = directory + "\\" + comboBox1.Text.Split('.')[0] + "_" + System.DateTime.Today.ToString("MM-dd-yyyy") + "_file_" + FileCount + ".tsv";
                }

               

                
             

                //using (StreamWriter sw = File.CreateText(name_file))
                //{
                //    sw.WriteLine(headerLine);
                //    for (int i = 0; i < len; i++)
                //    {
                //        string valuesLine = "";
                //        foreach (string key in extractedPartOfSamples.Keys)
                //        {
                //            valuesLine = valuesLine + extractedPartOfSamples[key][i] + "\t";
                //        }
                //        //time_dictionatyf[time_divide][i]
                //        sw.WriteLine(valuesLine);
                //    }
                //}
                /////wklejone z saveall//////
              
                StringBuilder stringbuilder = new StringBuilder();
                stringbuilder.Append(headerLine + "\n");
                var keys = extractedPartOfSamples.Keys.ToList();
                for (int i = 1; i < len; i++)
                {
                    foreach (string key in keys)
                    {
                        try
                        {
                            stringbuilder.Append(extractedPartOfSamples[key][i].ToString() + "\t");
                        }
                        catch
                        {
                            keys.Remove(key);
                            break;
                        }

                    }
                    stringbuilder.Append("\n");
                }

                File.AppendAllText(name_file, stringbuilder.ToString());

                ////////////////



                MessageBox.Show("Save Complited");
            }
            catch { MessageBox.Show("Error. Save Cancelled"); }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (chart1.Legends[0].Enabled) { chart1.Legends[0].Enabled = false; }
            else { chart1.Legends[0].Enabled = true; }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            var axis_set = new Axis_setting(chart1);
            axis_set.Show();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            try
            {
                // var sortedDict = from entry in dic orderby entry.Value ascending select entry;
                dic1 = dic1.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                var time_dictionat_new = new Dictionary<string, List<ValueType>>();


                foreach (var x in time_dictionatyf)
                {
                    foreach (var item in time_dictionatyf[x.Key])
                    {
                        var it = (ValueType)item;

                        if (time_dictionat_new.ContainsKey("Time_with_occurence_" + x.Key))
                            time_dictionat_new?["Time_with_occurence_" + x.Key].Add(it);
                        else
                            time_dictionat_new["Time_with_occurence_" + x.Key] = new List<ValueType>() { it };
                    }
                }


                Dictionary<string, List<ValueType>> extractedSamplesWithTime = new Dictionary<string, List<ValueType>>();
                if (comboBox1.Text.Split('.')[1] == "cap")
                { extractedSamplesWithTime = extractedSamples.Concat(time_dictionat_new).ToDictionary(x => x.Key, x => x.Value); }
                else
                { extractedSamplesWithTime = extractedSamples; }
                var extractedSamplesSorted = extractedSamplesWithTime.OrderByDescending(x => x.Value.Count).ToDictionary(x => x.Key, x => x.Value);

                string headerLine = "";
                foreach (string key in extractedSamplesSorted.Keys)
                {
                    headerLine = headerLine + key + "\t";
                }
                var keys_test = new List<string>(extractedSamplesSorted.Keys);
                var test = keys_test[0];
                int len = extractedSamplesSorted[test].Count;
                FileCount++;
                //string dumpTextPath = "C:\\Users\\pniedbala\\Desktop\\test_data\\509_valid_file1\\data_read2.tsv";
                var directory = CaptureForm.path_savetsv;
                var name_file = directory + "\\" + comboBox1.Text.Split('.')[0] + '_' + System.DateTime.Today.ToString("MM-dd-yyyy") + "_allParamsfile" + FileCount.ToString() + ".tsv";


                StringBuilder stringbuilder = new StringBuilder();
                stringbuilder.Append(headerLine + "\n");
                var keys = extractedSamplesSorted.Keys.ToList();
                for (int i = 1; i < len; i++)
                {
                    foreach (string key in keys)
                    {
                        try
                        {
                            stringbuilder.Append(extractedSamplesSorted[key][i].ToString() + "\t");
                        }
                        catch
                        {
                            keys.Remove(key);
                            break;
                        }

                    }
                    stringbuilder.Append("\n");
                }

                File.AppendAllText(name_file, stringbuilder.ToString());


                MessageBox.Show("Save Complited");

            }
            catch { }
           
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            var min_max_marker = new Min_max_setting(this);
            min_max_marker.Show();
            

           // Marker_max_generator(max_name);
            //chart1.ChartAreas[0].CursorX.SetCursorPosition()
            //chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, false);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            series_iter++;
            if (series_iter == list_series.Count()) { series_iter = 0; }
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            while (chart1.ChartAreas[0].AxisY.ScaleView.IsZoomed || chart1.ChartAreas[0].AxisX.ScaleView.IsZoomed)
            {
                chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset();
                chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            }
        }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripContainer1_TopToolStripPanel_Click(object sender, EventArgs e)
        {

        }
        //save 2
        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            try
            {
                //chart1.Series[keys[i]].IsXValueIndexed = true;
                double max = chart2.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
                double min = chart2.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                var name = chart2.Series[0].ToString().Substring(7);
                extractedPartOfSamples2 = new Dictionary<string, List<ValueType>>();
                FileCount = 1;
                //var div = dic[name];

                //var strings = seria.ToString().Split('-');
                //string name = strings[1];
                //// if (strings.Count() == 2) { name = strings[1]; }
                //// else if (strings.Count() == 3) { name = strings[1] + '-' + strings[2]; }
                //// else if (strings.Count() == 4) { name = strings[1].Remove(strings[1].Length - 1); }

                //var chartarray = extractedSamples[seria.ToString().Substring(7)];
                //int iter = 0;
                //string time_divide = "";
                //try { time_divide = dic[name]; }
                //catch
                //{
                //    try { time_divide = dic[name.remove(strings[1].length - 1)]; }
                //    catch
                //    {
                //        try { time_divide = dic[strings[1] + '-' + strings[2].remove(strings[2].length - 1)]; }
                //        catch { time_divide = dic[strings[1] + '-' + strings[2]]; }
                //    }


                var list_series = chart2.Series.ToList();
                int series_max_count = 0;
                foreach (var series in list_series)
                {
                    name = series.ToString().Substring(7);
                    var count = extractedSamples2[name].Count;
                    if (count > series_max_count) { series_max_count = count; }
                }


                int max_it = 0, min_it = 0;
                foreach (var series in list_series)
                {
                    name = series.ToString().Substring(7);
                    var count = extractedSamples2[name].Count();
                    if (series_max_count > count)
                    {
                        min_it = (int)Math.Round((min * count) / series_max_count);
                        max_it = (int)Math.Round((max * count) / series_max_count);
                        if (max_it > count) { max_it = count; }
                    }
                    else
                    {
                        min_it = (int)Math.Round(min);
                        max_it = (int)Math.Round(max);
                        if (max_it > count) { max_it = count; }
                    }

                    for (int i = min_it; i < max_it; i++)
                    {
                        //extractedPartOfSamples.Add(extractedSamples[list_series[0].ToString().Substring(7)][i]);



                        if (extractedPartOfSamples2.ContainsKey(name))
                            extractedPartOfSamples2?[name].Add(extractedSamples2[name][i]);
                        else
                            extractedPartOfSamples2[name] = new List<ValueType>() { extractedSamples2[name][i] };


                    }

                    var strings = series.ToString().Split('-');
                    string name2 = strings[1];
                    string time_divide = "";
                    try { time_divide = dic2[name2]; }
                    catch
                    {
                        try { time_divide = dic2[name2.Remove(strings[1].Length - 1)]; }
                        catch
                        {
                            try { time_divide = dic2[strings[1] + '-' + strings[2].Remove(strings[2].Length - 1)]; }
                            catch { time_divide = dic2[strings[1] + '-' + strings[2]]; }
                        }
                    }
                    if (!extractedPartOfSamples2.Keys.Contains("Time_with_occurence_" + time_divide))
                    {
                        extractedPartOfSamples2["Time_with_occurence_" + time_divide] = new List<ValueType>();
                        for (int it = min_it; it < max_it; it++)
                        {
                            extractedPartOfSamples2["Time_with_occurence_" + time_divide].Add(time_dictionatyf2[time_divide][it]);
                        }
                    }

                }
                

              



                extractedPartOfSamples2.OrderByDescending(x => x.Value.Count).ToDictionary(x => x.Key, x => x.Value);

                string headerLine = "";
                foreach (string key in extractedPartOfSamples2.Keys)
                {
                    headerLine = headerLine + key + "\t";
                }

                int len = series_max_count;
                //FileCount++;
                //string dumpTextPath = "C:\\Users\\pniedbala\\Desktop\\test_data\\509_valid_file1\\data_read2.tsv";
                var directory = CaptureForm.path_savetsv;
                var name_file = directory + "\\" + comboBox2.Text.Split('.')[0] + "_" + System.DateTime.Today.ToString("MM-dd-yyyy") + "_file_1.tsv";
                while (File.Exists(name_file))
                {
                    FileCount++;
                    name_file = directory + "\\" + comboBox2.Text.Split('.')[0] + "_" + System.DateTime.Today.ToString("MM-dd-yyyy") + "_file_" + FileCount + ".tsv";
                }






                //using (StreamWriter sw = File.CreateText(name_file))
                //{
                //    sw.WriteLine(headerLine);
                //    for (int i = 0; i < len; i++)
                //    {
                //        string valuesLine = "";
                //        foreach (string key in extractedPartOfSamples.Keys)
                //        {
                //            valuesLine = valuesLine + extractedPartOfSamples[key][i] + "\t";
                //        }
                //        //time_dictionatyf[time_divide][i]
                //        sw.WriteLine(valuesLine);
                //    }
                //}
                /////wklejone z saveall//////

                StringBuilder stringbuilder = new StringBuilder();
                stringbuilder.Append(headerLine + "\n");
                var keys = extractedPartOfSamples2.Keys.ToList();
                for (int i = 1; i < len; i++)
                {
                    foreach (string key in keys)
                    {
                        try
                        {
                            stringbuilder.Append(extractedPartOfSamples2[key][i].ToString() + "\t");
                        }
                        catch
                        {
                            keys.Remove(key);
                            break;
                        }

                    }
                    stringbuilder.Append("\n");
                }

                File.AppendAllText(name_file, stringbuilder.ToString());

                ////////////////



                MessageBox.Show("Save Complited");
            }
            catch { MessageBox.Show("Error. Save Cancelled"); }
        
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
        int mrk_counter = 0;
        public double[,] Yvalues = new double[4,3];
        public int[] Xvalues = new int[4];
        public string[] lbl_texts = new string[4];
        private void marker_genrator(Label mrk_1,Label mrk_lbl_1, Label lbl_num_mrk, int X, int mrk_count)
        {
            try
            {
                //Label lbl = new System.Windows.Forms.Label();
                //mrk_1.Name = "marker" + mrk_counter.ToString();
                Xvalues[mrk_count - 1] = X;
                mrk_1.Text = "";
                mrk_1.BackColor = System.Drawing.Color.Lime;
                mrk_1.Location = new System.Drawing.Point(X, 50);

                mrk_1.Size = new System.Drawing.Size(1, 320);
                mrk_1.TabIndex = 0;


                xValue = (int) chart1.ChartAreas[0].AxisX.PixelPositionToValue(X);
                xValue = xValue * Int32.Parse(resamplingFile);
                // double yValue = chart1.ChartAreas[0].AxisY.PixelPositionToValue(Y);
                int iter = 0;
                string lbl_text = "";
                var series = new List<string>();
                try
                {
                    series = MarkerSeries[mrk_count - 1];
                }
                catch
                {
                    series = null;
                }

                var name = "";
                while (iter < 3)
                {
                    try
                    {

                        if (series == null)
                        {
                            name = list_series[iter].ToString().Substring(7);
                            //.Single(s => s.Equals(max));

                        }
                        else
                        {

                            name = series[iter].ToString().Substring(7);

                        }
                        var count = extractedSamples[name].Count;
                        if (count == globe_max_count)
                        { Yvalues[mrk_count - 1, iter] = Convert.ToDouble(extractedSamples[name][xValue]); }
                        else
                        { Yvalues[mrk_count - 1, iter] = Convert.ToInt32(interpolate_samples[count][name][xValue]); }

                        lbl_text = lbl_text + Yvalues[mrk_count - 1, iter].ToString() + "\n";

                    }
                    catch
                    {
                        if (mrk_series[mrk_count - 1] == null) break;

                    }

                    iter++;

                }

                lbl_texts[mrk_count - 1] = lbl_text;
                mrk_lbl_1.Text = lbl_text;
                mrk_lbl_1.Location = new Point(X, 350);
                mrk_lbl_1.Size = new System.Drawing.Size(60, 40);
                lbl_num_mrk.Text = '#' + mrk_count.ToString();
                lbl_num_mrk.Location = new Point(X, 32);
                //this.Controls.Add(lbl);
                Application.DoEvents();
                //this.ResumeLayout(false);
                //this.PerformLayout();
            }
            catch
            {
            }
        }

        private void marker_hider(Label mrk_1, Label mrk_lbl_1, Label lbl_num_mrk)
        {
            //Label lbl = new System.Windows.Forms.Label();
            //mrk_1.Name = "marker" + mrk_counter.ToString();
            mrk_1.Text = "";
            mrk_1.BackColor = System.Drawing.SystemColors.Control;
            mrk_1.Location = new System.Drawing.Point(1074, 8);

            mrk_1.Size = new System.Drawing.Size(1, 1);
            mrk_1.TabIndex = 0;
            
            mrk_lbl_1.Text = "";
            mrk_lbl_1.Location = new Point(1074,8);
            mrk_lbl_1.Size = new System.Drawing.Size(1, 1);

            lbl_num_mrk.Text = "";
            lbl_num_mrk.Location = new Point(1074, 8);
            //this.Controls.Add(lbl);
            Application.DoEvents();
            //this.ResumeLayout(false);
            //this.PerformLayout();
        }
        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!mouse_event)
            {
                for (int x = 0; x < 4; x++)
                {
                    if ((e.X <= ( Xvalues[x] + 10)) && (e.X  >= (Xvalues[x] - 15)))
                    {
                        mouse_event = true;
                        hold_coursor = x;
                        X_diff = e.X - Xvalues[x];
                    }
                }
            }
            else
            {
                mouse_event = false;
                Xvalues[hold_coursor] = e.X ;


                xValue = (int)chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X + X_diff);
                // double yValue = chart1.ChartAreas[0].AxisY.PixelPositionToValue(Y);
                int iter = 0;
                string lbl_text = "";
                var series = new List<string>();
                try
                {
                    series = MarkerSeries[hold_coursor];
                }
                catch
                { 
                    series = null;
                }
                var name = "";
                while (iter < 3)
                {
                    try
                    {

                        if (series == null)
                        {
                            name = list_series[iter].ToString().Substring(7);
                            //.Single(s => s.Equals(max));

                        }
                        else
                        {

                            name = series[iter].ToString().Substring(7);

                        }
                        xValue = xValue * Int32.Parse(resamplingFile);
                        var count = extractedSamples[name].Count;
                        if (count == globe_max_count)
                        { Yvalues[hold_coursor, iter] = Convert.ToDouble(extractedSamples[name][xValue]); }
                        else
                        { Yvalues[hold_coursor, iter] = Convert.ToDouble(interpolate_samples[count][name][xValue]); }


                        //Yvalues[hold_coursor, iter] = Convert.ToDouble(extractedSamples[name][xValue]);
                        lbl_text = lbl_text + Yvalues[hold_coursor , iter].ToString() + "\n";

                    }
                    catch
                    {
                        if (mrk_series[hold_coursor] == null) break;

                    }
                    iter++;

                }
                lbl_texts[hold_coursor] = lbl_text;
                Label mrk_lbl = new Label();
                var lbl_num_mrk = new Label();

                switch (hold_coursor)
                {
                    case 0: mrk_lbl = mrk_lbl_1;
                        lbl_num_mrk = lbl_num_mrk1; break;
                    case 1: mrk_lbl = mrk_lbl_2; lbl_num_mrk = lbl_num_mrk2; break;
                    case 2: mrk_lbl = mrk_lbl_3; lbl_num_mrk = lbl_num_mrk3; break;
                    case 3: mrk_lbl = mrk_lbl_4; lbl_num_mrk = lbl_num_mrk4; break;
                }


                mrk_lbl.Text = lbl_text;
                
                mrk_lbl.Size = new System.Drawing.Size(60, 40);
                lbl_num_mrk.Text = '#' + (hold_coursor + 1).ToString();
               
                //this.Controls.Add(lbl);
                Application.DoEvents();





            }


            
        }

      

        private void marker_shower(Label mrk_1, Label mrk_lbl_1, Label lbl_num_mrk, int num)
        {


            mrk_1.Text = "";
            mrk_1.BackColor = System.Drawing.Color.Lime;
            mrk_1.Location = new System.Drawing.Point(Xvalues[num], 50);

            mrk_1.Size = new System.Drawing.Size(1, 320);
            mrk_1.TabIndex = 0;


            xValue = (int)chart1.ChartAreas[0].AxisX.PixelPositionToValue(Xvalues[num]);
            //double yValue = chart1.ChartAreas[0].AxisY.PixelPositionToValue(Y);

            var name = list_series[series_iter].ToString().Substring(7);
            //.Single(s => s.Equals(max));
            //var YVAL = extractedSamples[name][xValue];
            mrk_lbl_1.Text = lbl_texts[num];
            mrk_lbl_1.Location = new Point(Xvalues[num], 350);
            mrk_lbl_1.Size = new System.Drawing.Size(60, 40);
            lbl_num_mrk.Text = '#' + (num + 1).ToString();
            lbl_num_mrk.Location = new Point(Xvalues[num], 32);
            //this.Controls.Add(lbl);
            Application.DoEvents();



        }

        public void marker_refresh( Label mrk_lbl_1,int mrk_count)
        {
            var X = Xvalues[mrk_count - 1];
            xValue = (int)chart1.ChartAreas[0].AxisX.PixelPositionToValue(X);
            // double yValue = chart1.ChartAreas[0].AxisY.PixelPositionToValue(Y);
            int iter = 0;
            string lbl_text = "";
            var series = new List<string>();
            try
            {
                series = MarkerSeries[mrk_count - 1];
            }
            catch
            {
                series = null;
            }

            var name = "";
            while (iter < 3)
            {
                try
                {

                    if (series == null)
                    {
                        name = list_series[iter].ToString().Substring(7);
                        //.Single(s => s.Equals(max));

                    }
                    else
                    {

                        name = series[iter].ToString().Substring(7);

                    }
                    var count = extractedSamples[name].Count;
                    if (count == globe_max_count)
                    { Yvalues[mrk_count - 1, iter] = Convert.ToDouble(extractedSamples[name][xValue]); }
                    else
                    { Yvalues[mrk_count - 1, iter] = Convert.ToDouble(interpolate_samples[count][name][xValue]); }

                    //Yvalues[mrk_count - 1, iter] = Convert.ToDouble(extractedSamples[name][xValue]);
                    lbl_text = lbl_text + Yvalues[mrk_count - 1, iter].ToString() + "\n";

                }
                catch
                {
                    if (mrk_series[mrk_count - 1] == null) break;

                }

                iter++;

            }

            lbl_texts[mrk_count - 1] = lbl_text;
            mrk_lbl_1.Text = lbl_text;
            //mrk_lbl_1.Location = new Point(X, 350);
            //mrk_lbl_1.Size = new System.Drawing.Size(60, 40);
            //lbl_num_mrk.Text = '#' + mrk_count.ToString();
            //lbl_num_mrk.Location = new Point(X, 32);
            //this.Controls.Add(lbl);
            Application.DoEvents();
        }
        private bool m1_press = false;
        private bool m2_press = false;
        private bool m3_press = false;
        private bool m4_press = false;
        internal static bool find_max;
        internal static string max_name;
        private bool find_min;
        public List<double> timebase;
        private int  series_num = 1;
        private int probkowanieNew;
        internal static string resamplingFile = "1";
        internal static bool everyFile = true;
        internal static bool dyFile = false;

        private bool preser(Label mrk,Label mrk_lbl,Label lbl_num_mrk,  bool m_press, int num)
        {
            if (!m_press)
            {

                try
                {
                    this.Invoke((MethodInvoker)delegate { marker_hider(mrk, mrk_lbl, lbl_num_mrk); });
                    //UpdateChart();
                }
                catch
                {
                }
            }

            else
            {
                try
                {
                    this.Invoke((MethodInvoker)delegate { marker_shower(mrk, mrk_lbl, lbl_num_mrk,num); });
                    //UpdateChart();
                }
                catch
                {
                }
            }

            return !m_press;


        }

   

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            m1_press = preser(mrk_1, mrk_lbl_1, lbl_num_mrk1, m1_press, 0);
            
        }

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            m2_press= preser(mrk_2, mrk_lbl_2, lbl_num_mrk2, m2_press,1);

        }

        private void toolStripButton19_Click(object sender, EventArgs e)
        {
            m3_press =  preser(mrk_3, mrk_lbl_3, lbl_num_mrk3, m3_press,2);
        }

        private void toolStripButton20_Click(object sender, EventArgs e)
        {
            m4_press = preser(mrk_4, mrk_lbl_4, lbl_num_mrk4,m4_press, 3);
        }

        private void toolStripButton21_Click(object sender, EventArgs e)
        {
            var marker_setings_form = new Marker_Setting(this);
           marker_setings_form.Show();
        }

        private void chart1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.X > 140 && e.Y < 325)
            {
                var mrk = new Label();
                var mrk_lbl = new Label();
                var lbl_num_mrk = new Label();
                var btn = new ToolStripButton();
                switch (mrk_counter)
                {
                    case 1: mrk = mrk_1; mrk_lbl = mrk_lbl_1; lbl_num_mrk = lbl_num_mrk1; btn = M1_btn; break;
                    case 2: mrk = mrk_2; mrk_lbl = mrk_lbl_2; lbl_num_mrk = lbl_num_mrk2; btn = M2_btn; break;
                    case 3: mrk = mrk_3; mrk_lbl = mrk_lbl_3; lbl_num_mrk = lbl_num_mrk3; btn = M3_btn; break;
                    case 4: mrk = mrk_4; mrk_lbl = mrk_lbl_4; lbl_num_mrk = lbl_num_mrk4; btn = M4_btn; break;
                    default:
                        {
                            mrk_counter = 1;
                            mrk = mrk_1;
                            mrk_lbl = mrk_lbl_1;
                            lbl_num_mrk = lbl_num_mrk1;
                            btn = M1_btn;
                            break;
                        }
                }

                var X = e.X;
                var Y = e.Y;
                M1_btn.BackColor = SystemColors.Menu;
                M2_btn.BackColor = SystemColors.Menu;
                M3_btn.BackColor = SystemColors.Menu;
                M4_btn.BackColor = SystemColors.Menu;
                btn.BackColor = Color.DarkSeaGreen;
                try
                {
                    this.Invoke((MethodInvoker)delegate { marker_genrator(mrk, mrk_lbl, lbl_num_mrk, X, mrk_counter); });
                    //UpdateChart();
                }
                catch
                {
                }
                mrk_counter++;
            }
        }

       
        public void Marker_max_generator(string name)
        {
            //MessageBox.Show("Jestes tu");
            var mrk = new Label();
            var mrk_lbl = new Label();
            var lbl_num_mrk = new Label();
            switch (mrk_counter)
            {
                case 1: mrk = mrk_1; mrk_lbl = mrk_lbl_1; lbl_num_mrk = lbl_num_mrk1; break;
                case 2: mrk = mrk_2; mrk_lbl = mrk_lbl_2; lbl_num_mrk = lbl_num_mrk2; break;
                case 3: mrk = mrk_3; mrk_lbl = mrk_lbl_3; lbl_num_mrk = lbl_num_mrk3; break;
                case 4: mrk = mrk_4; mrk_lbl = mrk_lbl_4; lbl_num_mrk = lbl_num_mrk4; break;
                default:
                    {
                        mrk_counter = 1;
                        mrk = mrk_1;
                        mrk_lbl = mrk_lbl_1;
                        lbl_num_mrk = lbl_num_mrk1;
                        break;
                    }
            }
            var count = extractedSamples[name].Count;
            ValueType max;
            int max_index;
            if (count == globe_max_count)
            { max = extractedSamples[name].Max(); max_index = extractedSamples[name].ToList().IndexOf(max); }
            else
            {
                max = interpolate_samples[count][name].Max();
                max_index = interpolate_samples[count][name].ToList().IndexOf(max); }


                //Yvalues[mrk_count - 1, iter] = Convert.ToDouble(interpolate_samples[count][name][xValue]); }

            
            
            var X = (int)chart1.ChartAreas[0].AxisX.ValueToPixelPosition(max_index) + 2;
            marker_genrator(mrk, mrk_lbl,lbl_num_mrk, X, mrk_counter);
        }

        public void Marker_min_generator(string name)
        {
            //MessageBox.Show("Jestes tu");
            var mrk = new Label();
            var mrk_lbl = new Label();
            var lbl_num_mrk = new Label();
            switch (mrk_counter)
            {
                case 1: mrk = mrk_1; mrk_lbl = mrk_lbl_1; lbl_num_mrk = lbl_num_mrk1; break;
                case 2: mrk = mrk_2; mrk_lbl = mrk_lbl_2; lbl_num_mrk = lbl_num_mrk2; break;
                case 3: mrk = mrk_3; mrk_lbl = mrk_lbl_3; lbl_num_mrk = lbl_num_mrk3; break;
                case 4: mrk = mrk_4; mrk_lbl = mrk_lbl_4; lbl_num_mrk = lbl_num_mrk4; break;
                default:
                    {
                        mrk_counter = 1;
                        mrk = mrk_1;
                        mrk_lbl = mrk_lbl_1;
                        lbl_num_mrk = lbl_num_mrk1;
                        break;
                    }
            }
            var count = extractedSamples[name].Count;
            ValueType min;
            int min_index;
            if (count == globe_max_count)
            { min = extractedSamples[name].Max(); min_index = extractedSamples[name].ToList().IndexOf(min); }
            else
            { min = interpolate_samples[count][name].Max(); min_index = interpolate_samples[count][name].ToList().IndexOf(min); }

            //var min = extractedSamples[name].Min();
            //var min_index = extractedSamples[name].ToList().IndexOf(min);
            var X = (int)chart1.ChartAreas[0].AxisX.ValueToPixelPosition(min_index) + 1;
            marker_genrator(mrk, mrk_lbl,lbl_num_mrk, X, mrk_counter);
        }

        private void toolStripButton22_Click(object sender, EventArgs e)
        {
            var dtime_fome = new Time_delta_form(this);
            dtime_fome.Show();
        }
        //legenda 2
        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            if (chart2.Legends[0].Enabled) { chart2.Legends[0].Enabled = false; }
            else { chart2.Legends[0].Enabled = true; }
        }
        //axis2
        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            var axis_set = new Axis_setting(chart2);
            axis_set.Show();
        }
        //resize2
        private void toolStripButton16_Click(object sender, EventArgs e)
        {
            while (chart2.ChartAreas[0].AxisY.ScaleView.IsZoomed || chart2.ChartAreas[0].AxisX.ScaleView.IsZoomed)
            {
                chart2.ChartAreas[0].AxisY.ScaleView.ZoomReset();
                chart2.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            }
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            if (croos_cursor_on)
            {
                
                chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(new Point(0,0), true);
                chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(new Point(0,0), true);
                chart1.ChartAreas[0].CursorX.IsUserEnabled = false;
            }
            else
            { chart1.ChartAreas[0].CursorX.IsUserEnabled = true; }
            croos_cursor_on = !croos_cursor_on;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            var ImpSet= new ImportSetting();
            ImpSet.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var Signalform =new  SiganalCalulator(this);
            Signalform.Show();
        }

        private void funkcja_testowa()
        {
            int i = 2;
            int w = 3;
            var wynik = i + w;
        }

        private void toolStripButton17_Click_1(object sender, EventArgs e)
        {
            if (indexing) { toolStripButton17.BackColor = SystemColors.ControlDark; }
            else { toolStripButton17.BackColor = SystemColors.Menu; }

            indexing = !indexing;
        }

        private void toolStripButton18_Click_1(object sender, EventArgs e)
        {
            if (indexing2) { toolStripButton18.BackColor = SystemColors.ControlDark; }
            else { toolStripButton18.BackColor = SystemColors.Menu; }

            indexing2 = !indexing2;
        }
    }
    }

