using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinformsExample
{



    public partial class Marker_Setting : Form
    {
        private Chart_form chart;
        public Marker_Setting(Chart_form chart1)
        {
            InitializeComponent();
            chart = chart1;
        }
        List<ComboBox> comboboxy = new List<ComboBox>();
        Dictionary<string,string> series_dictionary = new Dictionary<string,string>();

      
        private void Marker_Setting_Load(object sender, EventArgs e)
        {
            //comboboxy = new List<ComboBox>() { comboBox1 , comboBox2, comboBox3, comboBox4, comboBox5, comboBox6, comboBox7, comboBox8, comboBox9, comboBox10, comboBox11, comboBox12};
            //List<object> series = new List<object>(Chart_form.list_series.ToList());
            var series_X = Chart_form.list_series;

            if (series_X.Count == 0) { MessageBox.Show("Choose series"); Close(); }
            else
            {
                List<string> name_list = new List<string>();

                for (int i = series_X.Count() - 1; i >= 0; i--)
                {
                    string[] split_key = series_X[i].ToString().Split('_');

                    var name = split_key[split_key.Count() - 1];

                    if (name.Contains("AnalogIn")) { name = name + "_" + split_key[1]; }
                    if (name.Contains("Analog")) { name = name + "_" + split_key[4]; }
                    if (name.Split('_').Last() == "B") { name = name + "_" + split_key[5]; }
                    name_list.Add(name);
                    series_dictionary.Add(name, series_X[i].ToString());
                    //checkedListBox.Items.Insert(0, name);
                }
                name_list.Add("");
                string cos = "";


                var series = name_list.ToArray();
               
                comboBox1.Items.AddRange(series);
                comboBox1.SelectedItem = Chart_form.MarkerSeries[0][0].Map_Series();
                comboBox2.Items.AddRange(series);
                comboBox2.SelectedItem = Chart_form.MarkerSeries[0][1].Map_Series();
                comboBox3.Items.AddRange(series);
                comboBox3.SelectedItem = Chart_form.MarkerSeries[0][2].Map_Series();
                comboBox4.Items.AddRange(series);
                comboBox4.SelectedItem = Chart_form.MarkerSeries[1][2].Map_Series();
                comboBox5.Items.AddRange(series);
                comboBox5.SelectedItem = Chart_form.MarkerSeries[1][1].Map_Series();
                comboBox6.Items.AddRange(series);
                comboBox6.SelectedItem = Chart_form.MarkerSeries[1][0].Map_Series();
                comboBox7.Items.AddRange(series);
                comboBox7.SelectedItem = Chart_form.MarkerSeries[2][2].Map_Series();
                comboBox8.Items.AddRange(series);
                comboBox8.SelectedItem = Chart_form.MarkerSeries[2][1].Map_Series();
                comboBox9.Items.AddRange(series);
                comboBox9.SelectedItem = Chart_form.MarkerSeries[2][0].Map_Series();
                comboBox10.Items.AddRange(series);
                comboBox10.SelectedItem = Chart_form.MarkerSeries[3][2].Map_Series();
                comboBox11.Items.AddRange(series);
                comboBox11.SelectedItem = Chart_form.MarkerSeries[3][1].Map_Series();
                comboBox12.Items.AddRange(series);
                comboBox12.SelectedItem = Chart_form.MarkerSeries[3][0].Map_Series();
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {

            string mrk1_series = "";
            try
            {
                var chosen = comboBox1.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[0][0] = translated;
            }
            catch
            {
                Chart_form.MarkerSeries[0][0] = "";}
            try
            {
                var chosen = comboBox2.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[0][1] =  translated;
            }
            catch { Chart_form.MarkerSeries[0][1] = ""; }
            try
            {
                var chosen = comboBox3.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[0][2] = translated;
            }
            catch { Chart_form.MarkerSeries[0][2] = ""; }


            string mrk2_series = "";
            try
            {
                var chosen = comboBox4.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[1][2] =  translated;
            }
            catch { Chart_form.MarkerSeries[1][2] = ""; }
            try
            {
                var chosen = comboBox5.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[1][1] = translated;
            }
            catch { Chart_form.MarkerSeries[1][1] = ""; }
            try
            {
                var chosen = comboBox6.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[1][0] =  translated;
            }
            catch { Chart_form.MarkerSeries[1][0] = ""; }


            string mrk3_series = "";
            try
            {
                var chosen = comboBox7.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[2][2] = translated;
            }
            catch { Chart_form.MarkerSeries[2][2] = ""; }
            try
            {
                var chosen = comboBox8.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[2][1] = translated;
            }
            catch { Chart_form.MarkerSeries[2][1] = ""; }
            try
            {
                var chosen = comboBox9.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[2][0] = translated;
            }
            catch { Chart_form.MarkerSeries[2][0] = ""; }

            string mrk4_series = "";
            try
            {
                var chosen = comboBox10.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[3][2] = translated;
            }
            catch { Chart_form.MarkerSeries[3][2] = ""; }
            try
            {
                var chosen = comboBox11.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[3][1] = translated;
            }
            catch { Chart_form.MarkerSeries[3][1] = ""; }
            try
            {
                var chosen = comboBox12.SelectedItem.ToString();
                var translated = series_dictionary[chosen];
                Chart_form.MarkerSeries[3][0] = translated;
            }
            catch { Chart_form.MarkerSeries[3][0] = ""; }

            //Chart_form.mrk_series[0] = mrk1_series;
            //Chart_form.mrk_series[1] = mrk2_series;
            //Chart_form.mrk_series[2] = mrk3_series;
            //Chart_form.mrk_series[3] = mrk4_series;
            if (chart.mrk_1.BackColor == System.Drawing.Color.Lime)
            { chart.marker_refresh(chart.mrk_lbl_1, 1); }
            if (chart.mrk_2.BackColor == System.Drawing.Color.Lime) { chart.marker_refresh(chart.mrk_lbl_2, 2); }
            if (chart.mrk_3.BackColor == System.Drawing.Color.Lime) { chart.marker_refresh(chart.mrk_lbl_3, 3); }
            if (chart.mrk_4.BackColor == System.Drawing.Color.Lime) { chart.marker_refresh(chart.mrk_lbl_4, 4); }
            Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
           
            
        }
    }
}
