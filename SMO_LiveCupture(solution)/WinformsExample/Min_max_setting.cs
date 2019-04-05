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
    public partial class Min_max_setting : Form
    {
        public Min_max_setting(Chart_form chart_form)
        {
            InitializeComponent();
            chart_form2 = chart_form;
        }
        Chart_form chart_form2 = new Chart_form();
        
        Dictionary<string, string> series_dictionary = new Dictionary<string, string>();
        private Chart_form chart_form;

        private void Min_max_setting_Load(object sender, EventArgs e)
        {
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
            comboBox13.Items.AddRange(series);
            comboBox14.Items.AddRange(series);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Chart_form.find_max = true;

            if (comboBox13.SelectedItem == null)
            { MessageBox.Show("Brak seri o podanej nazwie.", "UWAGA!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            else
            {
                var chosen = comboBox13.SelectedItem.ToString();
                var translated = series_dictionary[chosen];

                var name = translated.ToString().Substring(7);

                //Chart_form.max_name = name;
                //var max = Chart_form.extractedSamples[name].Max();
                //var max_index = Chart_form.extractedSamples[name].ToList().IndexOf(max);
                chart_form2.Marker_max_generator( name);
                Close();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
           

            if (comboBox14.SelectedItem == null)
            { MessageBox.Show("Brak seri o podanej nazwie.", "UWAGA!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            else
            {
                var chosen = comboBox14.SelectedItem.ToString();
                var translated = series_dictionary[chosen];

                var name = translated.ToString().Substring(7);

                //Chart_form.max_name = name;
                //var max = Chart_form.extractedSamples[name].Max();
                //var max_index = Chart_form.extractedSamples[name].ToList().IndexOf(max);
                chart_form2.Marker_min_generator(name);
                Close();
            }
        }
    }
}
