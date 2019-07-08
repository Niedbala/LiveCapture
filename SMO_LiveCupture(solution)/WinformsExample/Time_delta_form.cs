using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformsExample
{
    public partial class Time_delta_form : Form
    {
        private Chart_form chart2;
        Dictionary<string, string> series_dictionary = new Dictionary<string, string>();
        public Time_delta_form(Chart_form chart)
        {
            InitializeComponent();
            chart2 = chart;
        }

        private void Time_delta_form_Load(object sender, EventArgs e)
        {
            object[] combo = new object[] { "M1", "M2", "M3", "M4" };
            comboBox1.Items.AddRange(combo);
            comboBox2.Items.AddRange(combo);
            comboBox3.Items.AddRange(combo);
            comboBox4.Items.AddRange(combo);


            var series_X = Chart_form.list_series;


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



            var series = name_list.ToArray();
            comboBox5.Items.AddRange(series);




        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null || comboBox2.SelectedItem == null)
            { MessageBox.Show("Brak markera o podanej nazwie.", "UWAGA!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            else
            {
                var num_1 = Int32.Parse(comboBox1.SelectedItem.ToString().Split('M')[1]);
                var num_2 = Int32.Parse(comboBox2.SelectedItem.ToString().Split('M')[1]);

                
                var xValue1 = (int)chart2.chart1.ChartAreas[0].AxisX.PixelPositionToValue(chart2.Xvalues[num_1 - 1]);

                var time1 = chart2.timebase[xValue1];
                var xValue2 = (int)chart2.chart1.ChartAreas[0].AxisX.PixelPositionToValue(chart2.Xvalues[num_2 - 1]);
                var time2 = chart2.timebase[xValue2];
                label4.Text = Math.Round(time2 - time1,6).ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox3.SelectedItem == null || comboBox4.SelectedItem == null || comboBox5.SelectedItem == null)
            { MessageBox.Show("Pole markera jest puste.", "UWAGA!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            else
            {
                if (comboBox5 == null) { MessageBox.Show("Wybierz serie.", "UWAGA!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; }
                var num_1 = Int32.Parse(comboBox3.SelectedItem.ToString().Split('M')[1]);
                var num_2 = Int32.Parse(comboBox4.SelectedItem.ToString().Split('M')[1]);
                var chosen = comboBox5.SelectedItem.ToString();
                var translated = series_dictionary[chosen];

                var name = translated.ToString().Substring(7);

                var xValue1 = (int)chart2.chart1.ChartAreas[0].AxisX.PixelPositionToValue(chart2.Xvalues[num_1 - 1]);
                
                var ampl1 = Convert.ToDouble(Chart_form.extractedSamples[name][xValue1]);
                var xValue2 = (int)chart2.chart1.ChartAreas[0].AxisX.PixelPositionToValue(chart2.Xvalues[num_2 - 1]);
                var ampl2 = Convert.ToDouble(Chart_form.extractedSamples[name][xValue2]);
                label7.Text = Math.Round(ampl2 - ampl1, 6).ToString();
            }
        }
    }
}
