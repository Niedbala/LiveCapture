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


namespace WinformsExample
{
    public partial class Chart_form : Form
    {
        private double[] ChartArray = new double[1000];
        private bool stop_chart = false;
        private Set_chart_form set_chart_form = new Set_chart_form();
        private Thread Time;
        public static int chart_width = 8000;
        public static bool show_last_value = false;
        public static bool show_legend = false;
        Warning_Form warnform = new Warning_Form();
        Warningform2 warnform2 = new Warningform2();
        public static int view_point = 0;
        public static Dictionary<string, List<ValueType>> extractedSamples = new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, List<ValueType>> temp_exSam = new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, string> dic = new Dictionary<string, string>();
        public static List<string> keys = new List<string>();
        public static List<string> occurence = new List<string>();
        // public static List<DateTime> timearray_128f = new List<DateTime>();
        // public static List<DateTime> timearray_256f = new List<DateTime>();
        public static List<double> timearrayf = new List<double>();
        public static List<double> timearray_128f = new List<double>();
        public static List<double> timearray_256f = new List<double>();
        public static Dictionary<string, List<double>> time_dictionatyf = new Dictionary<string, List<double>>();
        public static string grid = "1000";
        private double axismax;
        private double axismin;
        private int axis_changed = 0;
        private double axis1 = 0.0;
        private double axis2 = 0.0;
        private int packetCount;
        public static List<ParameterDefinition> parametr_definition = new List<ParameterDefinition>();
        public static List<int> repeat_occurance = new List<int>();
        public static Dictionary<int, List<int>> occurance = new Dictionary<int, List<int>>();
        public Chart_form()
        {
            InitializeComponent();
            var sample = CaptureForm.extractedSamples;
            var parameter_def = CaptureForm.parametr_definition;
            if (dic.Count == 0)
            {
                parameter_def.ForEach(x => keys.Add(x.ToString()));
                parameter_def.ForEach(x => occurence.Add(x.Occurrences.ToString()));
                //dodawanie chasklistbox;
                dic = keys.Zip(occurence, (k, v) => new { k, v })
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
                name_list.Add(name);
                checkedListBox.Items.Insert(0, name);
            }



        }

        private void dodawanie_checklistbox2(List<string> keys, CheckedListBox checkedListBox)
        {
            checkedListBox.Items.Clear();

            List<string> name_list = new List<string>();

            for (int i = keys.Count() - 1; i >= 0; i--)
            {
                string[] split_key = keys[i].ToString().Split('_');
                var name = "(" + extractedSamples[keys[i]].Count.ToString() + ")" + split_key[split_key.Count() - 1];

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
                for (int i = 0; i <= co_eksportowac1.Count() - 1; i++)
                    if (co_eksportowac1[i])
                    {
                        var probkowanie_key = 1;
                        try
                        {
                            probkowanie_key = Int32.Parse(dic[keys[i]]);//.Split(')').First().Split('(').Last();
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
                        chart1.Series[keys[i]].IsXValueIndexed = true;
                        chart1.Series[keys[i]].SmartLabelStyle.Enabled = false;
                        //chart1.Series[keys[i]].IsValueShownAsLabel = true;
                    }

                chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
                chart1.ChartAreas[0].AxisY.LabelStyle.Format = "N2";
                probkowanie = 0;
                for (int i = 0; i <= co_eksportowac2.Count() - 1; i++)
                    if (co_eksportowac2[i])
                    {
                        var probkowanie_key = Int32.Parse(dic[keys[i]]);//.Split(')').First().Split('(').Last();
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

                Time = new Thread(new ThreadStart(this.TimeHandler));
                Time.IsBackground = true;
                Time.Start();
            }
            catch { }
        }

        private void StartChart_file()
        {

            if (grid == "None") { chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false; }
            else if (grid == "Auto") { chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true; }
            else { chart1.ChartAreas[0].AxisX.MajorGrid.Interval = Int64.Parse(grid); }

            if (grid == "None") { chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = false; }
            else if (grid == "Auto") { chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = true; }
            else { chart2.ChartAreas[0].AxisX.MajorGrid.Interval = Int64.Parse(grid); }

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


            //chart1.Series["Series1"].Points.Clear();
            List<string> keys = new List<string>(extractedSamples.Keys);
            int probkowanie = 0;
            for (int i = 0; i <= co_eksportowac1.Count() - 1; i++)
                if (co_eksportowac1[i])
                {
                    var probkowanie_key = 1;
                    try
                    {
                        probkowanie_key = Int32.Parse(extractedSamples[keys[i]].Count.ToString());//.Split(')').First().Split('(').Last();
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
                    chart1.Series[keys[i]].XValueType = ChartValueType.Double;
                    chart1.Series[keys[i]].IsXValueIndexed = true;
                    chart1.Series[keys[i]].SmartLabelStyle.Enabled = false;
                    chart1.Series[keys[i]].IsVisibleInLegend = show_legend;
                    //chart1.Series[keys[i]].IsValueShownAsLabel = true;
                }
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "N2";
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "N2";

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
               
                Changechart(chart1);
                
                Changechart(chart2);
            }

        }




        public void Changechart(Chart chart)
        {

            var series = chart.Series.ToList();

            List<DateTime> timebase = new List<DateTime>();
            foreach (Series seria in series)
            {
                chart.Series[seria.ToString().Substring(7)].Points.Clear();

                string name = seria.ToString().Split('-')[1];
                var chartarray = CaptureForm.extractedSamples[seria.ToString().Substring(7)];
                int iter = 0;
                if (chartarray.Count() < chart_width)
                { iter = 0; }
                else { iter = chartarray.Count() - chart_width - 1; }
                var time_divide = dic[name];
                //if (time_divide < 3) { timebase = CaptureForm.timearray; }
                // if (time_divide > 120 && time_divide < 130) { timebase = CaptureForm.timearray_128; }
                // if (time_divide > 250 && time_divide < 260) { timebase = CaptureForm.timearray_256; }
                timebase = CaptureForm.time_dictionaty[time_divide];

                var sw = new Stopwatch(); 
                sw.Start();
                for (int i = iter; i < chartarray.Count() - 1; ++i)
                {

                    chart.Series[seria.ToString().Substring(7)].Points.AddXY(timebase[i], chartarray[i]);

                    //chart1.Series[seria.ToString().Substring(7)].XValueType(Data);
                }
                //chart.Series[seria.ToString().Substring(7)].Points.Last().Label = chartarray[chartarray.Count() - 1].ToString();
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds.ToString());
                if (show_last_value)
                {
                    try
                    {
                        int which_point = (view_point * (chartarray.Count() - 10) / 1000) + 1;
                        string[] split_key = seria.ToString().Split('_');
                        chart.Series[seria.ToString().Substring(7)].LegendText = split_key[split_key.Count() - 1] + ";Val:" + Math.Round(chart.Series[seria.ToString().Substring(7)].Points[which_point].YValues[0]).ToString();
                        //chart.Series[seria.ToString().Substring(7)].LegendText = split_key[split_key.Count() - 1] + ";Val:" + Math.Round(chart.Series[seria.ToString().Substring(7)].Points.Last().YValues[0]).ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                //string[] split_key = keys[i].ToString().Split('_')
                //chart.Series[seria.ToString().Substring(7)].Points.Last;
            }



        }
        //kvar sw = new Stopwatch();
        public void Changechartfile(Chart chart)
        {



            var series = chart.Series.ToList();

            //List<DateTime> timebase = new List<DateTime>();
            List<double> timebase = new List<double>();
            foreach (Series seria in series)
            {
                chart.Series[seria.ToString().Substring(7)].Points.Clear();
                string name = seria.ToString().Split('-')[1];

                var chartarray = extractedSamples[seria.ToString().Substring(7)];
                int iter = 0;
                var time_divide = dic[name];
                
                timebase = time_dictionatyf[time_divide];

                //var time_divide = chartarray.Count() / (timearrayf.Count() - 1);
                //if (time_divide < 3) { timebase = timearrayf; }
                //if (time_divide > 120 && time_divide < 130) { timebase = timearray_128f; }
                //if (time_divide > 250 && time_divide < 260) { timebase = timearray_256f; }



                for (int i = iter; i < chartarray.Count() - 1; ++i)
                {

                    chart.Series[seria.ToString().Substring(7)].Points.AddXY(timebase[i], chartarray[i]);

                    //chart1.Series[seria.ToString().Substring(7)].XValueType(Data);
                }
                //chart.Series[seria.ToString().Substring(7)].Points.Last().Label = chartarray[chartarray.Count() - 1].ToString();


                //string[] split_key = keys[i].ToString().Split('_')
                //chart.Series[seria.ToString().Substring(7)].Points.Last;
            }



        }


        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void Chart_form_Load(object sender, EventArgs e)
        {

        }

        private void Chart_form_FormClosing(object sender, FormClosingEventArgs e)
        {
            CaptureForm.start_chart = false;
            Thread.Sleep(2000);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            stop_chart = true;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            set_chart_form = new Set_chart_form();
            set_chart_form.Show();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            view_point = trackBar1.Value;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
            
        private void textBox1_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.ShowDialog();

            textBox1.Text = openFileDialog1.FileName;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                warnform2.ShowDialog(); return;

            }
            else
            {
                extractedSamples = new Dictionary<string, List<ValueType>>();
                timearrayf = new List<double>();
                timearray_128f = new List<double>();
                timearray_256f = new List<double>();
                packetIndex = 0;
                ICaptureDevice device;
                string capFile = textBox1.Text;
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
                new PacketArrivalEventHandler((_sender, _e) => device_OnPacketArrival(this, _e, converter, extractedSamples));
                device.Capture();
                var keys2 = new List<string>(extractedSamples.Keys);

                dodawanie_checklistbox2(keys2, checkedListBox1);
                if (dic.Count == 0)
                {
                    parametr_definition.ForEach(x => keys.Add(x.ToString()));
                    parametr_definition.ForEach(x => occurence.Add(x.Occurrences.ToString()));
                    //dodawanie chasklistbox;
                    dic = keys.Zip(occurence, (k, v) => new { k, v })
                      .ToDictionary(x => x.k, x => x.v);
                }









            }
        }
        private static int packetIndex = 0;
        private static double last_time = 0;
        private void device_OnPacketArrival(object sender, CaptureEventArgs e, LiveConverter converter, Dictionary<string, List<ValueType>> extractedSamples)
        {
            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet)
            {
                //var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                //var ethernetPacket = (PacketDotNet.EthernetPacket)packet;


                packetIndex++;

                var result = converter.DecodePacket(e.Packet);
                temp_exSam = new Dictionary<string, List<ValueType>>();
                if (packetCount == 0)
                {
                    packetCount++;




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



                    });
                }

                    result.Samples.ForEach(s =>
                {
                    if (extractedSamples.ContainsKey(s.Key))
                        extractedSamples?[s.Key].Add(s.Value);


                    else
                        extractedSamples[s.Key] = new List<ValueType>() { s.Value };


                    if (temp_exSam.ContainsKey(s.Key))
                        temp_exSam?[s.Key].Add(s.Value);
                    else
                        temp_exSam[s.Key] = new List<ValueType>() { s.Value };
                });

                try
                {
                    if (timearrayf.Count == 0)
                    {
                        last_time = result.AcraTime.Value.TimeOfDay.TotalSeconds;
                        timearrayf.Add(0);

                    }
                    else
                    {
                        timearrayf.Add(result.AcraTime.Value.TimeOfDay.TotalSeconds - last_time);

                    }
                }
                catch { }
                try
                {
                    if (timearrayf.Count() > 1)
                    {
                        foreach (var time_divide in occurance[result.streamID])
                        {
                            //TimeSpan delta = TimeSpan.FromTicks((timearrayf[timearrayf.Count() - 1].Subtract(timearrayf[timearrayf.Count() - 2]).Ticks) / time_divide);
                            double delta = timearrayf[timearrayf.Count() - 1] - timearrayf[timearrayf.Count() - 2] / time_divide;
                            if (time_dictionatyf.ContainsKey(time_divide.ToString()))
                            {
                                time_dictionatyf?[time_divide.ToString()].Add(timearrayf[timearrayf.Count() - 2] + delta);
                                for (int i = 0; i < time_divide; i++)
                                {
                                    time_dictionatyf[time_divide.ToString()].Add(time_dictionatyf[time_divide.ToString()].Last() + delta);

                                }
                            }


                            else
                            {
                                time_dictionatyf[time_divide.ToString()] = new List<double>();
                                time_dictionatyf[time_divide.ToString()].Add(timearrayf[timearrayf.Count() - 2] + delta);
                                for (int i = 0; i < time_divide - 1; i++)
                                {
                                    time_dictionatyf[time_divide.ToString()].Add(time_dictionatyf[time_divide.ToString()].Last() + delta);

                                }


                            }
                        }
                    }
                }
                catch { }
                
                //if (timearrayf.Count() > 1)
                //{
                //    double delta = timearrayf[timearrayf.Count() - 1] - timearrayf[timearrayf.Count() - 2] / 128;
                //    timearray_128f.Add(timearrayf[timearrayf.Count() - 2] + delta);
                //    for (int i = 0; i < 127; i++)
                //    {
                //        timearray_128f.Add(timearray_128f.Last() + delta);
                //    }
                //}
                //if (timearrayf.Count() > 1)
                //{
                //    double delta = timearrayf[timearrayf.Count() - 1] - timearrayf[timearrayf.Count() - 2] / 128;
                //    timearray_256f.Add(timearrayf[timearrayf.Count() - 2] + delta);
                //    for (int i = 0; i < 255; i++)
                //    {
                //        timearray_256f.Add(timearray_256f.Last() + delta);
                //    }
                //}


                // int dataKeys = extractedSamples.Keys.Count();
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

        private void button2_Click(object sender, EventArgs e)
        {
            StartChart_file();
            Changechartfile(chart1);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {

        }
       
        private void chart1_AxisViewChanged(object sender, ViewEventArgs e)
        {
            double ymax = chart1.ChartAreas[0].AxisY.ScaleView.ViewMaximum;

            double ymin = chart1.ChartAreas[0].AxisY.ScaleView.ViewMinimum;
            if (axis_changed != 0 && Math.Abs(ymax - axis1) > 0 && Math.Abs(ymin - axis2) > 0)
            {
               
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
    }

}