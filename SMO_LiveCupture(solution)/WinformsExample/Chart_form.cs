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


namespace WinformsExample
{
    public partial class Chart_form : Form
    {
        private double[] ChartArray = new double[1000];
        private bool stop_chart = false;
        private Set_chart_form set_chart_form = new Set_chart_form();
        private Thread Time;
        public static int chart_width = 8000;
        public static bool show_last_value = true;
        public static bool show_legend = true;
        Warning_Form warnform = new Warning_Form();
        Warningform2 warnform2 = new Warningform2();
        public static int view_point = 0;
        public static Dictionary<string, List<ValueType>> extractedSamples = new Dictionary<string, List<ValueType>>();
        public static Dictionary<string, List<ValueType>> extractedPartOfSamples= new Dictionary<string, List<ValueType>>();
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
        public static Dictionary<int, double> last_times = new Dictionary<int, double>();
        public static int xValue;
        public static int series_iter;
        public static List<Series> list_series = new List<Series>();
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
                name_list.Add(name);
                checkedListBox.Items.Insert(0, name);
            }



        }

        private void dodawanie_checklistbox2(List<string> keys, CheckedListBox checkedListBox, Dictionary<string, List<ValueType>> extractedSamples)
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
                for (int i = 0; i <= co_eksportowac1.Count() - 1; i++)
                    if (co_eksportowac1[i])
                    {
                        var probkowanie_key = 1;
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
                Time = new Thread(new ThreadStart(this.TimeHandler));
                Time.IsBackground = true;
                Time.Start();
            }
            catch { }
        }

        private void StartChart_file(Chart chart1, CheckedListBox checkedListBox1, Dictionary<string, List<ValueType>> extractedSamples)
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
            list_series = chart1.Series.ToList();
            
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

            //var series = chart.Series.ToList();

            List<DateTime> timebase = new List<DateTime>();
            foreach (Series seria in list_series)
            {
                chart.Series[seria.ToString().Substring(7)].Points.Clear();

                string name = seria.ToString().Split('-')[1];
                var chartarray = CaptureForm.extractedSamples[seria.ToString().Substring(7)];
                int iter = 0;
                if (chartarray.Count() < chart_width)
                { iter = 0; }
                else { iter = chartarray.Count() - chart_width - 1; }
                var time_divide = dic1[name];
                //if (time_divide < 3) { timebase = CaptureForm.timearray; }
                // if (time_divide > 120 && time_divide < 130) { timebase = CaptureForm.timearray_128; }
                // if (time_divide > 250 && time_divide < 260) { timebase = CaptureForm.timearray_256; }
                timebase = CaptureForm.time_dictionaty[time_divide];

                var sw = new Stopwatch(); 
                sw.Start();
                /*
                for (int i = iter; i < chartarray.Count() - 1; i++)
                {

                    chart.Series[seria.ToString().Substring(7)].Points.AddXY(timebase[i], chartarray[i]);

                    //chart1.Series[seria.ToString().Substring(7)].XValueType(Data);
                }
                */
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
        public void Changechartfile(Chart chart, Dictionary<string, List<ValueType>> extractedSamples, Dictionary<string, List<double>> time_dictionatyf, Dictionary<string, string> dic)
        {



            var series = chart.Series.ToList();

            //List<DateTime> timebase = new List<DateTime>();
            List<double> timebase = new List<double>();
            foreach (Series seria in series)
            {
                chart.Series[seria.ToString().Substring(7)].Points.Clear();
                var strings = seria.ToString().Split('-');
                string name = strings[1];
               // if (strings.Count() == 2) { name = strings[1]; }
               // else if (strings.Count() == 3) { name = strings[1] + '-' + strings[2]; }
               // else if (strings.Count() == 4) { name = strings[1].Remove(strings[1].Length - 1); }

                var chartarray = extractedSamples[seria.ToString().Substring(7)];
                int iter = 0;
                string time_divide = "";
                try {  time_divide = dic[name]; }
                catch {
                    try { time_divide = dic[name.Remove(strings[1].Length - 1)]; }
                    catch {
                        try { time_divide = dic[strings[1] + '-' + strings[2].Remove(strings[2].Length - 1)]; }
                        catch { time_divide = dic[strings[1] + '-' + strings[2]]; } }
                }
                
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

                    dodawanie_checklistbox2(keys2, checkedListBox1, extractedSamples);
                    if (dic1.Count == 0)
                    {
                        parametr_definition.ForEach(x => keys.Add(x.ToString()));
                        parametr_definition.ForEach(x => occurence.Add(x.Occurrences.ToString()));
                        //dodawanie chasklistbox;
                        dic1 = keys.Zip(occurence, (k, v) => new { k, v })
                          .ToDictionary(x => x.k, x => x.v);
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
        private void device_OnPacketArrival(object sender, CaptureEventArgs e, LiveConverter converter, Dictionary<string, List<ValueType>> extractedSamples, Dictionary<string, List<double>> time_dictionatyf)
        {
            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet && e.Packet.Data.Count() > 1)
            {
                //var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                //var ethernetPacket = (PacketDotNet.EthernetPacket)packet;


                packetIndex++;

                var result = converter.DecodePacket(e.Packet);
                //result.IsSuccess
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
                    if (last_times[result.streamID] == 0.0)
                    {
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
            StartChart_file(chart1, checkedListBox1, extractedSamples);
            Changechartfile(chart1, extractedSamples, time_dictionatyf, dic1);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            //double yOffset = GetYOffset(chart1, e.X);
            Point mousePoint = new Point(e.X, e.Y);
            chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, false);
            chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, false);

            try {
                //chart1.ChartAreas[0].CursorX.SelectionStart
                
                xValue = (int)chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                double yValue = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                
                var name = list_series[series_iter].ToString().Substring(7);
                //.Single(s => s.Equals(max));
                var YVAL = extractedSamples[name][xValue];
                label4.Text = String.Concat(String.Concat(xValue.ToString(), " , "), YVAL.ToString());
                label4.Location = new Point(10, e.Y + 40);
            }
            catch { }

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
                    new PacketArrivalEventHandler((_sender, _e) => device_OnPacketArrival(this, _e, converter, extractedSamples2, time_dictionatyf2));
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


                }
                if (capFile[capFile.Count() - 3] == 't')
                {
                    //MessageBox.Show("odczyt tsv", "UWAGA!", MessageBoxButtons.OK);
                    text_file_reader(capFile, extractedSamples2, time_dictionatyf2, checkedListBox2, dic2);
                    return;
                }










            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StartChart_file(chart2,checkedListBox2,extractedSamples2);
            Changechartfile(chart2, extractedSamples2, time_dictionatyf2, dic2);
        }
        public int FileCount = 0;
        private int time;

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            //chart1.Series[keys[i]].IsXValueIndexed = true;
            double max = chart1.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
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
            
            foreach (var series in list_series)
            {
                name = series.ToString().Substring(7);
                
                for (int i = (int)Math.Round(min); i < max; i++)
                {
                    //extractedPartOfSamples.Add(extractedSamples[list_series[0].ToString().Substring(7)][i]);



                    if (extractedPartOfSamples.ContainsKey(name))
                        extractedPartOfSamples?[name].Add(extractedSamples[name][i]);
                    else
                        extractedPartOfSamples[name] = new List<ValueType>() { extractedSamples[name][i] };


                }
            }
            
            var strings = list_series[0].ToString().Split('-');
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
            extractedPartOfSamples["Time_with_occurence_" + time_divide] = new List<ValueType>();
            for (int i = (int)Math.Round(min); i < max; i++)
            {
                extractedPartOfSamples["Time_with_occurence_" + time_divide].Add(time_dictionatyf[time_divide][i]);
            }


                string headerLine ="";
            foreach (string key in extractedPartOfSamples.Keys)
            {
                headerLine = headerLine + key + "\t";
            }
            var keys_test = new List<string>(extractedPartOfSamples.Keys);
            var test = keys_test[0];
            int len = extractedPartOfSamples[test].Count;
            //FileCount++;
            //string dumpTextPath = "C:\\Users\\pniedbala\\Desktop\\test_data\\509_valid_file1\\data_read2.tsv";
            var directory = Path.GetDirectoryName(CaptureForm.path_savetsv);
            var name_file = directory +"\\" +comboBox1.Text.Split('.')[0] + "_" + System.DateTime.Today.ToString("MM-dd-yyyy") + "_file_1.tsv";
            while (File.Exists(name_file))
            {
                FileCount++;
                name_file = directory + "\\" + comboBox1.Text.Split('.')[0] + "_" + System.DateTime.Today.ToString("MM-dd-yyyy") + "_file_" + FileCount + ".tsv";
            }
            using (StreamWriter sw = File.CreateText(name_file))
            {
                sw.WriteLine(headerLine);
                for (int i = 0; i < len; i++)
                {
                    string valuesLine = "";
                    foreach (string key in extractedPartOfSamples.Keys)
                    {
                        valuesLine = valuesLine + extractedPartOfSamples[key][i] + "\t";
                    }
                    //time_dictionatyf[time_divide][i]
                    sw.WriteLine(valuesLine);
                }
            }
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

// var sortedDict = from entry in dic orderby entry.Value ascending select entry;
            dic1 = dic1.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            var time_dictionat_new = new Dictionary<string, List<ValueType>>();
            

            foreach (var x in time_dictionatyf)
            {
                foreach(var item in time_dictionatyf[x.Key])
                {
                    var it = (ValueType)item;

                    if (time_dictionat_new.ContainsKey("Time_with_occurence_" + x.Key))
                        time_dictionat_new?["Time_with_occurence_" + x.Key].Add(it);
                    else
                        time_dictionat_new["Time_with_occurence_" + x.Key] = new List<ValueType>() { it };
                }
            }


           
            
            var extractedSamplesWithTime = extractedSamples.Concat(time_dictionat_new).ToDictionary(x => x.Key, x => x.Value);
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
            var directory = Path.GetDirectoryName(CaptureForm.path_savetsv);
            var name_file = directory + "\\" + System.DateTime.Today.ToString("MM-dd-yyyy") + "_allParamsfile" + FileCount.ToString() + ".tsv";


            StringBuilder stringbuilder = new StringBuilder();
            stringbuilder.Append(headerLine + "\n");
            var keys = extractedSamplesSorted.Keys.ToList();
            for (int i = 1; i < len; i++)
            {
                foreach(string key in keys)
                {
                    try
                    {
                        stringbuilder.Append(extractedSamplesSorted[key][i].ToString() + "\t");
                    }
                    catch {
                        keys.Remove(key);
                        break; }
                    
                }
                stringbuilder.Append("\n");
            }

            File.AppendAllText(name_file, stringbuilder.ToString());




            var threads = new List<Thread>();
            for (int i = 0; i < 300; i++)
            {
                int j = i;
                var thread = new Thread(() =>
                {
                   // primeGenerator.GeneratePrimes(j * 1000, 1000);
                });
                thread.Start();
                threads.Add(thread);
            };
            foreach (var t in threads)
                t.Join();
            //using (StreamWriter sw = File.CreateText(name_file))
            //{
            //    sw.WriteLine(headerLine);
            //    for (int i = 0; i < len; i++)
            //    {
            //        string valuesLine = "";
            //        foreach (string key in extractedSamplesSorted.Keys)
            //        {
            //            try
            //            {
            //                valuesLine = valuesLine + extractedSamplesSorted[key][i] + "\t";
            //            }
            //            catch { break; }
            //        }
            //        sw.WriteLine(valuesLine);
            //    }
            //}
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            var list_series = chart1.Series.ToList();
            var name = list_series[0].ToString().Substring(7);

            var max = extractedSamples[name].Max();
            var max_index = extractedSamples[name].ToList().IndexOf(max);
            //chart1.ChartAreas[0].CursorX.SetCursorPosition()
            //chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, false);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            series_iter++;
            if (series_iter == list_series.Count()) { series_iter = 0; }
        }
    }
    }

